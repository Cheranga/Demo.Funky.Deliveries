param name string
param location string = resourceGroup().location

@allowed([
  'nonprod'
  'prod'
])
param envType string = 'nonprod'

var sku = {
  nonprod: 'Y1'
  prod: 'Premium'
}

var tier = {
  nonprod: 'Dynamic'
  prod: 'S1'
}

resource asp 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: name
  location: location
  sku: {
    name: sku[envType]
    tier: tier[envType]
  }
}

output planId string = asp.id
