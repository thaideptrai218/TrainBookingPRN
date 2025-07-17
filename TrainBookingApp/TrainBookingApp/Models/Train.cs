using System;
using System.Collections.Generic;

namespace TrainBookingApp.Models;

public partial class Train
{
    public int TrainId { get; set; }

    public string TrainName { get; set; } = null!;

    public int TrainTypeId { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Coach> Coaches { get; set; } = new List<Coach>();

    public virtual TrainType TrainType { get; set; } = null!;

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
