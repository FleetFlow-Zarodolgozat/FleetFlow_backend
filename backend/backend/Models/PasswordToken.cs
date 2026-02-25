using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class PasswordToken
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool Used { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
