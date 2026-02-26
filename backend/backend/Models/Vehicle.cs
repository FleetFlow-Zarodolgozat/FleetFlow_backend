using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Vehicle
{
    public ulong Id { get; set; }

    public string LicensePlate { get; set; } = null!;

    public string Brand { get; set; } = null!;

    public string Model { get; set; } = null!;

    public int? Year { get; set; }

    public string? Vin { get; set; }

    public int CurrentMileageKm { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<FuelLog> FuelLogs { get; set; } = new List<FuelLog>();

    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();

    public virtual ICollection<VehicleAssignment> VehicleAssignments { get; set; } = new List<VehicleAssignment>();
}
