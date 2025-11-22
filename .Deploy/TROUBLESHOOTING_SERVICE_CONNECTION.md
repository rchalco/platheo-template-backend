# ?? Troubleshooting: Service Connection Name con Caracteres Especiales

## ? Problema

```
The pipeline is not valid. Job DeployToACA: Step input azureSubscription 
references service connection Suscripción Platheo which could not be found.
```

## ?? Causa

Azure DevOps YAML tiene problemas con **caracteres especiales** en nombres de service connections:
- ? Acentos (á, é, í, ó, ú)
- ? Ñ
- ? Espacios múltiples
- ?? Espacios simples (funcionan pero no recomendados)

## ? Solución

### Paso 1: Cambiar Nombre de la Service Connection

#### Opción A: Editar la Existente (Más Rápido)

1. **Ya estás en la ventana correcta** (Edit service connection)

2. **Cambiar el nombre**:
   ```
   Campo: Service Connection Name
   
   ? Antes: Suscripción Platheo
   ? Ahora: Suscripcion-Platheo
   ```

3. **Click "Save"**

4. **Verificar**:
   - La conexión ahora se llama: `Suscripcion-Platheo`
   - Estado: Ready ?
   - Grant access: Habilitado ?

#### Opción B: Crear Nueva (Si prefieres)

1. **Cancel** en el diálogo actual

2. **New service connection**

3. **Azure Resource Manager** ? Workload Identity federation ? Next

4. **Configurar**:
   ```
   Subscription: Suscripción Platheo
   Resource group: Platheo-template
   Service Connection Name: Suscripcion-Platheo
   ? Grant access permission to all pipelines
   ```

5. **Save**

6. **Eliminar la anterior** (opcional):
   - Lista de service connections
   - Click en "Suscripción Platheo" ? Delete

### Paso 2: Actualizar Pipeline

El archivo `.Deploy/azure-pipelines-stage.yml` ya fue actualizado con:

```yaml
variables:
  azureSubscription: 'Suscripcion-Platheo'  # ? Sin acento
```

### Paso 3: Commit y Push

```powershell
# Verificar cambios
git status

# Add y commit
git add .Deploy/azure-pipelines-stage.yml
git commit -m "fix: Update service connection name to avoid special characters"

# Push
git push origin main
```

### Paso 4: Ejecutar Pipeline

1. Ve a Azure DevOps ? Pipelines
2. Ejecuta la pipeline
3. ? Debería funcionar ahora

---

## ?? Nombres Recomendados para Service Connections

### ? Buenos Nombres

```
? Suscripcion-Platheo
? suscripcion-platheo
? platheo-subscription
? platheo-azure-rm
? azure-rm-stage
? AzureResourceManager-Stage
```

### ? Nombres a Evitar

```
? Suscripción Platheo (acento)
? Suscripción  Platheo (doble espacio)
? Platheo's Subscription (apóstrofe)
? Azure (ñ o ã)
```

### ?? Convenciones Recomendadas

**Para Azure Resource Manager:**
```
[servicio]-[ambiente]
Ejemplos:
- azure-rm-stage
- azure-rm-prod
- platheo-azure-stage
```

**Para Docker Registry:**
```
acr-[nombre]-[ambiente]
Ejemplos:
- acr-platheo-stage
- acr-templates-prod
```

---

## ?? Verificación

### Verificar Nombre de la Service Connection

```powershell
# Listar service connections
az devops service-endpoint list `
  --organization "https://dev.azure.com/platheoinc" `
  --project "Platheo-Templates" `
  --query "[].{Name:name, Type:type}" `
  -o table
```

**Output esperado:**
```
Name                      Type
------------------------  ---------------
acrplatheotemplatestg     dockerregistry
Suscripcion-Platheo       azurerm
```

### Verificar Pipeline YAML

```powershell
# Ver el valor de azureSubscription
Get-Content .Deploy\azure-pipelines-stage.yml | Select-String "azureSubscription"
```

**Output esperado:**
```yaml
azureSubscription: 'Suscripcion-Platheo'
```

---

## ?? Checklist de Corrección

- [ ] Service connection renombrada a: `Suscripcion-Platheo`
- [ ] Sin acentos en el nombre
- [ ] Pipeline actualizada con el nuevo nombre
- [ ] Cambios commiteados y pusheados
- [ ] Grant access permission habilitado
- [ ] Verificación con `az devops service-endpoint list`
- [ ] Pipeline ejecutada exitosamente

---

## ?? Otros Archivos a Actualizar (Si Aplica)

Si tienes otros archivos que referencian la service connection:

### README.md
```markdown
# Antes
azureSubscription: 'Suscripción Platheo'

# Después
azureSubscription: 'Suscripcion-Platheo'
```

### Scripts de PowerShell
```powershell
# Antes
$serviceConnection = "Suscripción Platheo"

# Después
$serviceConnection = "Suscripcion-Platheo"
```

---

## ?? Lecciones Aprendidas

### 1. Caracteres Especiales en YAML
Azure DevOps YAML es sensible a caracteres especiales en referencias a recursos.

### 2. Nombrado Consistente
Usa nombres simples, sin acentos, preferiblemente en inglés:
- ? `azure-rm-stage`
- ? `Suscripción Platheo`

### 3. Convención de Nombres
Establece una convención desde el inicio:
```
[recurso]-[proyecto]-[ambiente]
Ejemplo: azure-rm-platheo-stage
```

### 4. Documentación
Documenta los nombres de service connections en el README del proyecto.

---

## ?? Próximos Pasos

1. ? Nombre de service connection corregido
2. ? Pipeline actualizada
3. ?? Commit y push de cambios
4. ?? Ejecutar pipeline
5. ? Verificar deployment exitoso

---

## ?? Comandos Útiles

```powershell
# Verificar service connections
az devops service-endpoint list --project "Platheo-Templates" -o table

# Verificar pipeline YAML localmente
Get-Content .Deploy\azure-pipelines-stage.yml

# Ver diferencias antes de commit
git diff .Deploy\azure-pipelines-stage.yml

# Commit y push
git add .Deploy\azure-pipelines-stage.yml
git commit -m "fix: Update service connection name"
git push origin main
```

---

**Última actualización**: 11/01/2025 - v1.0
**Problema resuelto**: ? Caracteres especiales en service connection names
