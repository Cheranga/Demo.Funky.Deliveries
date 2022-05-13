param sgName string
param location string = resourceGroup().location

@allowed([
  'nonprod'
  'prod'
])
param envType string = 'nonprod'

var storageSku = {
  nonprod: 'Standard_LRS'
  prod: 'Standard_GRS'
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: sgName
  location: location
  kind: 'StorageV2'
  sku: {
    name: storageSku[envType]    
  }  
}

output storageAccountConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
output subsResourceId string = resourceId('Microsoft.Storage/storageAccounts',sgName)
