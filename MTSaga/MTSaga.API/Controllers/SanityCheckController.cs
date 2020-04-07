using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Messages;
using Microsoft.AspNetCore.Mvc;
using MTSaga.API.Database;

namespace MTSaga.API.Controllers
{
    [Route("api/sanity-check")]
    [ApiController]
    public class SanityCheckController : ControllerBase
    {
        private readonly IRequestClient<ISubmitOrder, IOrderAccepted> _requestClient;

        public SanityCheckController(IRequestClient<ISubmitOrder, IOrderAccepted> requestClient)
        {
            _requestClient = requestClient;
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> RequestResponse(string id, CancellationToken ct)
        {
            // We would do this in the initial consumer, typically. But we'll just do it here to
            // make the code cleaner
            await using var db = new MTSagaContext();

            db.Orders.Add(new Order
            {
                OrderId = id,
                Status = "PENDING",
                CreatedUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync(ct);

            var order = await db.Orders.FindAsync(id);

            try
            {
                var result = await _requestClient.Request(new { OrderId = id }, ct);

                order.Status = "ACCEPTED";
                order.UpdatedUtc = DateTime.UtcNow;

                await db.SaveChangesAsync(ct);

                return Accepted(result.OrderId);
            }
            catch (RequestTimeoutException)
            {
                return StatusCode((int)HttpStatusCode.RequestTimeout);
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}