param storageAccountName string

@secure()
param functionAppName string

var roleDefinitions = {
  queuereadwrite: '974c5e8b-45b9-4653-ba55-5f855dd0fb88' //  storage queue data contributor
  tablewrite: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3' //  Storage Table Data Contributor
}

resource app 'Microsoft.Web/sites@2021-03-01' existing = {
  name: functionAppName
  scope:resourceGroup()
}

resource stagingApp 'Microsoft.Web/sites/slots@2021-03-01' existing = {
  name: '${functionAppName}/Staging'
}

resource queueRole 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: roleDefinitions['queuereadwrite']
}

resource tableRole 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: roleDefinitions['tablewrite']
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-02-01' existing = {
  scope: resourceGroup()  
  name: storageAccountName
}

resource queueRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroup().id, replace('${functionAppName}queueowner', '-', ''), queueRole.id)  
  scope:storageAccount
  properties: {
    roleDefinitionId: queueRole.id
    principalId: app.identity.principalId
    principalType: 'ServicePrincipal'
  }  
}

resource tableRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroup().id, replace('${functionAppName}tableowner', '-', ''), tableRole.id)  
  scope:storageAccount
  properties: {
    roleDefinitionId: tableRole.id
    principalId: app.identity.principalId
    principalType: 'ServicePrincipal'
  }  
}

resource queueRoleAssignmentStaging 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroup().id, replace('${functionAppName}stagingqueueowner', '-', ''), queueRole.id)  
  scope:storageAccount
  properties: {
    roleDefinitionId: queueRole.id
    principalId: stagingApp.identity.principalId
    principalType: 'ServicePrincipal'
  }  
}

resource tableRoleAssignmentStaging 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroup().id, replace('${functionAppName}stagingtableowner', '-', ''), tableRole.id)  
  scope:storageAccount
  properties: {
    roleDefinitionId: tableRole.id
    principalId: stagingApp.identity.principalId
    principalType: 'ServicePrincipal'
  }  
}

