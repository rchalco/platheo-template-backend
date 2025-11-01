using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TReservasAmbiente
{
    public long IdReservaAmbiente { get; set; }

    public long? IdSesion { get; set; }

    public long IdAmbiente { get; set; }

    public long? IdPersona { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public int? Estado { get; set; }

    public decimal? NpsReserva { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
