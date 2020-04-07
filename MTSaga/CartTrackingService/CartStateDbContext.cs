using System;
using System.Collections.Generic;
using System.Text;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore;

namespace CartTrackingService
{
    public class CartStateDbContext : SagaDbContext
    {
        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get
            {
                yield return new ShoppingCartMap();
            }
        }

        public CartStateDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}