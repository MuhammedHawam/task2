FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS base

WORKDIR /src
COPY . .

RUN dotnet publish ./GAIA.Api/GAIA.Api.csproj -c Release -o /app



FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble AS runtime

WORKDIR /app

RUN apt-get clean
RUN apt-get update && apt-get upgrade -y
RUN apt-get purge -y --autoremove curl

COPY --from=base --chown=app:app --chmod=555 /app .

## ASPNET Environment Configuration
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
  
USER app
EXPOSE 8080
ENTRYPOINT ["./GAIA.Api"]
