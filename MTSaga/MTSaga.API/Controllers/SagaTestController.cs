using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Automatonymous;
using CartTracking;
using MassTransit;
using Messages.StateMachine;
using Microsoft.AspNetCore.Mvc;

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
                await _machine.RaiseEvent(_cart, _machine.ItemAdded, new CartItemAdded
                {
                    Timestamp = DateTime.UtcNow,
                    UserName = username
                }, ct);

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