using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TAddress
{
    public long IdAddress { get; set; }

    public long IdSesion { get; set; }

    public long IdAmbiente { get; set; }

    public int? IdCountry { get; set; }

    public string? City { get; set; }

    public string? Zona { get; set; }

    public string? AddressMap { get; set; }

    public string? Latitud { get; set; }

    public string? Longitud { get; set; }

    public int Activo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
