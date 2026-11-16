# Build context: the repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Project files first, so the restore layer is cached until a .csproj changes.
COPY PokeJuegos.Common/PokeJuegos.Common.csproj PokeJuegos.Common/
COPY PokeJuegos.DTO/PokeJuegos.DTO.csproj PokeJuegos.DTO/
COPY PokeJuegos.Domain/PokeJuegos.Domain.csproj PokeJuegos.Domain/
COPY PokeJuegos.Application/PokeJuegos.Application.csproj PokeJuegos.Application/
COPY PokeJuegos.Infrastructure/PokeJuegos.Infrastructure.csproj PokeJuegos.Infrastructure/
COPY PokeJuegos.Api/PokeJuegos.Api.csproj PokeJuegos.Api/
RUN dotnet restore PokeJuegos.Api/PokeJuegos.Api.csproj

COPY PokeJuegos.Common/ PokeJuegos.Common/
COPY PokeJuegos.DTO/ PokeJuegos.DTO/
COPY PokeJuegos.Domain/ PokeJuegos.Domain/
COPY PokeJuegos.Application/ PokeJuegos.Application/
COPY PokeJuegos.Infrastructure/ PokeJuegos.Infrastructure/
COPY PokeJuegos.Api/ PokeJuegos.Api/
RUN dotnet publish PokeJuegos.Api/PokeJuegos.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
# The image listens on port 8080.
ENTRYPOINT ["dotnet", "PokeJuegos.Api.dll"]
