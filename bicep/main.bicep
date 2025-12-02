targetScope = 'resourceGroup'

@description('Location for all resources')
param location string = resourceGroup().location

@description('Admin Object ID for Azure AD authentication')
param adminObjectId string

@description('Admin Login (UPN) for Azure AD authentication')
param adminLogin string

@description('Deploy GenAI resources (Azure OpenAI, AI Search)')
param deployGenAI bool = false

// Generate base name
var baseName = 'expensemgmt-${uniqueString(resourceGroup().id)}'

// Deploy App Service and Managed Identity
module appService 'app-service.bicep' = {
  name: 'app-service-deployment'
  params: {
    location: location
    baseName: baseName
  }
}

// Deploy Azure SQL
module azureSql 'azure-sql.bicep' = {
  name: 'azure-sql-deployment'
  params: {
    location: location
    baseName: baseName
    managedIdentityPrincipalId: appService.outputs.managedIdentityPrincipalId
    adminObjectId: adminObjectId
    adminLogin: adminLogin
  }
}

// Conditionally deploy GenAI resources
module genAI 'genai.bicep' = if (deployGenAI) {
  name: 'genai-deployment'
  params: {
    baseName: baseName
    managedIdentityPrincipalId: appService.outputs.managedIdentityPrincipalId
  }
}

// Outputs
output appServiceName string = appService.outputs.appServiceName
output appServiceUrl string = appService.outputs.appServiceUrl
output managedIdentityClientId string = appService.outputs.managedIdentityClientId
output sqlServerName string = azureSql.outputs.sqlServerName
output sqlServerFqdn string = azureSql.outputs.sqlServerFqdn
output databaseName string = azureSql.outputs.databaseName

// Conditional GenAI outputs (null-safe)
output openAIEndpoint string = deployGenAI ? genAI.outputs.openAIEndpoint : ''
output openAIModelName string = deployGenAI ? genAI.outputs.openAIModelName : ''
output searchEndpoint string = deployGenAI ? genAI.outputs.searchEndpoint : ''
