# ─── Variables ───────────────────────────────────────────────
RG="rg-mini-billing"
LOCATION="eastus"
APP_PLAN="plan-mini-billing-bioetario"
APP_NAME="app-mini-billing-api-bioetario"         
APIM_NAME="apim-mini-billing"            
APIM_EMAIL="bioetario@hotmail.com"
APIM_ORG="MiBillingOrg"

# ─── 1. Resource Group ───────────────────────────────────────
az group create \
  --name $RG \
  --location $LOCATION

# ─── 2. App Service Plan (Linux, Free tier para demo) ────────
az appservice plan create \
  --name $APP_PLAN \
  --resource-group $RG \
  --location $LOCATION \
  --sku B1 \
  --is-linux

# ─── 3. App Service (Web App) ────────────────────────────────
az webapp create \
  --name $APP_NAME \
  --resource-group $RG \
  --plan $APP_PLAN \
  --runtime "DOTNET|8.0"

# ─── 4. Habilitar System-Assigned Managed Identity ───────────
az webapp identity assign \
  --name $APP_NAME \
  --resource-group $RG

# ─── 5. App Settings (config básica) ─────────────────────────
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RG \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    Billing__MaxAmount=1000000

# ─── 6. API Management (Developer tier — ~45 min en provisionar)
az apim create \
  --name $APIM_NAME \
  --resource-group $RG \
  --location $LOCATION \
  --publisher-email $APIM_EMAIL \
  --publisher-name $APIM_ORG \
  --sku-name Developer \
  --sku-capacity 1

# ─── 7. Verificar que todo existe ────────────────────────────
az resource list --resource-group $RG --output table