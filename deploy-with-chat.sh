#!/bin/bash

set -e

echo "=== Expense Management System Deployment with GenAI ==="
echo ""

# Get current user information for Azure AD authentication
CURRENT_USER=$(az ad signed-in-user show --query userPrincipalName -o tsv)
ADMIN_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv)

if [ -z "$CURRENT_USER" ] || [ -z "$ADMIN_OBJECT_ID" ]; then
    echo "Error: Unable to get current user information. Please ensure you are logged in with 'az login'"
    exit 1
fi

echo "Deploying as: $CURRENT_USER"
echo "Admin Object ID: $ADMIN_OBJECT_ID"
echo ""

# Set variables
RESOURCE_GROUP="rg-expensemgmt-demo"
LOCATION="uksouth"

# Create resource group if it doesn't exist
echo "Creating resource group..."
az group create --name $RESOURCE_GROUP --location $LOCATION --output none

# Deploy infrastructure with GenAI
echo ""
echo "Deploying Azure infrastructure with GenAI resources..."
DEPLOYMENT_OUTPUT=$(az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file bicep/main.bicep \
  --parameters adminObjectId=$ADMIN_OBJECT_ID adminLogin=$CURRENT_USER deployGenAI=true \
  --query 'properties.outputs' \
  --output json)

# Extract outputs
APP_SERVICE_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.appServiceName.value')
SQL_SERVER_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.sqlServerName.value')
SQL_SERVER_FQDN=$(echo $DEPLOYMENT_OUTPUT | jq -r '.sqlServerFqdn.value')
DATABASE_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.databaseName.value')
MANAGED_IDENTITY_CLIENT_ID=$(echo $DEPLOYMENT_OUTPUT | jq -r '.managedIdentityClientId.value')
OPENAI_ENDPOINT=$(echo $DEPLOYMENT_OUTPUT | jq -r '.openAIEndpoint.value')
OPENAI_MODEL_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.openAIModelName.value')
SEARCH_ENDPOINT=$(echo $DEPLOYMENT_OUTPUT | jq -r '.searchEndpoint.value')
MANAGED_IDENTITY_NAME="mid-appmodassist-$(echo $RESOURCE_GROUP | md5sum | cut -c1-13)"

echo ""
echo "Deployment outputs:"
echo "  App Service: $APP_SERVICE_NAME"
echo "  SQL Server: $SQL_SERVER_NAME"
echo "  Database: $DATABASE_NAME"
echo "  Managed Identity Client ID: $MANAGED_IDENTITY_CLIENT_ID"
echo "  OpenAI Endpoint: $OPENAI_ENDPOINT"
echo "  OpenAI Model: $OPENAI_MODEL_NAME"
echo "  Search Endpoint: $SEARCH_ENDPOINT"
echo ""

# Configure App Service settings including GenAI
echo "Configuring App Service settings..."
az webapp config appsettings set \
  --resource-group $RESOURCE_GROUP \
  --name $APP_SERVICE_NAME \
  --settings \
    "SqlServer=$SQL_SERVER_FQDN" \
    "Database=$DATABASE_NAME" \
    "ManagedIdentityClientId=$MANAGED_IDENTITY_CLIENT_ID" \
    "OpenAI__Endpoint=$OPENAI_ENDPOINT" \
    "OpenAI__DeploymentName=$OPENAI_MODEL_NAME" \
    "SearchEndpoint=$SEARCH_ENDPOINT" \
    "AZURE_CLIENT_ID=$MANAGED_IDENTITY_CLIENT_ID" \
  --output none

echo "Waiting 30 seconds for SQL Server to be fully ready..."
sleep 30

# Add current user's IP to firewall
echo ""
echo "Adding your IP to SQL Server firewall..."
MY_IP=$(curl -s https://api.ipify.org)
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name "AllowDeployerIP" \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP \
  --output none

# Install Python dependencies
echo ""
echo "Installing Python dependencies..."
pip3 install --quiet pyodbc azure-identity

# Update Python scripts with actual values
echo ""
echo "Updating Python scripts with deployment values..."
sed -i.bak "s/SERVER = \"example.database.windows.net\"/SERVER = \"$SQL_SERVER_FQDN\"/g" run-sql.py && rm -f run-sql.py.bak
sed -i.bak "s/DATABASE = \"Northwind\"/DATABASE = \"$DATABASE_NAME\"/g" run-sql.py && rm -f run-sql.py.bak

sed -i.bak "s/SERVER = \"example.database.windows.net\"/SERVER = \"$SQL_SERVER_FQDN\"/g" run-sql-dbrole.py && rm -f run-sql-dbrole.py.bak
sed -i.bak "s/DATABASE = \"Northwind\"/DATABASE = \"$DATABASE_NAME\"/g" run-sql-dbrole.py && rm -f run-sql-dbrole.py.bak

sed -i.bak "s/SERVER = \"example.database.windows.net\"/SERVER = \"$SQL_SERVER_FQDN\"/g" run-sql-stored-procs.py && rm -f run-sql-stored-procs.py.bak
sed -i.bak "s/DATABASE = \"Northwind\"/DATABASE = \"$DATABASE_NAME\"/g" run-sql-stored-procs.py && rm -f run-sql-stored-procs.py.bak

sed -i.bak "s/MANAGED-IDENTITY-NAME/$MANAGED_IDENTITY_NAME/g" script.sql && rm -f script.sql.bak

# Import database schema
echo ""
echo "Importing database schema..."
python3 run-sql.py

# Configure database roles for managed identity
echo ""
echo "Configuring database roles for managed identity..."
python3 run-sql-dbrole.py

# Deploy stored procedures
echo ""
echo "Deploying stored procedures..."
python3 run-sql-stored-procs.py

# Deploy application
echo ""
echo "Deploying application code..."
az webapp deploy \
  --resource-group $RESOURCE_GROUP \
  --name $APP_SERVICE_NAME \
  --src-path ./app.zip \
  --type zip

# Deploy Chat UI to Azure App Service
echo ""
echo "Deploying Chat UI..."
az webapp deployment source config-zip \
  --resource-group $RESOURCE_GROUP \
  --name $APP_SERVICE_NAME \
  --src chatui/index.html

echo ""
echo "=== Deployment Complete ==="
echo ""
echo "Application URL: https://$APP_SERVICE_NAME.azurewebsites.net/Index"
echo "Chat UI: https://$APP_SERVICE_NAME.azurewebsites.net (or open chatui/index.html locally and point to your app URL)"
echo "Swagger API Documentation: https://$APP_SERVICE_NAME.azurewebsites.net/swagger"
echo ""
echo "Note: It may take a few minutes for the application and AI services to start."
echo ""
echo "To use the Chat UI locally:"
echo "  1. Open chatui/index.html in your browser"
echo "  2. The chat will connect to https://$APP_SERVICE_NAME.azurewebsites.net"
echo ""
