using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TTipoDeCambio
{
    public long IdTipoCambio { get; set; }

    public long? IdSesion { get; set; }

    public int? IdMonedaBase { get; set; }

    public int? IdMonedaObjeto { get; set; }

    public decimal? TipoDeCambio { get; set; }

    public int? IdTipoCotizacion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
