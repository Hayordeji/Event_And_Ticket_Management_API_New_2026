using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Application.DTOs;
using TicketingSystem.Modules.Finance.Domain.Repositories;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Finance.Application.Queries
{
    public class GetTransactionByReferenceQueryHandler : IRequestHandler<GetTransactionByReferenceQuery, Result<TransactionResponse>>
    {
        private readonly ILedgerTransactionRepository _transactionRepository;

        public GetTransactionByReferenceQueryHandler(ILedgerTransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<Result<TransactionResponse>> Handle(GetTransactionByReferenceQuery request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepository.GetByReferenceAsync(
                request.ReferenceType,
                request.ReferenceId,
                cancellationToken);

            if (transaction == null)
                throw new NotFoundException($"Transaction for {request.ReferenceType}", request.ReferenceId);

            var response = new TransactionResponse(
                transaction.Id,
                transaction.ReferenceType,
                transaction.ReferenceId,
                transaction.Description,
                transaction.OccurredAt,
                transaction.IsPosted,
                transaction.PostedAt,
                transaction.Entries.Select(e => new EntryResponse(
                    e.Id,
                    e.Account.AccountName,
                    e.Account.AccountCode,
                    e.Amount.Amount,
                    e.Amount.Currency,
                    e.EntryType.ToString(),
                    e.Description)).ToList());

            return Result.Success(response);
        }
    }
}
