using System;
using Automatonymous;

namespace CartTracking
{
    public class ShoppingCart : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }

        public DateTime Created { get; set; }

        public string CurrentState { get; set; }

        public Guid? ExpirationId { get; set; }

        public Guid? OrderId { get; set; }

        public byte[] RowVersion { get; set; }

        public DateTime Updated { get; set; }

        public string UserName { get; set; }

        public ShoppingCart()
        {
        }

        public ShoppingCart(Guid id)
        {
            CorrelationId = id;
        }
    }
}