using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TFotosAmbiente
{
    public long IdFoto { get; set; }

    public long? IdSesion { get; set; }

    public long? IdAmbiente { get; set; }

    public int? EsFotoInicial { get; set; }

    public string? PathFoto { get; set; }

    public byte[]? Foto { get; set; }

    public string? DescripcionFoto { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
