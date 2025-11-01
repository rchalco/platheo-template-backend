using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TAmbiente
{
    public long IdAmbiente { get; set; }

    public long? IdSesion { get; set; }

    public long? IdPersona { get; set; }

    public int? IdTipoAmbiente { get; set; }

    public string? NombreAmbiente { get; set; }

    public int? IdCountry { get; set; }

    public string? City { get; set; }

    public string? Zona { get; set; }

    public string? AddressMap { get; set; }

    public string? Latitud { get; set; }

    public string? Longitud { get; set; }

    public int Activo { get; set; }

    public string? Observacion { get; set; }

    public int? Disponible { get; set; }

    public decimal? RatingConsolidado { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
