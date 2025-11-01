using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TUsuario
{
    public long IdUsuario { get; set; }

    public long? IdSesion { get; set; }

    public long? IdPersona { get; set; }

    public long IdRol { get; set; }

    public string Usuario { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Activo { get; set; }

    public bool? EsVerificado { get; set; }

    public DateTime? FechaUltimoLogin { get; set; }

    public string? Proveedor { get; set; }

    public string? ProveedorUserId { get; set; }

    public bool? EsInvitado { get; set; }

    public string? FotoUrl { get; set; }

    public string? DecodedTokenClaims { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
