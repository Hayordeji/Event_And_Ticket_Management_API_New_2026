using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel
{
    ///<summary>
/// Generic repository interface for data access
/// </summary>
/// <typeparam name="T">Entity type that inherits from AggregateRoot</typeparam>
    public interface IRepository<T> where T : AggregateRoot
    {
        /// <summary>
        /// Get entity by ID
        /// </summary>
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all entities
        /// </summary>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Add new entity
        /// </summary>
        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update existing entity
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Delete entity
        /// </summary>
        void Delete(T entity);

        /// <summary>
        /// Check if entity exists
        /// </summary>
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
