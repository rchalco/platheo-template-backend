using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TPost
{
    public long IdAnuncio { get; set; }

    public long? IdSesion { get; set; }

    public long? IdAmbiente { get; set; }

    public int? IdTipoAnuncio { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public int? IdMoneda { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
