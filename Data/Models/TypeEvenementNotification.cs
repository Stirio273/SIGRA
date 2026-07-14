using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class TypeEvenementNotification
{
    public int IdTypeEvenement { get; set; }

    public string Libelle { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
