using Enterprise.TransactionPlatform.Application.Transactions.GetById;
using Enterprise.TransactionPlatform.Application.Transactions.GetByReference;
using Enterprise.TransactionPlatform.Application.Transactions.Submit;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.TransactionPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly SubmitTransactionHandler submitHandler;
        private readonly GetTransactionByIdHandler idHandler;
        private readonly GetTransactionByReferenceHandler referenceHandler;

        public TransactionsController(SubmitTransactionHandler submitHandler, GetTransactionByIdHandler idHandler, GetTransactionByReferenceHandler referenceHandler)
        {
            ArgumentNullException.ThrowIfNull(submitHandler);
            ArgumentNullException.ThrowIfNull(idHandler);
            ArgumentNullException.ThrowIfNull(referenceHandler);

            this.submitHandler = submitHandler;
            this.idHandler = idHandler;
            this.referenceHandler = referenceHandler;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAsync([FromBody] SubmitTransactionCommand command, CancellationToken cancellationToken)
        {
            var result = await submitHandler.HandleAsync(command, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Created($"/api/transactions/{result.Value!.TransactionId}", result.Value);
        }

        [HttpGet("{transactionId:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken)
        {
            var query = new GetTransactionByIdQuery(transactionId);
            var result = await idHandler.HandleAsync(query, cancellationToken);

            if (result is null)
                return NotFound();

            return Ok(result);
        }


        [HttpGet("reference/{reference}")]
        public async Task<IActionResult> GetByReferenceAsync(string reference, CancellationToken cancellationToken)
        {
            var query = new GetTransactionByReferenceQuery(reference);
            var result = await referenceHandler.HandleAsync(query, cancellationToken);

            if (result is null)
                return NotFound();

            return Ok(result);
        }
    }
}
