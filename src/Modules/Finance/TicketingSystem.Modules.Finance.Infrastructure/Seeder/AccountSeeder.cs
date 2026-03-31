using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TicketingSystem.Modules.Finance.Domain.Entities;
using TicketingSystem.Modules.Finance.Domain.Enums;
using TicketingSystem.Modules.Finance.Infrastructure.Persistence;
using TicketingSystem.SharedKernel.Finance;

namespace TicketingSystem.Modules.Finance.Infrastructure.Seeder
{
    public class AccountSeeder
    {
        private readonly FinanceDbContext _dbContext;
        private readonly ILogger<AccountSeeder> _logger;
        public AccountSeeder(FinanceDbContext dbContext, ILogger<AccountSeeder> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }


        public async Task Seed()
        {

            var newAccounts = new List<LedgerAccount>();
            var platformAccountCode = AccountCodeValues.Platform;
            var account1 = LedgerAccount.Create(
               accountCode: platformAccountCode,
               accountName: $"Platform Revenue",
               accountType: AccountType.Revenue,
               currency: "NGN",
               description: $"Platform commission revenue from ticket sales");


            // Check if account already exists (fast path - most common case)
            var platformexists = await _dbContext.LedgerAccounts
                .AnyAsync(a => a.AccountCode == platformAccountCode);

            if (platformexists)
            {
                _logger.LogInformation("Platform Account already exists");
            }
            else
            {
                await _dbContext.LedgerAccounts.AddAsync(account1.Value);
            }

            var paymentGatewayAccountCode = AccountCodeValues.PaymentGateWay;
            var account2 = LedgerAccount.Create(
               accountCode: paymentGatewayAccountCode,
               accountName: $"Payment Gateway Settlement",
               accountType: AccountType.Asset,
               currency: "NGN",
               description: $"Funds from payment gateway");

            // Check if account already exists (fast path - most common case)
            var gatewayExists = await _dbContext.LedgerAccounts
                .AnyAsync(a => a.AccountCode == paymentGatewayAccountCode);

            if (platformexists)
            {
                _logger.LogInformation("Gateway Account already exists");
            }
            else
            {
                await _dbContext.LedgerAccounts.AddAsync(account2.Value);
            }


            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Successfully Seeded default accounts");




        }
    }
}
