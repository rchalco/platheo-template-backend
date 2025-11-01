using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TServiciosAdicionale
{
    public long IdServicioAdicional { get; set; }

    public long? IdInmueble { get; set; }

    public int? IdClasificador { get; set; }

    public int? Numero { get; set; }

    public string? DescripcionGeneral { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
