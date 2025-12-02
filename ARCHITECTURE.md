# Azure Architecture Diagram - Expense Management System

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Internet / Users                             │
└──────────────────┬──────────────────────────────────────────────────┘
                   │
                   │ HTTPS
                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     Azure App Service (S1)                           │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  ASP.NET Core 8.0 Application                                  │  │
│  │  - Razor Pages UI                                              │  │
│  │  - REST APIs (Swagger)                                         │  │
│  │  - Chat Service                                                │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                       │
│  User-Assigned Managed Identity: mid-AppModAssist-xxxxx             │
└────┬────────────────────┬────────────────────┬──────────────────────┘
     │                    │                    │
     │ SQL Auth           │ OpenAI Auth        │ Search Auth
     │ (Managed Identity) │ (Managed Identity) │ (Managed Identity)
     ▼                    ▼                    ▼
┌──────────────────┐ ┌─────────────────────┐ ┌──────────────────────┐
│  Azure SQL DB    │ │  Azure OpenAI       │ │  Azure AI Search     │
│                  │ │                     │ │                      │
│  - Northwind DB  │ │  - GPT-4o Model     │ │  - Basic Tier        │
│  - Basic Tier    │ │  - swedencentral    │ │  - Index Storage     │
│  - Stored Procs  │ │  - Capacity: 8      │ │                      │
│  - AD Auth Only  │ │                     │ │                      │
└──────────────────┘ └─────────────────────┘ └──────────────────────┘
         │                                             │
         │ Tables & Data                               │ RAG Documents
         ▼                                             ▼
┌──────────────────┐                         ┌──────────────────────┐
│  Database Schema │                         │  Document Storage    │
│  - Users         │                         │  (Future)            │
│  - Expenses      │                         │                      │
│  - Categories    │                         └──────────────────────┘
│  - Statuses      │
│  - Roles         │
└──────────────────┘


## Key Security Features

┌─────────────────────────────────────────────────────────────┐
│                    Managed Identity                          │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  • No passwords or keys in code                        │ │
│  │  • Automatic credential rotation                       │ │
│  │  • Azure RBAC for access control                       │ │
│  │  • Service-to-service authentication                   │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                Azure AD Authentication                       │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  • Azure AD-only SQL authentication                    │ │
│  │  • No SQL logins allowed                               │ │
│  │  • Enforced by Azure Policy                            │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘


## Data Flow

1. User Interaction
   ┌─────┐
   │ Web │ ──► User views expenses via Razor Pages
   │ UI  │ ──► User adds/approves via forms
   └─────┘

2. API Requests
   ┌─────┐
   │ API │ ──► REST endpoints for CRUD operations
   │     │ ──► Swagger documentation
   └─────┘

3. Database Access (via Stored Procedures)
   ┌──────┐
   │  SP  │ ──► GetExpenses
   │      │ ──► CreateExpense
   │      │ ──► ApproveExpense
   │      │ ──► All data access secured
   └──────┘

4. AI Chat (Optional)
   ┌─────┐
   │ Chat│ ──► Natural language queries
   │  UI │ ──► Function calling to APIs
   │     │ ──► GPT-4o responses
   └─────┘


## Deployment Options

Option 1: Basic Deployment (deploy.sh)
  ✓ App Service
  ✓ Azure SQL Database
  ✓ Managed Identity
  ✓ Web UI + APIs
  ~ Cost: ~$75/month

Option 2: Full Deployment (deploy-with-chat.sh)
  ✓ Everything from Option 1
  ✓ Azure OpenAI (GPT-4o)
  ✓ Azure AI Search
  ✓ Chat UI with AI
  ~ Cost: ~$200/month


## Resource Naming Convention

All resources follow lowercase naming:
  - Resource Group:     rg-expensemgmt-demo
  - App Service:        app-expensemgmt-{uniqueString}
  - Managed Identity:   mid-appmodassist-{uniqueString}
  - SQL Server:         sql-expensemgmt-{uniqueString}
  - Azure OpenAI:       aoai-expensemgmt-{uniqueString}
  - AI Search:          search-expensemgmt-{uniqueString}


## Azure Regions

  Primary Region:  UK South (uksouth)
    - App Service
    - SQL Database
    - AI Search
    - Resource Group

  Secondary Region: Sweden Central (swedencentral)
    - Azure OpenAI (GPT-4o)
    Note: Used to avoid quota issues for POC deployments


## Best Practices Applied

✓ Infrastructure as Code (Bicep)
✓ Managed Identity (no passwords)
✓ Stable API versions (no preview)
✓ Stored procedures (data access layer)
✓ HTTPS only
✓ TLS 1.2+ minimum
✓ Azure AD authentication
✓ Role-based access control
✓ Error handling with user-friendly messages
✓ Resource tagging and naming standards
```
