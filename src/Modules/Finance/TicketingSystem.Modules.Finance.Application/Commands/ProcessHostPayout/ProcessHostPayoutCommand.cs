using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Application.Commands.ProcessHostPayout
{
    public record ProcessHostPayoutCommand(
    Guid HostId,
    string BankAccountNumber,
    string BankCode,
    string Reason
) : IRequest<Result<string>>;
}
