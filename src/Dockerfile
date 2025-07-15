# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first
COPY DKR.Web/DKR.Web.csproj DKR.Web/
COPY DKR.Core/DKR.Core.csproj DKR.Core/
COPY DKR.Infrastructure/DKR.Infrastructure.csproj DKR.Infrastructure/
COPY DKR.Infrastructure.OnPremise/DKR.Infrastructure.OnPremise.csproj DKR.Infrastructure.OnPremise/
COPY DKR.Shared/DKR.Shared.csproj DKR.Shared/

# Restore
RUN dotnet restore DKR.Web/DKR.Web.csproj

# Copy all source code
COPY . .

# Build
WORKDIR /src/DKR.Web
RUN dotnet build DKR.Web.csproj -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish DKR.Web.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DKR.Web.dll"]
