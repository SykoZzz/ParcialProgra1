# ================================
# Build Stage
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solo los csproj primero (cacheo de dependencias)
COPY *.sln .
COPY PortalAcademicoApp/*.csproj ./PortalAcademicoApp/

# Restaurar dependencias
RUN dotnet restore "PortalAcademicoApp/PortalAcademicoApp.csproj"

# Copiar todo el código
COPY . .

# Publicar en modo Release
WORKDIR /src/PortalAcademicoApp
RUN dotnet publish "PortalAcademicoApp.csproj" -c Release -o /app/publish

# ================================
# Runtime Stage
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copiar binarios publicados desde el build
COPY --from=build /app/publish .

# Configuración de Render (escucha en el puerto dinámico asignado)
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Entrypoint
ENTRYPOINT ["dotnet", "PortalAcademicoApp.dll"]
