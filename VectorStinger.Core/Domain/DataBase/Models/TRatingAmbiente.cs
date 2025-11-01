using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TRatingAmbiente
{
    public long IdRatingAmbiente { get; set; }

    public long? IdSesion { get; set; }

    public long IdReservaAmbiente { get; set; }

    public int? Rating { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
