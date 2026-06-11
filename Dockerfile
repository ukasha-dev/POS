# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (better layer caching)
COPY SmartPOS.csproj ./
RUN dotnet restore SmartPOS.csproj

# Copy the rest and publish (generates the static-web-assets manifest
# that MapStaticAssets needs to serve blazor.web.js + scoped CSS)
COPY . .
RUN dotnet publish SmartPOS.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Railway provides $PORT; bind Kestrel to it (fallback 8080 for local docker run)
CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet SmartPOS.dll"]
