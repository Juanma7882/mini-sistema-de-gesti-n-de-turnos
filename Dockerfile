# Build del backend (Turnos.Api) para Railway. Contexto de build: la raíz del
# repo (necesita ver backend/ entero); frontend/ se despliega aparte en Vercel.

# --- build ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar primero los .csproj (y el .sln) de las 4 capas y restaurar: mientras no
# cambien las dependencias, Docker cachea esta capa y no vuelve a bajar paquetes
# en cada build por un cambio de código.
COPY backend/Turnos.sln backend/
COPY backend/src/Domain/Turnos.Domain.csproj backend/src/Domain/
COPY backend/src/Application/Turnos.Application.csproj backend/src/Application/
COPY backend/src/Infrastructure/Turnos.Infrastructure.csproj backend/src/Infrastructure/
COPY backend/src/Api/Turnos.Api.csproj backend/src/Api/
RUN dotnet restore backend/src/Api/Turnos.Api.csproj

COPY backend/src backend/src
RUN dotnet publish backend/src/Api/Turnos.Api.csproj -c Release -o /app --no-restore

# --- runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080

# Railway inyecta PORT dinámicamente; el fallback a 8080 hace que la misma
# imagen también sirva para "docker run -p 8080:8080" en local.
ENTRYPOINT ["sh", "-c", "dotnet Turnos.Api.dll --urls http://+:${PORT:-8080}"]
