# ?? Mejores Prácticas para Tasks en Azure DevOps Pipelines

## ?? Error Común: Bash@3 sin Autenticación

### ? Problema

```yaml
- task: Bash@3
  displayName: 'Verify Image in ACR'
  inputs:
    targetType: 'inline'
    script: |
      az acr repository show-tags --name $(acrName) --repository $(imageName)
```

**Error:**
```
WARNING: Please run 'az login' to setup account.
ERROR: Could not connect to the registry login server
```

**Causa:** El task `Bash@3` no tiene autenticación con Azure automáticamente.

### ? Solución

```yaml
- task: AzureCLI@2
  displayName: 'Verify Image in ACR'
  inputs:
    azureSubscription: '$(azureSubscription)'
    scriptType: 'bash'
    scriptLocation: 'inlineScript'
    inlineScript: |
      az acr repository show-tags --name $(acrName) --repository $(imageName)
```

**Ventajas:**
- ? Autenticación automática con Azure
- ? Usa la service connection configurada
- ? No requiere `az login` manual

---

## ?? Comparativa de Tasks

### Para Scripts con Azure CLI

| Task | Autenticación Azure | Cuándo Usar |
|------|---------------------|-------------|
| `AzureCLI@2` | ? Automática | Comandos `az` (Azure CLI) |
| `Bash@3` | ? Manual | Scripts bash sin Azure |
| `PowerShell@2` | ? Manual | Scripts PowerShell sin Azure |
| `AzurePowerShell@5` | ? Automática | Comandos Azure PowerShell |

### Para Docker

| Task | Autenticación Docker | Cuándo Usar |
|------|---------------------|-------------|
| `Docker@2` | ? Con containerRegistry | Build, push, pull de imágenes |
| `Bash@3` + docker | ? Manual | Scripts Docker sin service connection |

### Para Deployments

| Task | Autenticación | Cuándo Usar |
|------|---------------|-------------|
| `AzureCLI@2` | ? Automática | Cualquier comando Azure CLI |
| `AzureContainerApps@1` | ? Automática | Deploy específico a Container Apps |
| `AzureWebAppContainer@1` | ? Automática | Deploy a App Service Container |

---

## ? Ejemplos Correctos

### 1. Verificar Imagen en ACR

```yaml
- task: AzureCLI@2
  displayName: 'Verify Image in ACR'
  inputs:
    azureSubscription: '$(azureSubscription)'
    scriptType: 'bash'
    scriptLocation: 'inlineScript'
    inlineScript: |
      echo "?? Verificando imagen en ACR..."
      az acr repository show-tags \
        --name $(acrName) \
        --repository $(imageName) \
        --orderby time_desc \
        --top 5 \
        --output table
```

### 2. Actualizar Container App

```yaml
- task: AzureCLI@2
  displayName: 'Update Container App'
  inputs:
    azureSubscription: '$(azureSubscription)'
    scriptType: 'bash'
    scriptLocation: 'inlineScript'
    inlineScript: |
      az containerapp update \
        --name $(containerAppName) \
        --resource-group $(resourceGroup) \
        --image $(acrLoginServer)/$(imageName):$(imageTag)
```

### 3. Health Check (No requiere Azure)

```yaml
- task: Bash@3
  displayName: 'Health Check'
  inputs:
    targetType: 'inline'
    script: |
      APP_URL="https://$(containerAppUrl)"
      HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$APP_URL/health")
      
      if [ "$HTTP_CODE" == "200" ]; then
        echo "? Health check exitoso"
      else
        echo "? Health check falló (HTTP $HTTP_CODE)"
        exit 1
      fi
```

### 4. Docker Build y Push

```yaml
- task: Docker@2
  displayName: 'Build and Push'
  inputs:
    command: buildAndPush
    repository: '$(imageName)'
    dockerfile: '$(dockerfilePath)'
    containerRegistry: '$(acrName)'
    tags: |
      $(imageTag)
      latest
```

---

## ?? Troubleshooting

### Error: "az: command not found"

**Causa:** Azure CLI no está instalado en el agent.

