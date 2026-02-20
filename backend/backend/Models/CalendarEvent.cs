using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class CalendarEvent
{
    public ulong Id { get; set; }

    public ulong OwnerUserId { get; set; }

    public ulong CreatedByUserId { get; set; }

    public string EventType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public ulong? RelatedServiceRequestId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual User OwnerUser { get; set; } = null!;

    public virtual ServiceRequest? RelatedServiceRequest { get; set; }
}
