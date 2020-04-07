using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MTSaga.API.Database
{
    public class Order
    {
        public DateTime CreatedUtc { get; set; }

        [Key]
        public string OrderId { get; set; }

        public string Status { get; set; }

        public DateTime? UpdatedUtc { get; set; }
    }
}