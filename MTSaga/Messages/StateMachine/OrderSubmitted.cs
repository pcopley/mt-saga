using System;
using System.Collections.Generic;
using System.Text;

namespace Messages.StateMachine
{
    public class OrderSubmitted
    {
        public Guid CartId { get; set; }

        public Guid OrderId { get; set; }

        public DateTime Timestamp { get; set; }

        public string UserName { get; set; }
    }
}