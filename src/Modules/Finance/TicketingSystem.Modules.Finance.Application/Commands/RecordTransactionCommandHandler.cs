using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Application.DTOs;
using TicketingSystem.Modules.Finance.Domain.Entities;
using TicketingSystem.Modules.Finance.Domain.Repositories;
using TicketingSystem.Modules.Finance.Domain.ValueObjects;
using TicketingSystem.Modules.Finance.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Finance.Application.Commands
{
    public class RecordTransactionCommandHandler : IRequestHandler<RecordTransactionCommand, Result<TransactionResponse>>
    {
        private readonly ILedgerTransactionRepository _transactionRepository;
        private readonly ILedgerAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly FinanceDbContext _context;


        public RecordTransactionCommandHandler(
            ILedgerTransactionRepository transactionRepository,
            ILedgerAccountRepository accountRepository,
            IUnitOfWork unitOfWork,
            FinanceDbContext context)
        {
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<Result<TransactionResponse>> Handle(RecordTransactionCommand request, CancellationToken cancellationToken)
        {
            // Check if transaction already exists for this reference (idempotency)
            var exists = await _transactionRepository.TransactionExistsForReferenceAsync(
                request.ReferenceType,
                request.ReferenceId,
                cancellationToken);

            if (exists)
                throw new ConflictException($"Transaction already exists for {request.ReferenceType} with ID {request.ReferenceId}");

            // Create transaction
            var transactionResult = LedgerTransaction.Create(
                request.ReferenceType,
                request.ReferenceId,
                request.Description);

            if (transactionResult.IsFailure)
                return Result.Failure<TransactionResponse>(transactionResult.Error);

            var transaction = transactionResult.Value;

            // Add entries
            foreach (var entryRequest in request.Entries)
            {
                // Get account by code
                var account = await _accountRepository.GetByAccountCodeAsync(entryRequest.AccountCode, cancellationToken);
                if (account == null)
                    throw new NotFoundException("LedgerAccount", entryRequest.AccountCode);

                // Create money value object
                var moneyResult = Money.Create(entryRequest.Amount, account.CurrentBalance.Currency);
                if (moneyResult.IsFailure)
                    return Result.Failure<TransactionResponse>(moneyResult.Error);

                // Add entry to transaction
                var addResult = entryRequest.EntryType == Domain.Enums.EntryType.Debit
                    ? transaction.AddDebit(account.Id, moneyResult.Value, entryRequest.Description)
                    : transaction.AddCredit(account.Id, moneyResult.Value, entryRequest.Description);

                if (addResult.IsFailure)
                    return Result.Failure<TransactionResponse>(addResult.Error);
            }
            
            // Validate transaction balances
            var validationResult = transaction.Validate();
            if (validationResult.IsFailure)
                return Result.Failure<TransactionResponse>(validationResult.Error);

            // Post transaction
            var postResult = transaction.Post();
            if (postResult.IsFailure)
                return Result.Failure<TransactionResponse>(postResult.Error);

            // Save transaction
            await _transactionRepository.AddAsync(transaction, cancellationToken);

            // Update account balances
            foreach (var entry in transaction.Entries)
            {
                var account = await _accountRepository.GetByIdAsync(entry.AccountId, cancellationToken);
                if (account == null)
                    throw new NotFoundException("LedgerAccount", entry.AccountId);

                account.UpdateBalance(entry.Amount, entry.EntryType);
            }

            // Save all changes in one transaction
            await _context.SaveChangesAsync(cancellationToken);

            // Build response
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
