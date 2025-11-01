using System;
using System.Collections.Generic;
using VectorStinger.Core.Domain.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace VectorStinger.Core.Domain.DataBase.Data;

public partial class DbearthBnbContext : DbContext
{
    public DbearthBnbContext()
    {
    }

    public DbearthBnbContext(DbContextOptions<DbearthBnbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TAddress> TAddresses { get; set; }

    public virtual DbSet<TAmbiente> TAmbientes { get; set; }

    public virtual DbSet<TBankAccountsHost> TBankAccountsHosts { get; set; }

    public virtual DbSet<TClasificador> TClasificadors { get; set; }

    public virtual DbSet<TClasificadorTipo> TClasificadorTipos { get; set; }

    public virtual DbSet<TDatosPersonale> TDatosPersonales { get; set; }

    public virtual DbSet<TEspecificacionesAmbiente> TEspecificacionesAmbientes { get; set; }

    public virtual DbSet<TFotosAmbiente> TFotosAmbientes { get; set; }

    public virtual DbSet<TPersona> TPersonas { get; set; }

    public virtual DbSet<TPost> TPosts { get; set; }

    public virtual DbSet<TRatingAmbiente> TRatingAmbientes { get; set; }

    public virtual DbSet<TReservasAmbiente> TReservasAmbientes { get; set; }

    public virtual DbSet<TRole> TRoles { get; set; }

    public virtual DbSet<TRolesMenu> TRolesMenus { get; set; }

    public virtual DbSet<TServiciosAdicionale> TServiciosAdicionales { get; set; }

    public virtual DbSet<TSesione> TSesiones { get; set; }

    public virtual DbSet<TTipoDeCambio> TTipoDeCambios { get; set; }

    public virtual DbSet<TUsuario> TUsuarios { get; set; }

    public virtual DbSet<TVersione> TVersiones { get; set; }

    public virtual DbSet<TmenuOpcione> TmenuOpciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TAddress>(entity =>
        {
            entity.HasKey(e => e.IdAddress).HasName("PK__tAddress__67E8C78CC56A541E");

            entity.ToTable("tAddress", "Inmueble");

            entity.Property(e => e.IdAddress).HasColumnName("idAddress");
            entity.Property(e => e.Activo)
                .HasDefaultValue(1)
                .HasColumnName("activo");
            entity.Property(e => e.AddressMap)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("addressMap");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("city");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.IdAmbiente).HasColumnName("idAmbiente");
            entity.Property(e => e.IdCountry)
                .HasDefaultValue(736)
                .HasColumnName("idCountry");
            entity.Property(e => e.IdSesion).HasColumnName("idSesion");
            entity.Property(e => e.Latitud)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("latitud");
            entity.Property(e => e.Longitud)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("longitud");
            entity.Property(e => e.Zona)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("zona");
        });

