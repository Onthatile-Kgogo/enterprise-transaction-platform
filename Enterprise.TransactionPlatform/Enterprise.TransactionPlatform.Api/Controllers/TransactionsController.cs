using Enterprise.TransactionPlatform.Application.Transactions.Submit;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.TransactionPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly SubmitTransactionHandler handler;

        public TransactionsController(SubmitTransactionHandler handler)
        {
            ArgumentNullException.ThrowIfNull(handler);
            this.handler = handler;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAsync(
        [FromBody] SubmitTransactionCommand command,
        CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Created($"/api/transactions/{result.Value!.TransactionId}", result.Value);
        }
    }
}
