FROM ghcr.io/sistecsur/dotnet-runtime:8.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

ARG NUGET_USERNAME
ARG NUGET_TOKEN

COPY ["nuget.config", "."]
COPY ["src/TasaCambio.Worker/TasaCambio.Worker.csproj", "src/TasaCambio.Worker/"]
COPY ["src/TasaCambio.Application/TasaCambio.Application.csproj", "src/TasaCambio.Application/"]
COPY ["src/TasaCambio.Domain/TasaCambio.Domain.csproj", "src/TasaCambio.Domain/"]
COPY ["src/TasaCambio.Infrastructure/TasaCambio.Infrastructure.csproj", "src/TasaCambio.Infrastructure/"]

RUN dotnet restore "src/TasaCambio.Worker/TasaCambio.Worker.csproj"

COPY . .

RUN dotnet build "src/TasaCambio.Worker/TasaCambio.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/TasaCambio.Worker/TasaCambio.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TasaCambio.Worker.dll"]
