using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel
{
     ///<summary>
    /// Unit of Work pattern for managing transactions
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Save all changes to the database
        /// Returns the number of affected rows
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begin a database transaction
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rollback the current transaction
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
