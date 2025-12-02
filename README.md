![Header image](https://github.com/DougChisholm/App-Mod-Booster/blob/main/repo-header-booster.png)

# App-Mod-Booster
A project to show how GitHub coding agent can turn screenshots of a legacy app into a working proof-of-concept for a cloud native Azure replacement if the legacy database schema is also provided.

Steps to modernise an app:

1. Fork this repo 
2. In new repo replace the screenshots and sql schema (or keep the samples)
3. Open the coding agent and use app-mod-booster agent telling it "modernise my app"
4. When the app code is generated (can take up to 30 minutes) there will be a pull request to approve.
5. Now you can open VS Code and clone the repo 
6. Open terminal in VS Code and using the Azure CLI run "az login" to set subscription/context
7. Run the deploy.sh file (ensuring the settings in the bicep files are what you want - it will have resource group, names, SKUs set)

Note the deployment script current requires python to be installed.

Supporting slides for Microsoft Employees:
[Here](<https://microsofteur-my.sharepoint.com/:p:/g/personal/dchisholm_microsoft_com/IQAY41LQ12fjSIfFz3ha4hfFAZc7JQQuWaOrF7ObgxRK6f4?e=p6arJs>)

---

# Expense Management System - Modernized Application

A modern cloud-native expense management application built with .NET 8, Azure SQL Database, and Azure OpenAI.

## Features

- 💰 **Expense Management**: Create, view, approve, and track expenses
- 🤖 **AI Assistant**: Natural language interaction with Azure OpenAI
- 🔒 **Secure**: Azure AD authentication with Managed Identity
- 📊 **REST APIs**: Full Swagger documentation
- 🎨 **Modern UI**: Clean, responsive design with Bootstrap 5

## Quick Start

### Option 1: Deploy without AI Chat (Faster, Lower Cost)

```bash
./deploy.sh
```

### Option 2: Deploy with AI Chat (Full Experience)

```bash
./deploy-with-chat.sh
```

## Accessing the Application

After deployment:
- **Main App**: `https://app-expensemgmt-xyz.azurewebsites.net/Index`
- **Chat UI**: Open `chatui/index.html` in your browser
- **API Docs**: `https://app-expensemgmt-xyz.azurewebsites.net/swagger`

## Key Pages

1. **View Expenses** (`/Index`) - List all expenses with filtering
2. **Add Expense** (`/AddExpense`) - Create new expenses
3. **Approve Expenses** (`/Approve`) - Manager view to approve/reject
4. **Chat UI** - Natural language interaction (if deployed with GenAI)

## Security Features

✅ Azure AD-Only Authentication - No SQL passwords  
✅ Managed Identity - Secure service-to-service auth  
✅ HTTPS Only - Encrypted communication  
✅ Stored Procedures - SQL injection prevention  
✅ Role-Based Access - Employee and Manager roles

## Cost Estimate

- **Basic**: ~$75/month (App Service + SQL)
- **With AI**: ~$200/month (adds OpenAI + AI Search)

**Tip**: Delete resources when not in use:
```bash
az group delete --name rg-expensemgmt-demo --yes
```

## Troubleshooting

### Database Connection Errors
1. Check managed identity has database permissions
2. Verify firewall rules allow your IP
3. Check App Service environment variables

### Chat UI Not Working
1. Verify deployment used `deploy-with-chat.sh`
2. Check OpenAI endpoint is configured
3. Verify managed identity has proper roles

## Project Structure

```
.
├── bicep/                  # Infrastructure as Code (Bicep templates)
├── app/ExpenseManagement/  # .NET 8 Application
├── chatui/                # Chat UI (HTML/JS)
├── Database-Schema/       # SQL schema files
├── stored-procedures.sql  # Database stored procedures
├── deploy.sh             # Basic deployment script
└── deploy-with-chat.sh   # Full deployment with AI
```

## Azure Best Practices

This solution follows Azure best practices:
- ✅ Infrastructure as Code (Bicep)
- ✅ Managed Identity for authentication
- ✅ Azure AD-only SQL authentication
- ✅ Stable API versions
- ✅ Stored procedures for data access
- ✅ Error handling with user-friendly messages

Reference: [Azure Architecture Best Practices](https://learn.microsoft.com/en-us/azure/architecture/best-practices/index-best-practices)
