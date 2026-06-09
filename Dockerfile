FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/TasaCambio.Presentation/TasaCambio.Presentation.csproj", "src/TasaCambio.Presentation/"]
COPY ["src/TasaCambio.Application/TasaCambio.Application.csproj", "src/TasaCambio.Application/"]
COPY ["src/TasaCambio.Domain/TasaCambio.Domain.csproj", "src/TasaCambio.Domain/"]
COPY ["src/TasaCambio.Infrastructure/TasaCambio.Infrastructure.csproj", "src/TasaCambio.Infrastructure/"]

RUN dotnet restore "src/TasaCambio.Presentation/TasaCambio.Presentation.csproj"

COPY . .

RUN dotnet build "src/TasaCambio.Presentation/TasaCambio.Presentation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/TasaCambio.Presentation/TasaCambio.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TasaCambio.Presentation.dll"]
