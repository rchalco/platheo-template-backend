using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TClasificadorTipo
{
    public int IdClasificadorTipo { get; set; }

    public string? ClasificadorTipo { get; set; }

    public DateTime? FechaRegistro { get; set; }
}
