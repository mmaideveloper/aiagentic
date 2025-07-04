#install choco install azd

azd env get-values

azd env set AZURE_SUBSCRIPTION_ID 5c0078f8-78f1-49be-9917-bdcc35f5e831
azd env set AZURE_RESOURCE_GROUP aiagents

azd auth login

azd provisioning
az deploy

az up


az containerapp exec --name documentintelligenceweb --resource-group aiagents

az containerapp logs show --name documentintelligenceweb --resource-group aiagents

az containerapp logs show --name documentprocessingapi --resource-group aiagents

1. add to azure app
'https://documentintelligenceweb.ashysea-4f2a10ed.eastus2.azurecontainerapps.io/signin-oidc

2. setup container in Ingress to allow public requests

 az containerapp update --name aiagents --resource-group aiagents --set configuration.ingress.external=true


--------------

az containerapp exec --name documentprocessingapi --resource-group aiagents

az containerapp update --name documentprocessingapi --resource-group aiagents --set securityContext.runAsUser=0


https://aspire-dashboard.ext.ashysea-4f2a10ed.eastus2.azurecontainerapps.io/



# Web App Chat`

## Table of Contents

0. [Local](#local)
1. [Web App](#web-app)
2. [Agent Document Intelligent](#what-was-added)
3. [Agent Analyzer ](#billing)
4. [Troubleshooting](#troubleshooting)

## Local

https://learn.microsoft.com/en-us/azure/ai-foundry/foundry-local/get-started

## Web App

$cert = Get-ChildItem -Path "Cert:\LocalMachine\My" | Where-Object {$_.Subject -match "CN=localhost"}
Export-PfxCertificate -Cert $cert -FilePath "./certificate.pfx" -Password (ConvertTo-SecureString -String "YourSecurePassword" -Force -AsPlainText)


## frond end

https://github.com/microsoft/chat-copilot/

https://github.com/Azure-Samples/get-started-with-ai-agents/blob/main/src/api/routes.py

##CORS
webapi - setup as external, note new link without  xxx-inteternal-xxx in url
https://ai-chat-webapi.internal.ashysea-4f2a10ed.eastus2.azurecontainerapps.io

for cors.
https://ai-chat-webapi.ashysea-4f2a10ed.eastus2.azurecontainerapps.io


##Authentication Chat
1. create sepearte app with spa
2. create separte app for api
   details here: https://github.com/microsoft/chat-copilot/?tab=readme-ov-file


##Agentic solution
https://learn.microsoft.com/en-us/azure/ai-foundry/agents/

plugins:
https://www.developerscantina.com/p/semantic-kernel-bing-graph-plugins/


##TODO

1. binaryContent - fix from memoryItem to file storage?