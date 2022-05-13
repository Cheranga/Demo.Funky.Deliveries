@allowed([
  'nonprod'
  'prod'
])
param envType string = 'nonprod'
param environmentName string
param buildNumber string
param location string = resourceGroup().location
param funcAppName string
param sgName string
param smsConfigurationBaseUrl string

@secure()
param smsConfigurationAuthToken string

var appInsightsName = 'ai-${funcAppName}-${environmentName}'
var aspName = 'asp-${funcAppName}-${environmentName}'
var kvName = 'kv-${funcAppName}-${environmentName}'
var functionAppName = 'func-${funcAppName}-${environmentName}'

// Storage account
module storageAccount 'StorageAccount/template.bicep' = {
  name: '${buildNumber}-storage-account'
  params: {
    sgName: sgName
    location: location
    envType: envType
  }
}

// Application insights
module appInsights 'ApplicationInsights/template.bicep' = {
  name: '${buildNumber}-application-insights'
  params: {
    name: appInsightsName
    location: location
  }
}

// Application service plan
module appServicePlan 'AppServicePlan/template.bicep' = {
  name: '${buildNumber}-app-service-plan'
  params: {
    name: aspName
    location: location
  }
}

// Function app with no settings
module functionAppWithNoSettings 'FunctionApp/functionapp.bicep' = {
  name: '${buildNumber}-function-app'
  params: {
    funcAppName: functionAppName
    planName: appServicePlan.outputs.planId
    location: location
  }
}

// Keyvault with secrets
module keyVaultModule 'KeyVault/template.bicep' = {
  name: '${buildNumber}-key-vault'
  params: {
    kvName: kvName
    location: location
    funcAppProdSlotId: functionAppWithNoSettings.outputs.productionPrincipalId
    funcAppStagingSlotId: functionAppWithNoSettings.outputs.stagingPrincipalId
    secretData: {
      items: [
        {
          name: 'StorageAccountConnectionString'
          value: storageAccount.outputs.storageAccountConnectionString
        }        
        {
          name: 'SmsConfigurationAuthToken'
          value: smsConfigurationAuthToken
        }        
        {
          name: 'AppInsightsKey'
          value: appInsights.outputs.appInsightsKey
        }
      ]
    }
  }
  dependsOn: [
    storageAccount
  ]
}

// Function app settings
module functionAppSettings 'FunctionApp/functionappsettings.bicep' = {
  name: '${buildNumber}-functionapp-settings'
  params: {
    functionAppName: functionAppName
    keyVaultName: kvName
    storageAccountConnectionString: storageAccount.outputs.storageAccountConnectionString
    smsConfigurationBaseUrl: smsConfigurationBaseUrl
    sgName: sgName
  }
  dependsOn:[
    storageAccount
    appInsights
    appServicePlan
    functionAppWithNoSettings
    keyVaultModule
  ]
}

// // Assigning RBAC to function app
// module functionAppToStorageRbac 'RBAC/functionapptostorageaccount.bicep' = {
//   name: '${buildNumber}-functionapp-to-storage'
//   params: {
//     functionAppName: functionAppName
//     storageAccountName: sgName
//   }
//   dependsOn:[
//     functionAppWithNoSettings
//     functionAppSettings
//     storageAccount
//   ]
// }
