using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Driver
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public string LicenseNumber { get; set; } = null!;

    public DateTime? LicenseExpiryDate { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<FuelLog> FuelLogs { get; set; } = new List<FuelLog>();

    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<VehicleAssignment> VehicleAssignments { get; set; } = new List<VehicleAssignment>();
}
