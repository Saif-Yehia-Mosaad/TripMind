using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TripMind.Application.DTOs.Trip
{
    public sealed class CreateTripRequest
    {
        [Required] public string DestinationGovernorate { get; set; } = null!;
        [Required] public DateTime StartDate { get; set; }
        [Required] public DateTime EndDate { get; set; }
        [Required][Range(1, 10000000)] public decimal TotalBudgetEgp { get; set; }
        public List<string> Interests { get; set; } = new();
    }

    public sealed class TripResponse
    {
        public Guid TripId { get; set; }
        public string DestinationGovernorate { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationDays { get; set; }
        public decimal TotalBudgetEgp { get; set; }
        public string Status { get; set; } = null!;
        public string ShareToken { get; set; } = null!;
        public bool IsPublic { get; set; }
        public BudgetSummary? Budget { get; set; }
        public List<TripDayResponse> Days { get; set; } = new();
    }

    public sealed class BudgetSummary
    {
        public decimal Total { get; set; }
        public decimal Accommodation { get; set; }
        public decimal Food { get; set; }
        public decimal Transport { get; set; }
        public decimal Activities { get; set; }
        public decimal ActualSpent { get; set; }
        public float VariancePct { get; set; }
    }

    public sealed class TripDayResponse
    {
        public int DayNumber { get; set; }
        public DateTime Date { get; set; }
        public List<TripLocationResponse> Locations { get; set; } = new();
    }

    public sealed class TripLocationResponse
    {
        public Guid LocationId { get; set; }
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string TimeSlot { get; set; } = null!;
        public int DurationMinutes { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsHiddenGem { get; set; }
    }
}

    public sealed class UpdateTripRequest
    {
        public string? DestinationGovernorate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [System.ComponentModel.DataAnnotations.Range(1, 10000000)]
        public decimal? TotalBudgetEgp { get; set; }
        public bool? IsPublic { get; set; }
    }
