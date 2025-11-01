using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TPersona
{
    public long IdPersona { get; set; }

    public long IdSesion { get; set; }

    public int? IdTipoPersona { get; set; }

    public string TipoDocumentoIdentidad { get; set; } = null!;

    public string DocumentoDeIdentidad { get; set; } = null!;

    public string? ComplementoCi { get; set; }

    public string? Extension { get; set; }

    public string? RazonSocial { get; set; }

    public string? Nit { get; set; }

    public string ApellidoPaterno { get; set; } = null!;

    public string ApellidoMaterno { get; set; } = null!;

    public string Nombres { get; set; } = null!;

    public string Nacionalidad { get; set; } = null!;

    public string Telfono { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? DireccionUbicacionLatitud { get; set; }

    public string? DireccionUbicacionLongitud { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
