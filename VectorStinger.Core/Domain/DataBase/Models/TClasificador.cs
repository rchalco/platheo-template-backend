using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TClasificador
{
    public long IdClasificador { get; set; }

    public int? IdClasificadorTipo { get; set; }

    public string? Clasificador { get; set; }

    public string? Icono { get; set; }

    public string? Observacion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaRegistroHasta { get; set; }
}
