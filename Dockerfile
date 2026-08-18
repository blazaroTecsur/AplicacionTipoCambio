# =====================================
# BUILD
# =====================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG NUGET_USERNAME
ARG NUGET_TOKEN
ENV NUGET_USERNAME=${NUGET_USERNAME}
ENV NUGET_TOKEN=${NUGET_TOKEN}

WORKDIR /src

COPY nuget.config .
COPY . .

RUN dotnet restore 

RUN dotnet publish TasaCambio.Worker/TasaCambio.Worker.csproj \
    -c Release \
    -o /app/publish

# =====================================
# RUNTIME
# =====================================

FROM ghcr.io/sistecsur/dotnet-runtime:8.1

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TasaCambio.Worker.dll"]