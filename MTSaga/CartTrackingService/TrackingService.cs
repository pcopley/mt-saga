using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CartTracking;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration.Saga;
using MassTransit.EntityFrameworkCoreIntegration.Saga.Context;
using MassTransit.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace CartTrackingService
{
    public class TrackingService : BackgroundService
    {
        private readonly IBusControl _bus;

        public TrackingService()
        {
            var machine = new ShoppingCartStateMachine();

            var dbOptionsBuilder = new DbContextOptionsBuilder()
                .UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=MTSaga;")
                .EnableSensitiveDataLogging();

            var repository = EntityFrameworkSagaRepository<ShoppingCart>.CreateOptimistic(() =>
                new CartStateDbContext(dbOptionsBuilder.Options));

            _bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri("amqp://csrycfsz:p8ww82_xYDGyobflWncpQnsf419KiH4c@buffalo.rmq.cloudamqp.com/csrycfsz"),
                    h => { });

                cfg.ReceiveEndpoint("shopping_cart_state", e =>
                {
                    var temp = "test";

                    e.StateMachineSaga(machine, repository);
                });
            });
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(base.StopAsync(cancellationToken), _bus.StopAsync(cancellationToken));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _bus.StartAsync(stoppingToken);
        }
    }
}