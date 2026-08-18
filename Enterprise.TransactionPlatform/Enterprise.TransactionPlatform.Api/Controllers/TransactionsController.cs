using Enterprise.TransactionPlatform.Api.Contracts.Transactions;
using Enterprise.TransactionPlatform.Application.Transactions.GetById;
using Enterprise.TransactionPlatform.Application.Transactions.GetByReference;
using Enterprise.TransactionPlatform.Application.Transactions.Search;
using Enterprise.TransactionPlatform.Application.Transactions.Submit;
using Enterprise.TransactionPlatform.Application.Transactions.UpdateStatus;
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
        private readonly UpdateTransactionStatusHandler updateStatusHandler;
        private readonly SearchTransactionsHandler searchHandler;

        public TransactionsController(SubmitTransactionHandler submitHandler, GetTransactionByIdHandler idHandler, GetTransactionByReferenceHandler referenceHandler,
            UpdateTransactionStatusHandler updateStatusHandler, SearchTransactionsHandler searchHandler)
        {
            ArgumentNullException.ThrowIfNull(submitHandler);
            ArgumentNullException.ThrowIfNull(idHandler);
            ArgumentNullException.ThrowIfNull(referenceHandler);
            ArgumentNullException.ThrowIfNull(updateStatusHandler);

            this.submitHandler = submitHandler;
            this.idHandler = idHandler;
            this.referenceHandler = referenceHandler;
            this.updateStatusHandler = updateStatusHandler;
            this.searchHandler = searchHandler;
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

        [HttpPatch("{transactionId:guid}/status")]
        [ProducesResponseType(typeof(UpdateTransactionStatusResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateStatusAsync(Guid transactionId, [FromBody] UpdateTransactionStatusRequest request, CancellationToken cancellationToken)
        {
            var command = new UpdateTransactionStatusCommand(transactionId, request.Status);
            var result = await updateStatusHandler.HandleAsync(command, cancellationToken);

            return Ok(result);
        }


        [HttpGet("search")]
        public async Task<IActionResult> SearchAsync([FromQuery] SearchTransactionsQuery query, CancellationToken cancellationToken)
        {
            var result = await searchHandler.HandleAsync(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }
    }
}
