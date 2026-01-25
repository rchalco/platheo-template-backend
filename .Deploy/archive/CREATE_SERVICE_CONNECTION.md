# ?? Guía Paso a Paso: Crear Service Connection en Azure DevOps

## ?? IMPORTANTE
Este paso es **OBLIGATORIO** antes de ejecutar la pipeline. El error que estás viendo se debe a que falta esta configuración.

---

## ?? Información que Necesitas

### Desde Azure Portal (ACR)

Ya tienes esta información (según tu imagen):

| Campo | Valor |
|-------|-------|
| **Registry name** | `acrplatheotemplatestg` |
| **Login server** | `acrplatheotemplatestg-gafvh3d5d4hbb4fc.azurecr.io` |
| **Usuario admin** | ? Habilitado |
| **Username** | `acrplatheotemplatestg` |
| **Password** | (password o password2 del ACR) |

---

## ?? Pasos para Crear la Service Connection

### Paso 1: Obtener las Credenciales del ACR

#### Opción A: Desde Azure Portal (Tu método actual)

1. Ve a Azure Portal ? `acrplatheotemplatestg`
2. En el menú izquierdo, click en **Claves de acceso** (Access keys)
3. Verás:
   - **Usuario administrador**: ? Habilitado
   - **Nombre de usuario**: `acrplatheotemplatestg`
   - **password**: `[valor oculto]` ? Click en ??? para ver
   - **password2**: `[valor oculto]` ? Click en ??? para ver
4. **Copia el valor de `password` o `password2`** (cualquiera funciona)

#### Opción B: Desde PowerShell

```powershell
# Ejecutar este comando
az acr credential show --name acrplatheotemplatestg --resource-group "Suscripción Platheo"
```

**Output esperado:**
```json
{
  "passwords": [
    {
      "name": "password",
      "value": "COPIAR_ESTE_VALOR_COMPLETO"
    },
    {
      "name": "password2",
      "value": "O_ESTE_VALOR"
    }
  ],
  "username": "acrplatheotemplatestg"
}
```

---

### Paso 2: Ir a Azure DevOps

1. **Abrir Azure DevOps**
   - URL: `https://dev.azure.com/platheoinc/Platheo-Templates`
   - Inicia sesión si no lo has hecho

2. **Ir a Project Settings**
   - Click en el ícono de **?? Project Settings** (abajo a la izquierda)
   - O navega directamente: `https://dev.azure.com/platheoinc/Platheo-Templates/_settings/adminservices`

3. **Abrir Service Connections**
   - En el menú lateral, bajo **Pipelines**, click en **Service connections**

---

### Paso 3: Crear Nueva Service Connection

1. **Iniciar creación**
   - Click en el botón azul **New service connection**
   - Se abrirá un panel lateral

2. **Buscar y seleccionar tipo**
   - En el cuadro de búsqueda, escribe: `docker`
   - Selecciona: **Docker Registry**
   - Click en **Next**

---

### Paso 4: Configurar la Conexión

**?? IMPORTANTE: Selecciona "Others" NO "Azure Container Registry"**

#### Configuración:

| Campo | Valor a Ingresar |
|-------|------------------|
| **Registry type** | Selecciona: **Others** |
| **Docker Registry** | `acrplatheotemplatestg-gafvh3d5d4hbb4fc.azurecr.io` |
| **Docker ID** | `acrplatheotemplatestg` |
| **Docker Password** | (Pega el password del Paso 1) |
| **Service connection name** | `acrplatheotemplatestg` ?? **EXACTO** |
| **Description** | `ACR connection for Platheo Templates` |
| **Security** | ? **Grant access permission to all pipelines** |

#### Detalles Importantes:

- **Registry type**: 
  - ? NO selecciones "Azure Container Registry"
  - ? Selecciona "Others"
  - Razón: Estamos usando credenciales de admin, no Azure AD

- **Service connection name**:
  - ?? DEBE SER EXACTAMENTE: `acrplatheotemplatestg`
  - Sin espacios, sin guiones, sin mayúsculas diferentes
  - Razón: La pipeline lo busca con este nombre exacto

- **Grant access permission to all pipelines**:
  - ? DEBE estar marcado
  - Razón: Permite que la pipeline use esta conexión

---

### Paso 5: Guardar y Verificar

1. **Guardar**
   - Click en el botón **Save** (abajo)
   - Azure DevOps validará la conexión automáticamente

2. **Verificar estado**
   - La nueva service connection aparecerá en la lista
   - Estado esperado: ? **Ready**
   - Si aparece error, verifica las credenciales

