# TasaCambio — Contexto del Proyecto

## Descripción
Worker Service .NET 8 que consulta el tipo de cambio de la SBS (Peru) y lo registra en una BD MySQL y en SyteLine (ERP Infor) via IDO.

## Arquitectura
Clean Architecture con 4 capas:
- **Domain** — entidades e interfaces de dominio
- **Application** — casos de uso con MediatR
- **Infrastructure** — EF Core (MySQL), cliente SBS, cliente Infor IDO
- **Worker** — `BackgroundService` (`SbsSyncWorker`) que orquesta el ciclo

## Comandos útiles

### Desarrollo local
```bash
dotnet build
dotnet test
dotnet run --project src/TasaCambio.Worker
```

### Docker
```bash
docker compose up -d --build        # levantar todo
docker compose logs -f worker       # ver logs en tiempo real
docker compose restart worker       # reiniciar solo el worker
```

## Variables de entorno requeridas (`.env`)
```env
# Base de datos
MYSQL_ROOT_PASSWORD=
MYSQL_DATABASE=tasa_cambio_db
DB_CONNECTION_STRING=Server=db;Port=3306;Database=tasa_cambio_db;User=root;Password=;

# SBS
URL_XML=https://www.sbs.gob.pe/app/xmltipocambio/TC_TI_Portal_xml.xml
URL_HTML=https://www.sbs.gob.pe/app/pp/SISTIP_PORTAL/Paginas/Publicacion/TipoCambioPromedio.aspx
HORA_INICIO_REGISTRO=21
HORA_FIN_REGISTRO=6
INTERVALO_BUSQUEDA_MINUTOS=30
VALIDACION_PARTES_ENTERAS=2
VALIDACION_PARTES_DECIMALES=6

# Infor SyteLine
INFOR_BASE_URL=
INFOR_SSO_URL=
INFOR_TENANT=
INFOR_CLIENT_ID=
INFOR_CLIENT_SECRET=
INFOR_SERVICE_ACCOUNT_KEY=
INFOR_SERVICE_ACCOUNT_SECRET=
INFOR_APP_ID=
INFOR_MONGOOSE=

# NuGet privado (GitHub Packages - sistecsur)
GITHUB_USER=
NUGET_TECSUR_TOKEN=

# Seq
SEQ_ADMIN_PASSWORD=
```

## Paquetes privados
El proyecto usa paquetes NuGet del feed privado `nuget.tecsur`:
- `Infor.Abstractions` v1.0.0
- `Infor.Infrastructure` v1.0.0

Requieren `GITHUB_USER` y `NUGET_TECSUR_TOKEN` (PAT con permiso `read:packages`).

## Ventana de actualización
El worker solo graba en BD/SyteLine dentro de la ventana configurada (`HoraInicioRegistro`–`HoraFinRegistro`). Fuera de ella solo valida los datos de la SBS sin persistir.

La ventana nocturna (ej. 21–6) cruza medianoche: usa lógica OR. La diurna (ej. 8–9) usa lógica AND.

## Zona horaria
El contenedor corre con `TZ=America/Lima` (UTC-5). Sin esto `DateTime.Now` devuelve hora UTC.

## Logs
- **Seq** — `http://localhost:8081` (interfaz web)
- **Archivo** — carpeta `./logs/` junto al `docker-compose.yml`, rotación diaria, 30 días de retención
