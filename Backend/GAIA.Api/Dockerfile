# -------------------------------
# Base image for runtime
# -------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# -------------------------------
# Build image with Node.js + SDK
# -------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install required tools for apt and Node.js
RUN apt-get update -y && \
    apt-get install -y --no-install-recommends \
        curl \
        gnupg \
        ca-certificates \
        apt-transport-https && \
    rm -rf /var/lib/apt/lists/*

# Install Node.js (v20 LTS)
RUN curl -sL https://deb.nodesource.com/setup_24.x | bash - && \
    apt-get install -y nodejs && \
    rm -rf /var/lib/apt/lists/*

# Copy project files for restore
COPY ["Backend/GAIA.Api/GAIA.Api.csproj", "Backend/GAIA.Api/"]
COPY ["Frontend/GAIA.Client/GAIA.Client.esproj", "Frontend/GAIA.Client/"]

# Restore .NET dependencies
RUN dotnet restore "Backend/GAIA.Api/GAIA.Api.csproj"

# Copy the rest of the solution
COPY . .

# Build Backend
WORKDIR "/src/Backend/GAIA.Api"
RUN dotnet build "GAIA.Api.csproj" -c Release -o /app/build

# Build Frontend (adjust 'build' vs 'dist' depending on your SPA framework)
WORKDIR "/src/Frontend/GAIA.Client"
RUN npm install && npm run build

# -------------------------------
# Publish Backend
# -------------------------------
FROM build AS publish
WORKDIR "/src/Backend/GAIA.Api"
RUN dotnet publish "GAIA.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -------------------------------
# Final image
# -------------------------------
FROM base AS final
WORKDIR /app

# Copy published backend
COPY --from=publish /app/publish .

# Copy frontend build output into wwwroot
# If React: replace 'dist' with 'build'
COPY --from=build /src/Frontend/GAIA.Client/dist ./wwwroot

ENTRYPOINT ["dotnet", "GAIA.Api.dll"]
