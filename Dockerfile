FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/TasaCambio.Presentacion/TasaCambio.Presentacion.csproj", "src/TasaCambio.Presentacion/"]
COPY ["src/TasaCambio.Aplicacion/TasaCambio.Aplicacion.csproj", "src/TasaCambio.Aplicacion/"]
COPY ["src/TasaCambio.Dominio/TasaCambio.Dominio.csproj", "src/TasaCambio.Dominio/"]
COPY ["src/TasaCambio.Infraestructura/TasaCambio.Infraestructura.csproj", "src/TasaCambio.Infraestructura/"]

RUN dotnet restore "src/TasaCambio.Presentacion/TasaCambio.Presentacion.csproj"

COPY . .

RUN dotnet build "src/TasaCambio.Presentacion/TasaCambio.Presentacion.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/TasaCambio.Presentacion/TasaCambio.Presentacion.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TasaCambio.Presentacion.dll"]
