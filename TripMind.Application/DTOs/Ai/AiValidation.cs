using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TripMind.Application.DTOs.Ai
{
    public static class AiValidation
    {
        public static ValidationResult? ValidateCity(string? city, ValidationContext ctx)
        {
            // City is optional in some DTOs (e.g. HomeRequest)
            if (string.IsNullOrWhiteSpace(city))
                return ValidationResult.Success;

            if (!AiAllowedValues.Cities.Contains(city))
                return new ValidationResult(
                    $"City '{city}' is not supported. Allowed: {string.Join(", ", AiAllowedValues.Cities)}");

            return ValidationResult.Success;
        }
        public static ValidationResult? ValidateCities(List<string>? cities, ValidationContext ctx)
        {
            if (cities == null || cities.Count == 0)
                return ValidationResult.Success;

            var invalid = cities.Where(c => !AiAllowedValues.Cities.Contains(c)).ToList();
            if (invalid.Any())
                return new ValidationResult(
                    $"Invalid cities: {string.Join(", ", invalid)}. Allowed: {string.Join(", ", AiAllowedValues.Cities)}");

            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateDisplayInterests(List<string>? values, ValidationContext ctx)
        {
            if (values == null || values.Count == 0)
                return ValidationResult.Success;

            var invalid = values.Where(v => !AiAllowedValues.DisplayInterests.Contains(v)).ToList();
            if (invalid.Any())
                return new ValidationResult(
                    $"Invalid interests: {string.Join(", ", invalid)}. Allowed: {string.Join(", ", AiAllowedValues.DisplayInterests)}");

            return ValidationResult.Success;
        }

        public static ValidationResult? ValidatePlaceCategories(List<string>? values, ValidationContext ctx)
        {
            if (values == null || values.Count == 0)
                return ValidationResult.Success;

            var invalid = values.Where(v => !AiAllowedValues.PlaceCategorySlugs.Contains(v)).ToList();
            if (invalid.Any())
                return new ValidationResult(
                    $"Invalid categories: {string.Join(", ", invalid)}. Allowed: {string.Join(", ", AiAllowedValues.PlaceCategorySlugs)}");

            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateSortBy(string? value, ValidationContext ctx)
        {
            if (string.IsNullOrWhiteSpace(value) || !AiAllowedValues.SortByValues.Contains(value))
                return new ValidationResult(
                    $"SortBy must be one of: {string.Join(", ", AiAllowedValues.SortByValues)}");

            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateOrder(string? value, ValidationContext ctx)
        {
            if (string.IsNullOrWhiteSpace(value) || !AiAllowedValues.OrderValues.Contains(value))
                return new ValidationResult(
                    $"Order must be one of: {string.Join(", ", AiAllowedValues.OrderValues)}");

            return ValidationResult.Success;
        }
    }
}