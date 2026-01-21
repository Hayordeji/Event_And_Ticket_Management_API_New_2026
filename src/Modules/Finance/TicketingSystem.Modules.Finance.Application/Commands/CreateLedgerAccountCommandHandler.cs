using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Application.DTOs;
using TicketingSystem.Modules.Finance.Domain.Entities;
using TicketingSystem.Modules.Finance.Domain.Repositories;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Finance.Application.Commands
{
    public class CreateLedgerAccountCommandHandler : IRequestHandler<CreateLedgerAccountCommand, Result<LedgerAccountResponse>>
    {
        private readonly ILedgerAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateLedgerAccountCommandHandler(
            ILedgerAccountRepository accountRepository,
            IUnitOfWork unitOfWork)
        {
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<LedgerAccountResponse>> Handle(CreateLedgerAccountCommand request, CancellationToken cancellationToken)
        {
            // Check if account code already exists
            var codeExists = await _accountRepository.AccountCodeExistsAsync(request.AccountCode, cancellationToken);
            if (codeExists)
                throw new ConflictException($"Account with code '{request.AccountCode}' already exists");

            // Create account
            var accountResult = LedgerAccount.Create(
                request.AccountName,
                request.AccountCode,
                request.AccountType,
                request.Currency,
                request.Description);

            if (accountResult.IsFailure)
                return Result.Failure<LedgerAccountResponse>(accountResult.Error);

            var account = accountResult.Value;

            // Save account
            await _accountRepository.AddAsync(account, cancellationToken);
            //await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new LedgerAccountResponse(
                account.Id,
                account.AccountName,
                account.AccountCode,
                account.AccountType,
                account.CurrentBalance.Amount,
                account.CurrentBalance.Currency,
                account.Description,
                account.IsActive,
                account.CreatedAt);

            return Result.Success(response);
        }
    }
}
