param kvName string
param location string = resourceGroup().location

@secure()
param funcAppProdSlotId string

@secure()
param funcAppStagingSlotId string

@secure()
param secretData object


resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01-preview' = {
  name: kvName
  location: location
  properties: {
    enabledForDeployment: true
    enabledForTemplateDeployment: true
    enabledForDiskEncryption: true
    sku: {
      family: 'A'
      name: 'standard'
    }    
    tenantId: subscription().tenantId
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: funcAppProdSlotId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
      {
        tenantId: subscription().tenantId
        objectId: funcAppStagingSlotId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }        
    ]  
  }    
}

// Keyvault secrets - storage account connection string
resource keyVaultSecret 'Microsoft.KeyVault/vaults/secrets@2021-06-01-preview'=[for item in secretData.items:{
  name: '${keyVault.name}/${item.name}'
  properties: {
    value:'${item.value}'
  }  
}]

output keyVaultUri string = keyVault.properties.vaultUri
