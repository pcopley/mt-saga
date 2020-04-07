using System;
using System.Collections.Generic;
using System.Text;

namespace Messages.StateMachine
{
    public class CartItemAdded
    {
        public DateTime Timestamp { get; set; }

        public string UserName { get; set; }
    }
}