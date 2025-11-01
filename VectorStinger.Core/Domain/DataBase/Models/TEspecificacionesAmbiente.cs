using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TEspecificacionesAmbiente
{
    public long IdEspecificacion { get; set; }

    public long? IdSesion { get; set; }

    public long? IdAmbiente { get; set; }

    public int? IdClasificador { get; set; }

    public int? NumeroAmbientes { get; set; }

    public string? DescripcionGeneral { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
