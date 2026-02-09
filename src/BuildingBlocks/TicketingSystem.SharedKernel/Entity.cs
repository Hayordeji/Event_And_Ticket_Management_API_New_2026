using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel
{
    ///<summary>
    /// Base class for all entities with identity
    /// </summary>
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        /// <summary>
        /// When the entity was created (UTC)
        /// </summary>
        public DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// When the entity was last updated (UTC)
        /// </summary>
        public DateTime? UpdatedAt { get; protected set; }

        /// <summary>
        /// User ID who created this entity
        /// </summary>
        public Guid? CreatedBy { get; protected set; }

        /// <summary>
        /// User ID who last updated this entity
        /// </summary>
        public Guid? UpdatedBy { get; protected set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; protected set; }

        /// <summary>
        /// When the entity was soft deleted (UTC)
        /// </summary>
        public DateTime? DeletedAt { get; protected set; }

        /// <summary>
        /// User ID who deleted this entity
        /// </summary>
        public Guid? DeletedBy { get; protected set; }


        protected Entity()
        {
            //Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
        }

        protected Entity(Guid id)
        {
            Id = id;
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
        }

        /// <summary>
        /// Mark entity as deleted (soft delete)
        /// </summary>
        public void MarkAsDeleted(Guid? deletedBy = null)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
        }

        /// <summary>
        /// Mark entity as updated
        /// </summary>
        public void MarkAsUpdated(Guid? updatedBy = null)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Entity other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Entity? a, Entity? b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity? a, Entity? b)
        {
            return !(a == b);
        }
    }
}