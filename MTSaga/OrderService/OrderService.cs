using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Messages;

namespace OrderService
{
    public class OrderService : BackgroundService
    {
        private readonly IBusControl _bus;

        public OrderService()
        {
            _bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri("amqp://csrycfsz:p8ww82_xYDGyobflWncpQnsf419KiH4c@buffalo.rmq.cloudamqp.com/csrycfsz"), h => { });

                cfg.ReceiveEndpoint("order-service", e =>
                {
                    e.Handler<ISubmitOrder>(c => c.RespondAsync<IOrderAccepted>(new
                    {
                        c.Message.OrderId
                    }));
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