using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CartTracking;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration.Saga.Context;
using MassTransit.Saga;
using MassTransit.Scheduling;
using Microsoft.Extensions.Hosting;

namespace CartTrackingService
{
    public class TrackingService : BackgroundService
    {
        private readonly IBusControl _bus;

        // needed w/ dotnetcore?
        private readonly BusHandle _busHandle;

        private readonly ShoppingCartStateMachine _machine;

        private readonly Lazy<ISagaRepository<ShoppingCart>> _repository;

        private readonly IMessageScheduler _scheduler;

        public TrackingService()
        {
            _machine = new ShoppingCartStateMachine();

            //_repository = new SagaRepository<ShoppingCart>
            //{
            //};

            _bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(
                    new Uri("amqp://csrycfsz:x1ukAcpwS8Bm-jDi-lg_J2ZaC4jzKZwg@buffalo.rmq.cloudamqp.com/csrycfsz"),
                    h => { });

                //cfg.ReceiveEndpoint("shopping_cart_state", e =>
                //{
                //    e.StateMachineSaga(_machine, _repository.Value);
                //});
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