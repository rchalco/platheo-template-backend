using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TAnuncio
{
    public long IdAnuncio { get; set; }

    public long? IdInmueble { get; set; }

    public int? IdTipoAmbiente { get; set; }

    public int? IdTipoAnuncio { get; set; }

    public decimal? Precio { get; set; }

    public int? IdMoneda { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
