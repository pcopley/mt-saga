using System;

namespace Messages.StateMachine
{
    public class CartRemoved
    {
        public Guid CartId { get; set; }

        public string UserName { get; set; }
    }
}