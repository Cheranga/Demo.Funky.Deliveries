param funcAppName string
param location string = resourceGroup().location
param planName string


resource functionAppProductionSlot 'Microsoft.Web/sites@2021-03-01' = {
  name: funcAppName
  location: location
  kind:'functionapp'
  identity:{
    type:'SystemAssigned'
  }    
  properties:{
    serverFarmId:planName            
  }  
}

resource functionAppStagingSlot 'Microsoft.Web/sites/slots@2021-03-01' = {
  name: '${functionAppProductionSlot.name}/Staging'
  location: location
  kind:'functionapp'
  identity:{
    type:'SystemAssigned'
  }  
  properties:{
    serverFarmId:planName   
    siteConfig:{
      autoSwapSlotName:'Production'
    }         
  }  
}


output productionPrincipalId string = functionAppProductionSlot.identity.principalId
output stagingPrincipalId string = functionAppStagingSlot.identity.principalId

