using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPTRevive.Domain.Entities;
public class Subscription : BaseAuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int PeriodInDays { get; set; }

    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = default!;
}

