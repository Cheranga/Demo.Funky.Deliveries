param functionAppName string
param keyVaultName string

@secure()
param storageAccountConnectionString string

param smsConfigurationBaseUrl string
param sgName string

var smsConfigurationAuthToken = '@Microsoft.KeyVault(SecretUri=https://${keyVaultName}.vault.azure.net/secrets/SmsConfigurationAuthToken/)'
var storageAccountConnectionStringSecret = '@Microsoft.KeyVault(SecretUri=https://${keyVaultName}.vault.azure.net/secrets/StorageAccountConnectionString/)'
var appInsightsKeySecret = '@Microsoft.KeyVault(SecretUri=https://${keyVaultName}.vault.azure.net/secrets/AppInsightsKey/)'
var timeZone = 'AUS Eastern Standard Time'

resource productionSlotAppSettings 'Microsoft.Web/sites/config@2021-02-01' = {
  name: '${functionAppName}/appsettings'
  properties: {
    AzureWebJobsStorage: storageAccountConnectionString
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: storageAccountConnectionString
    WEBSITE_CONTENTSHARE: toLower(functionAppName)
    FUNCTIONS_EXTENSION_VERSION: '~4'
    APPINSIGHTS_INSTRUMENTATIONKEY: appInsightsKeySecret
    FUNCTIONS_WORKER_RUNTIME: 'dotnet'
    WEBSITE_TIME_ZONE: timeZone
    WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG: 1  
  }
}

resource stagingSlotAppSettings 'Microsoft.Web/sites/slots/config@2021-02-01' = {
  name: '${functionAppName}/Staging/appsettings'
  properties: {    
    SmsConfiguration__BaseUrl: smsConfigurationBaseUrl
    SmsConfiguration__AuthToken: smsConfigurationAuthToken    
    AzureWebJobsStorage: storageAccountConnectionString
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: storageAccountConnectionString
    WEBSITE_CONTENTSHARE: toLower(functionAppName)
    FUNCTIONS_EXTENSION_VERSION: '~4'
    APPINSIGHTS_INSTRUMENTATIONKEY: appInsightsKeySecret
    FUNCTIONS_WORKER_RUNTIME: 'dotnet'
    WEBSITE_TIME_ZONE: timeZone
    WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG: 1
  }
}
