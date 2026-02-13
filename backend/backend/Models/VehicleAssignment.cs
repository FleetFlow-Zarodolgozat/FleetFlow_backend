using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class VehicleAssignment
{
    public ulong Id { get; set; }

    public ulong VehicleId { get; set; }

    public ulong DriverId { get; set; }

    public DateTime AssignedFrom { get; set; }

    public DateTime? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Driver Driver { get; set; } = null!;

    public virtual Vehicle Vehicle { get; set; } = null!;
}
