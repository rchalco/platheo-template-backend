# ?? Crear Azure Resource Manager Service Connection

## ?? PASO OBLIGATORIO

Este es el **segundo service connection** que necesitas. Ya creaste el de ACR, ahora necesitas este para poder desplegar a Azure Container Apps.

---

## ?? Error que Estás Viendo

```
Job DeployToACA: Step input azureSubscription references service connection 
Suscripción Platheo which could not be found.
```

**Causa:** Falta crear la conexión a Azure Resource Manager.

---

## ?? Información Necesaria

Ya la tienes de tu suscripción de Azure:

| Campo | Valor |
|-------|-------|
| **Subscription ID** | `7430bafe-bc29-443c-93d1-a8cb3090136c` |
| **Subscription Name** | `Suscripción Platheo` |
| **Resource Group** | `Suscripción Platheo` |
| **Service Connection Name** | `Suscripción Platheo` ?? **Con acento** |

---

## ?? Pasos para Crear la Service Connection

### Paso 1: Abrir Service Connections

Ya estás ahí! Solo necesitas:

1. **Cerrar** el diálogo actual (el de `acrplatheotemplatestg`)
2. **Click** en "New service connection" (botón azul arriba a la derecha)

### Paso 2: Seleccionar Tipo

1. En el buscador, escribe: `azure resource`
2. Selecciona: **Azure Resource Manager**
3. Click **Next**

### Paso 3: Seleccionar Authentication Method

**Opciones disponibles:**
- ? Service principal (manual)
- ? **Workload Identity federation (automatic)** ? **Selecciona este** (Recomendado)
- ? Managed identity
- ? Publish profile

**¿Por qué Workload Identity federation?**
- ? Más seguro (no usa secrets)
- ? Configuración automática
- ? No requiere gestionar certificados
- ? Recomendado por Microsoft

Click **Next**

### Paso 4: Configurar Scope

**Scope level:**
```
? Subscription  ? Selecciona este
? Management Group
? Machine Learning Workspace
```

**Subscription:**
- Dropdown: Busca y selecciona **"Suscripción Platheo"**
- O busca por ID: `7430bafe-bc29-443c-93d1-a8cb3090136c`

**Resource group (opcional pero recomendado):**
- Dropdown: Selecciona **"Suscripción Platheo"**
- Esto limita el acceso solo a ese resource group (más seguro)

### Paso 5: Detalles de la Conexión

**Service connection name:**
```
Suscripción Platheo
```
?? **IMPORTANTE:**
- Con acento en "ó"
- Un solo espacio entre palabras
- Mayúsculas exactamente como se muestra

**Description (opcional):**
```
Azure Resource Manager connection for Platheo Templates Container Apps deployment
```

**Security:**
```
? Grant access permission to all pipelines
```
?? **DEBE estar marcado** para que la pipeline pueda usarla

### Paso 6: Guardar

1. Click **"Save"**
2. Azure DevOps creará automáticamente:
   - Service Principal en Azure AD
   - Roles necesarios en la suscripción
   - Federated credential

3. Espera a que termine (puede tomar 10-30 segundos)

4. Verifica que el estado sea: ? **Ready**

---

## ? Verificación Post-Creación

### En Azure DevOps UI

Deberías ver **DOS** service connections:

| Name | Type | Status |
|------|------|--------|
| `acrplatheotemplatestg` | Docker Registry | ? Ready |
| `Suscripción Platheo` | Azure Resource Manager | ? Ready |

### Con PowerShell

```powershell
# Ejecutar script de verificación
.\.Deploy\verify-service-connections.ps1
```

**Output esperado:**
```
?? Verificando service connections...

?? Verificando: acrplatheotemplatestg
   ? Existe
      Tipo: dockerregistry
      Estado: Ready

?? Verificando: Suscripción Platheo
   ? Existe
      Tipo: azurerm
      Estado: Ready

? Todas las service connections están configuradas
?? Puedes ejecutar la pipeline
```

### Con Azure CLI

```powershell
# Listar todas las service connections
az devops service-endpoint list `
  --organization "https://dev.azure.com/platheoinc" `
  --project "Platheo-Templates" `
  --query "[].{Name:name, Type:type, IsReady:isReady}" `
  -o table
```

---

