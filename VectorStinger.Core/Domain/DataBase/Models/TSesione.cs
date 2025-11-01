using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TSesione
{
    public long IdSesion { get; set; }

    public long IdUsuario { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime FechaVigenciaHasta { get; set; }

    public Guid Token { get; set; }
}
