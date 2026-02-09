using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Domain.ValueObjects
{
    public class Venue : ValueObject
    {
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string Country { get; private set; }
        public string? PostalCode { get; private set; }
        public decimal? Latitude { get; private set; }
        public decimal? Longitude { get; private set; }

        // EF Core constructor
        private Venue() { }

        private Venue(
            string name,
            string address,
            string city,
            string state,
            string country,
            string? postalCode,
            decimal? latitude,
            decimal? longitude)
        {
            Name = name;
            Address = address;
            City = city;
            State = state;
            Country = country;
            PostalCode = postalCode;
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>
        /// Factory method to create a venue
        /// </summary>
        public static Result<Venue> Create(
            string name,
            string address,
            string city,
            string state,
            string country,
            string? postalCode = null,
            decimal? latitude = null,
            decimal? longitude = null)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<Venue>("Venue name is required");

            if (name.Length > 200)
                return Result.Failure<Venue>("Venue name cannot exceed 200 characters");

            if (string.IsNullOrWhiteSpace(address))
                return Result.Failure<Venue>("Venue address is required");

            if (address.Length > 500)
                return Result.Failure<Venue>("Venue address cannot exceed 500 characters");

            if (string.IsNullOrWhiteSpace(city))
                return Result.Failure<Venue>("City is required");

            if (string.IsNullOrWhiteSpace(state))
                return Result.Failure<Venue>("State is required");

            if (string.IsNullOrWhiteSpace(country))
                return Result.Failure<Venue>("Country is required");

            // Validate coordinates if provided
            if (latitude.HasValue && (latitude < -90 || latitude > 90))
                return Result.Failure<Venue>("Latitude must be between -90 and 90");

            if (longitude.HasValue && (longitude < -180 || longitude > 180))
                return Result.Failure<Venue>("Longitude must be between -180 and 180");

            return Result.Success(new Venue(
                name.Trim(),
                address.Trim(),
                city.Trim(),
                state.Trim(),
                country.Trim(),
                postalCode?.Trim(),
                latitude,
                longitude));
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Name;
            yield return Address;
            yield return City;
            yield return State;
            yield return Country;
            yield return PostalCode;
            yield return Latitude;
            yield return Longitude;
        }

        public override string ToString()
            => $"{Name}, {Address}, {City}, {State}, {Country}";
    }
}
