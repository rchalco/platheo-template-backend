using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class TBankAccountsHost
{
    public long IdAccount { get; set; }

    public long? IdSesion { get; set; }

    public long? IdPersona { get; set; }

    public int? IdBank { get; set; }

    public string? AccountNumber { get; set; }

    public int? IdMoneda { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaVigenciaHasta { get; set; }
}
