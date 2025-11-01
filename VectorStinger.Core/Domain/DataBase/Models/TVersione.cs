using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TVersione
{
    public int IdVersion { get; set; }

    public string Version { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
