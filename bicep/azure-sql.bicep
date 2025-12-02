@description('Location for all resources')
param location string = resourceGroup().location

@description('Base name for resources')
param baseName string

@description('Managed Identity Principal ID for role assignments')
param managedIdentityPrincipalId string

@description('Admin Object ID for Azure AD authentication')
param adminObjectId string

@description('Admin Login (UPN) for Azure AD authentication')
param adminLogin string

// Create SQL Server
resource sqlServer 'Microsoft.Sql/servers@2021-11-01' = {
  name: 'sql-${baseName}'
  location: location
  properties: {
    administrators: {
      azureADOnlyAuthentication: true
      administratorType: 'ActiveDirectory'
      login: adminLogin
      sid: adminObjectId
      tenantId: subscription().tenantId
      principalType: 'User'
    }
    publicNetworkAccess: 'Enabled'
  }
}

// Create Database
resource database 'Microsoft.Sql/servers/databases@2021-11-01' = {
  parent: sqlServer
  name: 'Northwind'
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648 // 2GB
  }
}

// Allow Azure services to access the server
resource firewallRule 'Microsoft.Sql/servers/firewallRules@2021-11-01' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

output sqlServerName string = sqlServer.name
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output databaseName string = database.name
