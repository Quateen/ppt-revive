using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPTRevive.Domain.Entities;
public class UserSubscription : BaseAuditableEntity
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    [Required]
    public DateTime StartDateTime { get; set; }

    [Required]
    public DateTime EndDateTime { get; set; }

    public User User { get; set; } = default!;
    public Subscription Subscription { get; set; } = default!;
}