## ?? Troubleshooting

### Error: "Failed to create service connection"

**Causa:** No tienes permisos suficientes en la suscripción de Azure.

**Solución:**
1. Verifica que tengas rol de **Owner** o **Contributor** en la suscripción
2. O pide al administrador que cree la conexión
3. Permisos necesarios:
   - Azure: Contributor (mínimo)
   - Azure DevOps: Project Administrator

### Error: "Service principal creation failed"

**Causa:** Permisos insuficientes en Azure AD.

**Solución:**
1. Necesitas permisos de **Application Developer** en Azure AD
2. O usa método **Service principal (manual)** y pide al admin que cree el SP

### Error: "Grant access permission" no está disponible

**Causa:** No tienes permisos de administrador del proyecto.

**Solución:**
1. Pide al administrador del proyecto que te dé permisos
2. O que el administrador cree la conexión y marque esa opción

### Nombre con caracteres especiales

**Problema:** El acento en "Suscripción" causa problemas.

**Solución A (Recomendada):**
- Mantener el nombre exacto: `Suscripción Platheo`
- La pipeline ya está configurada para usarlo así

**Solución B (Alternativa):**
1. Crear con nombre sin acento: `Suscripcion Platheo`
2. Actualizar la pipeline:
```yaml
# En .Deploy/azure-pipelines-stage.yml
variables:
  azureSubscription: 'Suscripcion Platheo'  # Sin acento
```

---

## ?? Permisos Necesarios

### En Azure Subscription

La service connection necesitará estos permisos (se asignan automáticamente):

- **Rol**: Contributor
- **Scope**: Subscription o Resource Group
- **Acciones permitidas**:
  - Leer recursos
  - Crear/actualizar Container Apps
  - Leer/escribir en ACR
  - Gestionar revisiones

### En Azure DevOps

Tu usuario necesita:
- **Project Administrator** o **Build Administrator**
- Permisos para crear service connections

---

## ?? Comparativa: Manual vs Automatic

| Aspecto | Workload Identity (Automatic) | Service Principal (Manual) |
|---------|-------------------------------|----------------------------|
| **Seguridad** | ? Alta (sin secrets) | ?? Media (usa secret) |
| **Configuración** | ? Automática | ?? Manual |
| **Mantenimiento** | ? Bajo | ?? Alto (renovar secrets) |
| **Permisos necesarios** | ?? Application Developer | ? Menos permisos |
| **Recomendado por MS** | ? Sí | ? Legacy |

---

## ?? Próximos Pasos

Una vez creadas **AMBAS** service connections:

1. ? Verificar que ambas están con estado **Ready**
2. ?? Ejecutar la pipeline
3. ?? Monitorear la ejecución

### Ejecutar la Pipeline

```
1. Ve a Pipelines ? Pipelines
2. Selecciona tu pipeline
3. Click "Run pipeline"
4. Branch: main
5. Click "Run"
```

---

## ?? Checklist Final

Antes de ejecutar la pipeline:

- [ ] Service connection ACR creada: `acrplatheotemplatestg`
  - [ ] Tipo: Docker Registry
  - [ ] Estado: Ready
  - [ ] Grant access: ?

- [ ] Service connection Azure creada: `Suscripción Platheo`
  - [ ] Tipo: Azure Resource Manager
  - [ ] Authentication: Workload Identity federation
  - [ ] Subscription: Suscripción Platheo (7430bafe...)
  - [ ] Resource Group: Suscripción Platheo
  - [ ] Estado: Ready
  - [ ] Grant access: ?

- [ ] Script de verificación ejecutado: `.\.Deploy\verify-service-connections.ps1`
  - [ ] Output: "? Todas las service connections están configuradas"

- [ ] Pipeline existe en Azure DevOps
  - [ ] YAML: `.Deploy/azure-pipelines-stage.yml`
  - [ ] Repository: Platheo-Templates-API
  - [ ] Branch: main

---

## ?? Una Vez Completado

¡Ya tienes todo listo para el deployment automático!

La pipeline podrá:
- ? Construir la imagen Docker
- ? Subirla a ACR
- ? Desplegarla a Container Apps
- ? Verificar el deployment
- ? Todo automáticamente con cada push a main

---

**Última actualización**: 11/01/2025 - v1.0
