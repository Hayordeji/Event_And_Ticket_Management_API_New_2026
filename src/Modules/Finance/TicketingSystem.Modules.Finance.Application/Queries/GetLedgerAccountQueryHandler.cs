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
    public class GetLedgerAccountQueryHandler : IRequestHandler<GetLedgerAccountQuery, Result<LedgerAccountResponse>>
    {
        private readonly ILedgerAccountRepository _accountRepository;

        public GetLedgerAccountQueryHandler(ILedgerAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<Result<LedgerAccountResponse>> Handle(GetLedgerAccountQuery request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByAccountCodeAsync(request.AccountCode, cancellationToken);
            if (account == null)
                throw new NotFoundException("LedgerAccount", request.AccountCode);

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
