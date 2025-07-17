using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class CoachType
{
    public int CoachTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public decimal PriceMultiplier { get; set; }

    public bool IsCompartmented { get; set; }

    public int? DefaultCompartmentCapacity { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Coach> Coaches { get; set; } = new List<Coach>();
}