**Solución:**
```yaml
# Azure Pipelines agents ya tienen Azure CLI instalado
# Si usas self-hosted agent:
- task: AzureCLI@2  # Esto asegura que Azure CLI esté disponible
```

### Error: "Please run 'az login'"

**Causa:** Usando `Bash@3` o `PowerShell@2` con comandos Azure.

**Solución:** Cambiar a `AzureCLI@2` o `AzurePowerShell@5`

```yaml
# ? Incorrecto
- task: Bash@3
  inputs:
    script: az acr list

# ? Correcto
- task: AzureCLI@2
  inputs:
    azureSubscription: '$(azureSubscription)'
    scriptType: 'bash'
    inlineScript: az acr list
```

### Error: "Could not connect to registry"

**Causa:** Falta autenticación o nombre incorrecto.

**Solución:**
```yaml
- task: AzureCLI@2
  inputs:
    azureSubscription: '$(azureSubscription)'  # ? Service connection correcta
    scriptType: 'bash'
    inlineScript: |
      # Verificar que el nombre sea correcto (sin .azurecr.io)
      az acr repository show-tags --name acrplatheotemplatestg --repository platheo-api
```

---

## ?? Plantillas Recomendadas

### Template: Azure CLI Task

```yaml
- task: AzureCLI@2
  displayName: 'Descripción de la tarea'
  inputs:
    azureSubscription: '$(azureSubscription)'
    scriptType: 'bash'  # o 'ps' para PowerShell
    scriptLocation: 'inlineScript'
    inlineScript: |
      # Tus comandos Azure CLI aquí
      echo "Ejecutando comandos..."
      az <comando>
```

### Template: Docker Task

```yaml
- task: Docker@2
  displayName: 'Descripción de la operación Docker'
  inputs:
    command: 'build'  # o 'push', 'buildAndPush', 'login'
    repository: '$(imageName)'
    dockerfile: '$(dockerfilePath)'
    containerRegistry: '$(acrName)'
    tags: |
      $(imageTag)
      latest
    arguments: '--build-arg KEY=VALUE'
```

### Template: Bash Script (sin Azure)

```yaml
- task: Bash@3
  displayName: 'Descripción del script'
  inputs:
    targetType: 'inline'
    script: |
      # Scripts bash generales (sin comandos Azure)
      echo "Ejecutando script..."
      curl https://example.com/health
```

---

## ?? Reglas de Oro

### 1. ¿Necesitas comandos Azure CLI?
```
? Usa: AzureCLI@2
? No uses: Bash@3 + az
```

### 2. ¿Necesitas Docker?
```
? Usa: Docker@2 con containerRegistry
? No uses: Bash@3 + docker (a menos que sea muy específico)
```

### 3. ¿Necesitas Azure PowerShell?
```
? Usa: AzurePowerShell@5
? No uses: PowerShell@2 + Connect-AzAccount
```

### 4. ¿Script simple sin Azure/Docker?
```
? Usa: Bash@3 o PowerShell@2
```

---

## ?? Checklist de Tasks

Antes de usar un task, verifica:

- [ ] ¿El script usa `az` (Azure CLI)?
  - **Sí** ? Usar `AzureCLI@2`
  - **No** ? Continuar

- [ ] ¿El script usa `docker`?
  - **Sí** ? Usar `Docker@2` si es posible
  - **No** ? Continuar

- [ ] ¿El script usa comandos Azure PowerShell?
  - **Sí** ? Usar `AzurePowerShell@5`
  - **No** ? Continuar

- [ ] ¿Es un script bash/PowerShell general?
  - **Sí** ? Usar `Bash@3` o `PowerShell@2`

---

## ?? Referencias

- [AzureCLI@2 Documentation](https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/deploy/azure-cli)
- [Docker@2 Documentation](https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/build/docker)
- [Bash@3 Documentation](https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/utility/bash)
- [Azure Pipelines Tasks Reference](https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/)

---

**Última actualización**: 11/01/2025 - v1.0
**Problema resuelto**: ? Bash@3 sin autenticación Azure
