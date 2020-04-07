using System;
using MassTransit;
using Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Saga;
using MassTransit.Saga;
using Automatonymous;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using CartTracking;
using CartTrackingService;
using MassTransit.EntityFrameworkCoreIntegration.Saga.Context;
using Messages.StateMachine;
using Newtonsoft.Json;

namespace MTSaga.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();

            app.UseSwaggerUI(swag =>
            {
                swag.SwaggerEndpoint("/swagger/v1/swagger.json", "MassTransit Saga POC v1");
                swag.RoutePrefix = string.Empty;
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            var sagaStateMachine = new ShoppingCartStateMachine();
            var repository = new InMemorySagaRepository<ShoppingCart>();

            var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(
                    new Uri("amqp://csrycfsz:x1ukAcpwS8Bm-jDi-lg_J2ZaC4jzKZwg@buffalo.rmq.cloudamqp.com/csrycfsz"),
                    h => { });

                cfg.ReceiveEndpoint("shopping_cart_state", e =>
                {
                    e.StateMachineSaga(sagaStateMachine, repository);
                });
            });

            services.AddMassTransit(cfg =>
            {
                cfg.AddSagaStateMachine<ShoppingCartStateMachine, ShoppingCart>()
                    .EntityFrameworkRepository(r =>
                    {
                        r.ConcurrencyMode = ConcurrencyMode.Optimistic;

                        r.AddDbContext<DbContext, CartStateDbContext>((provider, builder) =>
                        {
                            builder.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=MTSaga;", m =>
                            {
                                m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                                m.MigrationsHistoryTable($"__{nameof(CartStateDbContext)}");
                            });
                        });
                    });
            });

            services.AddScoped<ShoppingCart>();
            services.AddSingleton<ShoppingCartStateMachine>();

            services.AddSingleton<IPublishEndpoint>(bus);
            services.AddSingleton<ISendEndpointProvider>(bus);
            services.AddSingleton<IBus>(bus);

            var timeout = TimeSpan.FromSeconds(10);

            var serviceAddress =
                new Uri(
                    "amqp://csrycfsz:x1ukAcpwS8Bm-jDi-lg_J2ZaC4jzKZwg@buffalo.rmq.cloudamqp.com/csrycfsz/order-service");

            services.AddScoped<IRequestClient<ISubmitOrder, IOrderAccepted>>(x =>
                new MessageRequestClient<ISubmitOrder, IOrderAccepted>(x.GetRequiredService<IBus>(), serviceAddress,
                    timeout, timeout));

            bus.Start();

            services.AddSwaggerGen(swag =>
            {
                swag.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "MassTransit Saga POC",
                    Version = "v1"
                });
            });
        }
    }
}