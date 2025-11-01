using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TInmueble
{
    public long IdInmueble { get; set; }

    public long? IdPersona { get; set; }

    public int? IdTipoAmbiente { get; set; }

    public string? NombreAmbiente { get; set; }

    public string? Direccion { get; set; }

    public long? AmbienteUbicacionLatitud { get; set; }

    public long? AmbienteUbicacionLongitud { get; set; }

    public int? Agua { get; set; }

    public int? Luz { get; set; }

    public int? Alcantarillado { get; set; }

    public string? Observacion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