3. **Verificar permisos**
   - La conexión debe tener: `All pipelines can use this connection`
   - Si no, editarla y marcar la opción

---

## ? Verificación Post-Creación

### Verificar en Azure DevOps UI

1. Ve a: **Project Settings** ? **Service connections**
2. Busca: `acrplatheotemplatestg`
3. Verifica:
   - ? Estado: Ready
   - ? Type: Docker Registry
   - ? Registry: acrplatheotemplatestg-gafvh3d5d4hbb4fc.azurecr.io
   - ? Security: All pipelines can use this connection

### Verificar con Azure CLI (Opcional)

```powershell
# Listar service connections
az devops service-endpoint list `
  --organization "https://dev.azure.com/platheoinc" `
  --project "Platheo-Templates" `
  --output table
```

---

## ?? Ejecutar la Pipeline

Una vez creada la service connection:

1. **Ir a Pipelines**
   - Ve a: **Pipelines** ? **Pipelines**
   - URL: `https://dev.azure.com/platheoinc/Platheo-Templates/_build`

2. **Ejecutar pipeline**
   - Encuentra la pipeline de Stage
   - Click en **Run pipeline**
   - Branch: `main`
   - Click en **Run**

3. **Monitorear ejecución**
   - La pipeline comenzará a ejecutarse
   - Verás el progreso en tiempo real
   - Primer stage: Build (3-5 minutos)
   - Segundo stage: Deploy (2-3 minutos)

---

## ?? Troubleshooting

### Error: "service connection could not be found"

**Causa:** El nombre de la service connection no coincide con el nombre en la pipeline.

**Solución:**
1. Ve a **Service connections**
2. Verifica que el nombre sea exactamente: `acrplatheotemplatestg`
3. Si es diferente, edítala o créala de nuevo con el nombre correcto

### Error: "Failed to validate connection"

**Causa:** Credenciales incorrectas o mal copiadas.

**Solución:**
1. Obtén nuevamente las credenciales:
   ```powershell
   az acr credential show --name acrplatheotemplatestg
   ```
2. Edita la service connection:
   - Click en la conexión ? **Edit**
   - Actualiza el Docker Password
   - Click **Verify and Save**

### Error: "Authentication failed"

**Causa:** El Docker Registry URL está incorrecto.

**Solución:**
- Verifica que sea: `acrplatheotemplatestg-gafvh3d5d4hbb4fc.azurecr.io`
- NO debe tener `https://` al inicio
- NO debe tener `/` al final

### Error: "This service connection has been disabled or deleted"

**Causa:** La service connection se deshabilitó o eliminó accidentalmente.

**Solución:**
1. Si está deshabilitada: **Edit** ? Habilitar ? **Save**
2. Si fue eliminada: Crear nuevamente siguiendo los pasos

### Error: "Pipeline is not authorized to use this service connection"

**Causa:** La opción "Grant access permission to all pipelines" no está marcada.

**Solución:**
1. Edit la service connection
2. En la pestaña **Security**
3. Marca: ? **Grant access permission to all pipelines**
4. O agrega específicamente tu pipeline
5. Click **Save**

---

## ?? Script de Ayuda

He creado un script que facilita el proceso:

```powershell
# Ejecutar desde la raíz del repositorio
.\.Deploy\create-service-connection.ps1
```

**El script te ayudará a:**
- ? Obtener las credenciales automáticamente
- ? Mostrar toda la información necesaria
- ? Abrir Azure DevOps en el navegador
- ? Guiarte en los pasos de creación

---

## ?? Checklist Final

Antes de ejecutar la pipeline, verifica:

- [ ] Service connection creada con nombre: `acrplatheotemplatestg`
- [ ] Estado de la conexión: ? Ready
- [ ] Tipo: Docker Registry (Others)
- [ ] Registry: `acrplatheotemplatestg-gafvh3d5d4hbb4fc.azurecr.io`
- [ ] Username: `acrplatheotemplatestg`
- [ ] Password: (Válido y verificado)
- [ ] Permiso: "All pipelines can use this connection" ?
- [ ] Pipeline existe en Azure DevOps
- [ ] YAML file: `.Deploy/azure-pipelines-stage.yml`

---

## ?? Siguiente Paso

Una vez completada la configuración:

1. ? Service connection creada
2. ?? Ejecuta la pipeline
3. ?? Monitorea la ejecución
4. ? Verifica el deployment en Azure Container Apps

---

**¿Necesitas ayuda?** Usa el script helper o revisa la sección de troubleshooting.

---

**Última actualización**: 11/01/2025 - v1.0
