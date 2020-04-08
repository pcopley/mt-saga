using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Automatonymous;
using CartTracking;
using CartTrackingService;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration.Saga;
using MassTransit.EntityFrameworkCoreIntegration.Saga.Context;
using MassTransit.Initializers;
using Messages.StateMachine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MTSaga.API.Controllers
{
    [Route("api/saga-test")]
    [ApiController]
    public class SagaTestController : ControllerBase
    {
        private ShoppingCart _cart { get; set; }

        private ShoppingCartStateMachine _machine { get; set; }

        public SagaTestController(ShoppingCart cart, ShoppingCartStateMachine machine)
        {
            _cart = cart;
            _machine = machine;
        }

        [HttpPost("{username}")]
        public async Task<IActionResult> NewCart(string username, CancellationToken ct)
        {
            try
            {
                var dbOptionsBuilder = new DbContextOptionsBuilder()
                    .UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=MTSaga;")
                    .EnableSensitiveDataLogging();

                var sagaRepo =
                    EntityFrameworkSagaRepository<ShoppingCart>.CreateOptimistic(() =>
                        new CartStateDbContext(dbOptionsBuilder.Options));

                var message = await MessageInitializerCache<CartItemAdded>.InitializeMessage(new
                {
                    Timestamp = DateTime.UtcNow,
                    UserName = username
                }, ct);

                await _machine.RaiseEvent(_cart, _machine.ItemAdded, message, ct);

                return Ok();
            }
            catch (RequestTimeoutException)
            {
                return StatusCode((int)HttpStatusCode.RequestTimeout);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}