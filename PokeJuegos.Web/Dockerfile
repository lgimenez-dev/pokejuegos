# Build context: the repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Project files first, so the restore layer is cached until a .csproj changes.
COPY PokeJuegos.Common/PokeJuegos.Common.csproj PokeJuegos.Common/
COPY PokeJuegos.DTO/PokeJuegos.DTO.csproj PokeJuegos.DTO/
COPY PokeJuegos.Web/PokeJuegos.Web.csproj PokeJuegos.Web/
RUN dotnet restore PokeJuegos.Web/PokeJuegos.Web.csproj

COPY PokeJuegos.Common/ PokeJuegos.Common/
COPY PokeJuegos.DTO/ PokeJuegos.DTO/
COPY PokeJuegos.Web/ PokeJuegos.Web/
RUN dotnet publish PokeJuegos.Web/PokeJuegos.Web.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
# The image listens on port 8080.
ENTRYPOINT ["dotnet", "PokeJuegos.Web.dll"]