        modelBuilder.Entity<TAmbiente>(entity =>
        {
            entity.HasKey(e => e.IdAmbiente).HasName("PK__tAmbient__2CD01707FADECFFE");

            entity.ToTable("tAmbientes", "Inmueble");

            entity.Property(e => e.IdAmbiente).HasColumnName("idAmbiente");
            entity.Property(e => e.Activo)
                .HasDefaultValue(1)
                .HasColumnName("activo");
            entity.Property(e => e.AddressMap)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("addressMap");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("city");
            entity.Property(e => e.Disponible).HasColumnName("disponible");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.IdCountry)
                .HasDefaultValue(736)
                .HasColumnName("idCountry");
            entity.Property(e => e.IdPersona).HasColumnName("idPersona");
            entity.Property(e => e.IdSesion).HasColumnName("idSesion");
            entity.Property(e => e.IdTipoAmbiente).HasColumnName("idTipoAmbiente");
            entity.Property(e => e.Latitud)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("latitud");
            entity.Property(e => e.Longitud)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("longitud");
            entity.Property(e => e.NombreAmbiente)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombreAmbiente");
            entity.Property(e => e.Observacion)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("observacion");
            entity.Property(e => e.RatingConsolidado)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("ratingConsolidado");
            entity.Property(e => e.Zona)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("zona");
        });

        modelBuilder.Entity<TBankAccountsHost>(entity =>
        {
            entity.HasKey(e => e.IdAccount).HasName("PK__tBankAcc__DA18132C0D84200D");

            entity.ToTable("tBankAccountsHost", "Inmueble");

            entity.Property(e => e.IdAccount).HasColumnName("idAccount");
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("accountNumber");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.IdBank).HasColumnName("idBank");
            entity.Property(e => e.IdMoneda).HasColumnName("idMoneda");
            entity.Property(e => e.IdPersona).HasColumnName("idPersona");
            entity.Property(e => e.IdSesion).HasColumnName("idSesion");
        });

        modelBuilder.Entity<TClasificador>(entity =>
        {
            entity.HasKey(e => e.IdClasificador).HasName("PK__tClasifi__5BEE10F8709BE928");

            entity.ToTable("tClasificador", "Parametros");

            entity.Property(e => e.IdClasificador).HasColumnName("idClasificador");
            entity.Property(e => e.Clasificador)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("clasificador");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaRegistroHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistroHasta");
            entity.Property(e => e.Icono)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("icono");
            entity.Property(e => e.IdClasificadorTipo).HasColumnName("idClasificadorTipo");
            entity.Property(e => e.Observacion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("observacion");
        });

        modelBuilder.Entity<TClasificadorTipo>(entity =>
        {
            entity.HasKey(e => e.IdClasificadorTipo).HasName("PK__tClasifi__AE88AB2928DE874D");

            entity.ToTable("tClasificadorTipo", "Parametros");

            entity.Property(e => e.IdClasificadorTipo).HasColumnName("idClasificadorTipo");
            entity.Property(e => e.ClasificadorTipo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("clasificadorTipo");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
        });

        modelBuilder.Entity<TDatosPersonale>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tDatosPersonales", "Cliente");

            entity.Property(e => e.ApellidoMaterno)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellidoMaterno");
            entity.Property(e => e.ApellidoPaterno)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellidoPaterno");
            entity.Property(e => e.ComplementoCi)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("complementoCI");
            entity.Property(e => e.Direccion)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.DireccionUbicacionLatitud).HasColumnName("direccionUbicacionLatitud");
            entity.Property(e => e.DireccionUbicacionLongitud).HasColumnName("direccionUbicacionLongitud");
            entity.Property(e => e.DocumentoDeIdentidad)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("documentoDeIdentidad");
            entity.Property(e => e.Extension)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("extension");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.IdPersona)
                .ValueGeneratedOnAdd()
                .HasColumnName("idPersona");
            entity.Property(e => e.IdSesion).HasColumnName("idSesion");
            entity.Property(e => e.IdTipoPersona).HasColumnName("idTipoPersona");
            entity.Property(e => e.Nit)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nit");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombres");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("razonSocial");
        });

        modelBuilder.Entity<TEspecificacionesAmbiente>(entity =>
        {
            entity.HasKey(e => e.IdEspecificacion).HasName("PK__tEspecif__3A251E9362EEC6AE");

            entity.ToTable("tEspecificacionesAmbiente", "Inmueble");

            entity.Property(e => e.IdEspecificacion).HasColumnName("idEspecificacion");
            entity.Property(e => e.DescripcionGeneral)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("descripcionGeneral");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.IdAmbiente).HasColumnName("idAmbiente");
            entity.Property(e => e.IdClasificador).HasColumnName("idClasificador");
            entity.Property(e => e.IdSesion).HasColumnName("idSesion");
            entity.Property(e => e.NumeroAmbientes).HasColumnName("numeroAmbientes");
        });

        modelBuilder.Entity<TFotosAmbiente>(entity =>
        {
            entity.HasKey(e => e.IdFoto).HasName("PK__tFotosAm__69D6509402CDA579");

            entity.ToTable("tFotosAmbiente", "Inmueble");

            entity.Property(e => e.IdFoto).HasColumnName("idFoto");
            entity.Property(e => e.DescripcionFoto)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("descripcionFoto");
            entity.Property(e => e.EsFotoInicial).HasColumnName("esFotoInicial");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.Foto).HasColumnName("foto");
            entity.Property(e => e.IdAmbiente).HasColumnName("idAmbiente");
            entity.Property(e => e.IdSesion).HasColumnName("idSesion");
            entity.Property(e => e.PathFoto)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("pathFoto");
        });

        modelBuilder.Entity<TPersona>(entity =>
        {
            entity.HasKey(e => e.IdPersona);

            entity.ToTable("tPersona", "Cliente");

            entity.Property(e => e.IdPersona).HasColumnName("idPersona");
            entity.Property(e => e.ApellidoMaterno)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellidoMaterno");
            entity.Property(e => e.ApellidoPaterno)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellidoPaterno");
            entity.Property(e => e.ComplementoCi)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("complementoCI");
            entity.Property(e => e.Direccion)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.DireccionUbicacionLatitud)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("direccionUbicacionLatitud");
            entity.Property(e => e.DireccionUbicacionLongitud)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("direccionUbicacionLongitud");
            entity.Property(e => e.DocumentoDeIdentidad)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("documentoDeIdentidad");
            entity.Property(e => e.Extension)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("extension");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.IdSesion).HasColumnName("idSesion");
            entity.Property(e => e.IdTipoPersona).HasColumnName("idTipoPersona");
            entity.Property(e => e.Nacionalidad)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nacionalidad");
            entity.Property(e => e.Nit)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nit");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombres");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("razonSocial");
            entity.Property(e => e.Telfono)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("telfono");
            entity.Property(e => e.TipoDocumentoIdentidad)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TPost>(entity =>
        {
            entity.HasKey(e => e.IdAnuncio).HasName("PK__tPost__0BC1EC3E1CE835CB");

            entity.ToTable("tPost", "Inmueble");

            entity.Property(e => e.IdAnuncio).HasColumnName("idAnuncio");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.IdAmbiente).HasColumnName("idAmbiente");
            entity.Property(e => e.IdMoneda).HasColumnName("idMoneda");
            entity.Property(e => e.IdSesion).HasColumnName("idSesion");
            entity.Property(e => e.IdTipoAnuncio).HasColumnName("idTipoAnuncio");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("title");
        });

        modelBuilder.Entity<TRatingAmbiente>(entity =>
        {
            entity.HasKey(e => e.IdRatingAmbiente).HasName("PK__tRatingA__8802E2BBEF6C964C");

            entity.ToTable("tRatingAmbiente", "Inmueble");

            entity.Property(e => e.IdRatingAmbiente).HasColumnName("idRatingAmbiente");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.IdReservaAmbiente).HasColumnName("idReservaAmbiente");
            entity.Property(e => e.IdSesion).HasColumnName("idSesion");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("observaciones");
            entity.Property(e => e.Rating).HasColumnName("rating");
        });

        modelBuilder.Entity<TReservasAmbiente>(entity =>
        {
            entity.HasKey(e => e.IdReservaAmbiente).HasName("PK__tReserva__EA8912054A650817");

            entity.ToTable("tReservasAmbiente", "Inmueble");

            entity.Property(e => e.IdReservaAmbiente).HasColumnName("idReservaAmbiente");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("fechaFin");
            entity.Property(e => e.FechaInicio)
                .HasColumnType("datetime")
                .HasColumnName("fechaInicio");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.IdAmbiente).HasColumnName("idAmbiente");
            entity.Property(e => e.IdPersona).HasColumnName("idPersona");
            entity.Property(e => e.IdSesion).HasColumnName("idSesion");
            entity.Property(e => e.NpsReserva)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("npsReserva");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("observaciones");
        });

        modelBuilder.Entity<TRole>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__tRoles__3C872F76EDB904FF");

            entity.ToTable("tRoles", "seguridad");

            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.IdSesion).HasColumnName("id_sesion");
            entity.Property(e => e.Rol)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rol");
        });

        modelBuilder.Entity<TRolesMenu>(entity =>
        {
            entity.HasKey(e => e.IdrolMenu).HasName("PK__tRolesMe__E7658ACBF986AD23");

            entity.ToTable("tRolesMenu", "seguridad");

            entity.Property(e => e.IdrolMenu).HasColumnName("idrolMenu");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.IdMenuOpcion).HasColumnName("idMenuOpcion");
            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.Orden).HasColumnName("orden");
        });

        modelBuilder.Entity<TServiciosAdicionale>(entity =>
        {
            entity.HasKey(e => e.IdServicioAdicional).HasName("PK__tServici__877AC2665EB71AFF");

            entity.ToTable("tServiciosAdicionales", "Inmueble");

            entity.Property(e => e.IdServicioAdicional).HasColumnName("idServicioAdicional");
            entity.Property(e => e.DescripcionGeneral)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("descripcionGeneral");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.IdClasificador).HasColumnName("idClasificador");
            entity.Property(e => e.IdInmueble).HasColumnName("idInmueble");
            entity.Property(e => e.Numero).HasColumnName("numero");
        });

        modelBuilder.Entity<TSesione>(entity =>
        {
            entity.HasKey(e => e.IdSesion).HasName("PK__tSesione__DB6C2DE69306B7CF");

            entity.ToTable("tSesiones", "seguridad");

            entity.HasIndex(e => e.Token, "IDX_tSesioenes_token");

            entity.Property(e => e.IdSesion).HasColumnName("idSesion");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Token).HasColumnName("token");
        });

        modelBuilder.Entity<TTipoDeCambio>(entity =>
        {
            entity.HasKey(e => e.IdTipoCambio).HasName("PK__tTipoDeC__568436313618EDA4");

            entity.ToTable("tTipoDeCambio", "Parametros");

            entity.Property(e => e.IdTipoCambio).HasColumnName("idTipoCambio");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.IdMonedaBase).HasColumnName("idMonedaBase");
            entity.Property(e => e.IdMonedaObjeto).HasColumnName("idMonedaObjeto");
            entity.Property(e => e.IdSesion).HasColumnName("idSesion");
            entity.Property(e => e.IdTipoCotizacion).HasColumnName("idTipoCotizacion");
            entity.Property(e => e.TipoDeCambio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("tipoDeCambio");
        });

        modelBuilder.Entity<TUsuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__tUsuario__645723A62955B94B");

            entity.ToTable("tUsuarios", "seguridad");

            entity.HasIndex(e => new { e.Proveedor, e.ProveedorUserId }, "Idx_ProveedorUserId");

            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.DecodedTokenClaims).IsUnicode(false);
            entity.Property(e => e.EsInvitado).HasColumnName("es_invitado");
            entity.Property(e => e.EsVerificado).HasColumnName("es_verificado");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaUltimoLogin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_ultimo_login");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.FotoUrl)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("foto_url");
            entity.Property(e => e.IdPersona).HasColumnName("idPersona");
            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.IdSesion).HasColumnName("idSesion");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Proveedor)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("proveedor");
            entity.Property(e => e.ProveedorUserId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("proveedor_user_id");
            entity.Property(e => e.Usuario)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("usuario");
        });

        modelBuilder.Entity<TVersione>(entity =>
        {
            entity.HasKey(e => e.IdVersion).HasName("PK__tVersion__BBD5F8B29A2F3E5D");

            entity.ToTable("tVersiones", "seguridad");

            entity.Property(e => e.IdVersion).HasColumnName("idVersion");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.Version)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TmenuOpcione>(entity =>
        {
            entity.HasKey(e => e.IdMenuOpcion).HasName("PK__tmenuOpc__0C9FFA54BF0EEAC3");

            entity.ToTable("tmenuOpciones", "seguridad");

            entity.Property(e => e.IdMenuOpcion).HasColumnName("idMenuOpcion");
            entity.Property(e => e.Decripcion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FechaVigenciaHasta)
                .HasColumnType("datetime")
                .HasColumnName("fechaVigenciaHasta");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("icon");
            entity.Property(e => e.Title)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("title");
            entity.Property(e => e.Url)
                .HasMaxLength(250)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
