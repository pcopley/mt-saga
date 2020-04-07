using Automatonymous;
using Messages.StateMachine;
using System;

namespace CartTracking
{
    public class ShoppingCartStateMachine : MassTransitStateMachine<ShoppingCart>
    {
        public State Active { get; set; }

        public Event<CartItemAdded> ItemAdded { get; set; }

        public State Ordered { get; set; }

        public Event<OrderSubmitted> Submitted { get; set; }

        public ShoppingCartStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => ItemAdded,
                x => x.CorrelateBy(cart => cart.UserName, context => context.Message.UserName)
                    .SelectId(context => Guid.NewGuid()));

            Event(() => Submitted, x => x.CorrelateById(context => context.Message.CartId));

            Initially(
                When(ItemAdded)
                    .Then(context =>
                    {
                        context.Instance.Created = context.Data.Timestamp;
                        context.Instance.Updated = context.Data.Timestamp;
                        context.Instance.UserName = context.Data.UserName;
                        context.Instance.CorrelationId = Guid.NewGuid();
                    })
                    .ThenAsync(context =>
                        Console.Out.WriteLineAsync(
                            $"Item added: {context.Data.UserName} to {context.Instance.CorrelationId}"))
                    .TransitionTo(Active)
            );

            During(Active,
                When(Submitted)
                    .Then(context =>
                    {
                        if (context.Data.Timestamp > context.Instance.Updated)
                            context.Instance.Updated = context.Data.Timestamp;

                        context.Instance.OrderId = context.Data.OrderId;
                    })
                    .ThenAsync(context =>
                        Console.Out.WriteLineAsync(
                            $"Cart submitted: {context.Data.UserName} to {context.Instance.CorrelationId}"))
                    .TransitionTo(Ordered)
                    .Finalize(),
                When(ItemAdded)
                    .Then(context =>
                    {
                        if (context.Data.Timestamp > context.Instance.Updated)
                            context.Instance.Updated = context.Data.Timestamp;
                    })
                    .ThenAsync(context =>
                        Console.Out.WriteLineAsync(
                            $"Item added: {context.Data.UserName} to {context.Instance.CorrelationId}"))
            );

            SetCompletedWhenFinalized();
        }
    }
}