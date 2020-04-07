using CartTracking;
using System;
using System.Collections.Generic;
using System.Text;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CartTrackingService
{
    public class ShoppingCartMap : SagaClassMap<ShoppingCart>
    {
        protected override void Configure(EntityTypeBuilder<ShoppingCart> entity, ModelBuilder modelBuilder)
        {
            entity.Property(x => x.CurrentState).HasMaxLength(64);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.Property(x => x.Updated);

            entity.Property(x => x.UserName);
            entity.Property(x => x.ExpirationId);
            entity.Property(x => x.CorrelationId);
            entity.Property(x => x.OrderId);
        }
    }
}