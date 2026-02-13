using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class File
{
    public ulong Id { get; set; }

    public ulong UploadedByUserId { get; set; }

    public string OriginalName { get; set; } = null!;

    public string StoredName { get; set; } = null!;

    public string MimeType { get; set; } = null!;

    public ulong SizeBytes { get; set; }

    public string StorageProvider { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<FuelLog> FuelLogs { get; set; } = new List<FuelLog>();

    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

    public virtual User UploadedByUser { get; set; } = null!;
}
