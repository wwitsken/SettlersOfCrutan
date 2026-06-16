targetScope = 'subscription'

param resourceGroupName string

param location string

param principalId string

resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
}

module aca_env_acr 'aca-env-acr/aca-env-acr.bicep' = {
  name: 'aca-env-acr'
  scope: rg
  params: {
    location: location
  }
}

module aca_env 'aca-env/aca-env.bicep' = {
  name: 'aca-env'
  scope: rg
  params: {
    location: location
    aca_env_acr_outputs_name: aca_env_acr.outputs.name
    userPrincipalId: principalId
  }
}

output aca_env_AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = aca_env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN

output aca_env_AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = aca_env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID

output aca_env_AZURE_CONTAINER_REGISTRY_ENDPOINT string = aca_env.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT

output aca_env_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = aca_env.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID