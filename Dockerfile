# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos solo los .csproj primero para cachear el restore de paquetes
COPY *.sln .
COPY reputaly.api/*.csproj ./reputaly.Api/
RUN dotnet restore

# Copiamos todo el código y compilamos
COPY . .
WORKDIR /src/reputaly.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

ENTRYPOINT ["dotnet", "Reputaly.API.dll"]
