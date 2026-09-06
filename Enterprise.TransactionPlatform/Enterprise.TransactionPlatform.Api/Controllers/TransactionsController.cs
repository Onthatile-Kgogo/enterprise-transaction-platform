using Enterprise.TransactionPlatform.Api.Contracts.Transactions;
using Enterprise.TransactionPlatform.Application.Common.Results;
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
    public sealed class TransactionsController : ControllerBase
    {
        private readonly SubmitTransactionHandler _submitHandler;
        private readonly GetTransactionByIdHandler _idHandler;
        private readonly GetTransactionByReferenceHandler _referenceHandler;
        private readonly UpdateTransactionStatusHandler _updateStatusHandler;
        private readonly SearchTransactionsHandler _searchHandler;

        public TransactionsController(SubmitTransactionHandler submitHandler, GetTransactionByIdHandler idHandler, GetTransactionByReferenceHandler referenceHandler,
            UpdateTransactionStatusHandler updateStatusHandler, SearchTransactionsHandler searchHandler)
        {
            ArgumentNullException.ThrowIfNull(submitHandler);
            ArgumentNullException.ThrowIfNull(idHandler);
            ArgumentNullException.ThrowIfNull(referenceHandler);
            ArgumentNullException.ThrowIfNull(updateStatusHandler);

            _submitHandler = submitHandler;
            _idHandler = idHandler;
            _referenceHandler = referenceHandler;
            _updateStatusHandler = updateStatusHandler;
            _searchHandler = searchHandler;
        }

        [HttpPost]
        [ProducesResponseType(typeof(SubmitTransactionResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SubmitAsync([FromBody] SubmitTransactionCommand command, CancellationToken cancellationToken)
        {
            var result = await _submitHandler.HandleAsync(command, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Created($"/api/transactions/{result.Value!.TransactionId}", result.Value);
        }


        [HttpGet("{transactionId:guid}")]
        [ProducesResponseType(typeof(GetTransactionByIdResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken)
        {
            var query = new GetTransactionByIdQuery(transactionId);
            var result = await _idHandler.HandleAsync(query, cancellationToken);

            if (result is null)
                return NotFound();

            return Ok(result);
        }


        [HttpGet("reference/{reference}")]
        [ProducesResponseType(typeof(GetTransactionByIdResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByReferenceAsync(string reference, CancellationToken cancellationToken)
        {
            var query = new GetTransactionByReferenceQuery(reference);
            var result = await _referenceHandler.HandleAsync(query, cancellationToken);

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
            var result = await _updateStatusHandler.HandleAsync(command, cancellationToken);

            return Ok(result);
        }


        [HttpGet("search")]
        [ProducesResponseType(typeof(SearchTransactionsResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApplicationError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchAsync([FromQuery] SearchTransactionsQuery query, CancellationToken cancellationToken)
        {
            var result = await _searchHandler.HandleAsync(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }
    }
}
