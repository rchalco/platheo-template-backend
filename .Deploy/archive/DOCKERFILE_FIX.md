# ?? Corrección del Dockerfile - Proyectos Faltantes

## ?? Problema Identificado

El build de la imagen Docker en Azure Pipeline estaba fallando porque el **Dockerfile** no incluía todos los proyectos necesarios del workspace.

### ? Proyectos que Faltaban:

1. **VectorStinger.Foundation.Factory** - Proyecto de fábrica/factory pattern
2. **VectorStinger.Infrastructure.Bucket** - Proyecto de infraestructura para manejo de buckets (AWS S3)

## ?? Causa del Error

Cuando Docker intentaba restaurar las dependencias y compilar el proyecto `VectorStinger.Api.Service`, no encontraba las referencias a estos proyectos porque:

1. Los archivos `.csproj` no se copiaban en la etapa de build
2. Al ejecutar `dotnet restore`, fallaba por referencias faltantes
3. El build completo no podía completarse

## ? Solución Implementada

Se actualizó el archivo `VectorStinger.Container/Dockerfile` para incluir ambos proyectos en la sección de **COPY** de archivos de proyecto:

```dockerfile
# Copiar archivos de proyecto para restaurar dependencias (layer caching)
COPY ["VectorStinger.Api.Service/VectorStinger.Api.Service.csproj", "VectorStinger.Api.Service/"]
COPY ["VectorStinger.Application/VectorStinger.Application.csproj", "VectorStinger.Application/"]
COPY ["VectorStinger.Core/VectorStinger.Core.csproj", "VectorStinger.Core/"]
COPY ["VectorStinger.Foundation.Abstractions/VectorStinger.Foundation.Abstractions.csproj", "VectorStinger.Foundation.Abstractions/"]
COPY ["VectorStinger.Foundation.Factory/VectorStinger.Foundation.Factory.csproj", "VectorStinger.Foundation.Factory/"]  # ? AGREGADO
COPY ["VectorStinger.Foundation.Utilities/VectorStinger.Foundation.Utilities.csproj", "VectorStinger.Foundation.Utilities/"]
COPY ["VectorStinger.Host.ServiceDefaults/VectorStinger.Host.ServiceDefaults.csproj", "VectorStinger.Host.ServiceDefaults/"]
COPY ["VectorStinger.Infrastructure.Bucket/VectorStinger.Infrastructure.Bucket.csproj", "VectorStinger.Infrastructure.Bucket/"]  # ? AGREGADO
COPY ["VectorStinger.Infrastructure.DataAccess/VectorStinger.Infrastructure.DataAccess.csproj", "VectorStinger.Infrastructure.DataAccess/"]
COPY ["VectorStinger.Infrastructure.OAuth/VectorStinger.Infrastructure.OAuth.csproj", "VectorStinger.Infrastructure.OAuth/"]
COPY ["VectorStinger.Modules.Security/VectorStinger.Modules.Security.csproj", "VectorStinger.Modules.Security/"]
COPY ["VectorSinger.Modules.WebTemplate/VectorSinger.Modules.WebTemplate.csproj", "VectorSinger.Modules.WebTemplate/"]
```

## ?? Lista Completa de Proyectos en el Dockerfile

| # | Proyecto | Estado |
|---|----------|--------|
| 1 | VectorStinger.Api.Service | ? Incluido |
| 2 | VectorStinger.Application | ? Incluido |
| 3 | VectorStinger.Core | ? Incluido |
| 4 | VectorStinger.Foundation.Abstractions | ? Incluido |
| 5 | VectorStinger.Foundation.Factory | ? **AGREGADO** |
| 6 | VectorStinger.Foundation.Utilities | ? Incluido |
| 7 | VectorStinger.Host.ServiceDefaults | ? Incluido |
| 8 | VectorStinger.Infrastructure.Bucket | ? **AGREGADO** |
| 9 | VectorStinger.Infrastructure.DataAccess | ? Incluido |
| 10 | VectorStinger.Infrastructure.OAuth | ? Incluido |
| 11 | VectorStinger.Modules.Security | ? Incluido |
| 12 | VectorSinger.Modules.WebTemplate | ? Incluido |

**Nota:** El proyecto `VectorStinger.Infrastructure.Tests` no se incluye porque es solo para pruebas y no es necesario en la imagen de producción.

## ?? Siguiente Paso

Ahora puedes ejecutar el pipeline de Azure DevOps nuevamente. El build de la imagen Docker debería completarse exitosamente.

### Para verificar localmente (opcional):

```bash
# Desde la raíz del repositorio
docker build -f VectorStinger.Container/Dockerfile -t platheo-api:test .
```

## ?? Checklist para Futuros Proyectos

Cuando agregues un nuevo proyecto a la solución que sea referenciado por `VectorStinger.Api.Service`:

- [ ] Agregar el archivo `.csproj` al Dockerfile en la sección de COPY
- [ ] Verificar que el build local funcione: `dotnet build VectorStinger.Api.Service`
- [ ] Probar el build de Docker localmente antes de hacer push
- [ ] Actualizar esta documentación si es necesario

## ?? Referencias

- Dockerfile: `VectorStinger.Container/Dockerfile`
- Pipeline: `.Deploy/azure-pipelines-stage.yml`
- Build ID con error: #53

---
**Fecha de corrección:** $(date)
**Autor:** GitHub Copilot
