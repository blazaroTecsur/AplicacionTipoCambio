# ESPECIFICACION FUNCIONAL - OBTENCION AUTOMATICA DEL TIPO DE CAMBIO

---

## HOJA DE CONTROL

| Campo | Detalle |
|-------|---------|
| **Area solicitante** | 2800 - Proyecto ERP |
| **Proyecto** | Obtener y registrar el tipo de cambio de forma automatica |
| **Propietario (Owner)** | _[Nombre del owner]_ |
| **Interesado (Stakeholder)** | Gerencia de Administracion, Finanzas y Subcontratistas |
| **Fecha de elaboracion** | _[DD.MM.YYYY]_ |
| **Version** | 1.0 |

---

## OBJETIVO

El presente documento describe los requisitos funcionales, no funcionales y reglas de negocio del modulo de Obtencion Automatica del Tipo de Cambio para Tecsur, GCI y Los Andes, el cual formara parte de la plataforma interna ERP de la organizacion.

El sistema consta de dos componentes principales:

1. **Worker (servicio en segundo plano):** Proceso automatizado que consulta periodicamente la API de la SBS (Superintendencia de Banca, Seguros y AFP) para obtener las tasas de cambio oficiales y registrarlas en la base de datos.
2. **API REST:** Servicio web que expone las tasas de cambio almacenadas para ser consumidas por otros modulos del ERP (comprobantes de pago, ordenes de compra, contabilidad, etc.).

---

## METRICAS DE EXITO

- Obtener automaticamente las tasas de cambio oficiales publicadas por la SBS, eliminando el registro manual en un 100%.
- Garantizar la disponibilidad del tipo de cambio del dia siguiente antes de las 7:00 AM para el inicio de operaciones.
- Reducir el tiempo de consulta del tipo de cambio a menos de 1 segundo por peticion.
- Asegurar que las tasas registradas coincidan al 100% con las publicadas por la fuente oficial (SBS o SUNAT).
- Centralizar la informacion del tipo de cambio para todas las empresas del grupo (Tecsur, GCI, Los Andes) en una unica fuente de verdad.

---

## ANALISIS DE FUENTES DE DATOS: SBS vs SUNAT

Antes de definir los requisitos, es necesario evaluar las dos fuentes oficiales disponibles para obtener el tipo de cambio en Peru.

### Opcion A: SBS (Superintendencia de Banca, Seguros y AFP) - Implementacion actual

| Aspecto | Detalle |
|---------|---------|
| **API** | `https://api.sbs.gob.pe/tipo-cambio/v1/{ddMMyyyy}/{codigoMoneda}` |
| **Autenticacion** | Bearer Token (se solicita en el portal de la SBS) |
| **Costo** | Gratuito |
| **Monedas disponibles** | USD, EUR, GBP, JPY, CHF, CAD, BRL, y otras |
| **Horario de publicacion** | Las tasas del dia siguiente (D+1) se publican entre las 9:00 PM y 11:59 PM del dia anterior |
| **Formato de respuesta** | JSON: `{ "codigo_moneda": "USD", "descripcion_moneda": "...", "valor_compra": "3.700", "valor_venta": "3.750" }` |
| **Disponibilidad** | Alta, con interrupciones ocasionales por mantenimiento |
| **Limitaciones** | Token sujeto a politicas de uso de la SBS; sin SLA formal para el servicio |

**Ventajas:**
- API REST moderna y facil de integrar.
- Respuesta en JSON, parseo directo.
- Multiples monedas en un solo servicio.
- Sin costo.

**Desventajas:**
- El token puede requerir renovacion periodica.
- No existe un SLA garantizado (servicio puede caer sin aviso).
- Documentacion oficial limitada.

### Opcion B: SUNAT (Superintendencia Nacional de Aduanas y de Administracion Tributaria)

| Aspecto | Detalle |
|---------|---------|
| **API** | Web service SOAP / portal web (no hay API REST oficial publica) |
| **Autenticacion** | Clave SOL (para consultas tributarias) |
| **Costo** | Gratuito (consulta tributaria); servicios avanzados pueden tener costo |
| **Monedas disponibles** | Principalmente USD (tipo de cambio tributario) |
| **Horario de publicacion** | Las tasas se publican generalmente despues de las 5:00 PM |
| **Formato de respuesta** | Variado; web scraping necesario en algunos casos |
| **Disponibilidad** | Sujeta a mantenimientos programados y no programados |

**Ventajas:**
- Tipo de cambio con validez tributaria directa (el que exige SUNAT para declaraciones).
- Fuente oficial para efectos contables y fiscales.

**Desventajas:**
- No ofrece una API REST publica; integracion mas compleja (SOAP, scraping).
- Solo publica USD de manera confiable; otras monedas no siempre disponibles.
- Mayor complejidad tecnica para automatizar.
- Posibles cambios sin aviso en la estructura del servicio web.

### Recomendacion

| Criterio | SBS | SUNAT |
|----------|-----|-------|
| Facilidad de integracion | Alta (REST/JSON) | Baja (SOAP/scraping) |
| Costo | Gratuito | Gratuito |
| Variedad de monedas | Alta (8+) | Baja (USD principal) |
| Confiabilidad | Alta | Media |
| Validez tributaria | Referencial | Oficial |
| Mantenimiento tecnico | Bajo | Alto |

**Conclusion:** Se recomienda utilizar la **SBS como fuente principal** por su facilidad de integracion, variedad de monedas y estabilidad. Si se requiere validez tributaria estricta para el USD, se puede incorporar SUNAT como fuente complementaria en una fase posterior.

> **Nota para el usuario:** Evaluar con el area contable si el tipo de cambio de la SBS es aceptable para efectos tributarios, o si se requiere obligatoriamente el de SUNAT. En caso de requerir ambos, el sistema esta disenado para soportar multiples fuentes de origen (`FuenteOrigen` en la tabla `tttasacambio`).

---

## REQUISITOS

- **Sistema:** Plataforma interna ERP
- **Aplicacion:** Modulo de Tipo de Cambio (Worker + API REST)
- **Base de datos:** MySQL 8.3 (`erp_tecsur_pinterna`)
- **Tecnologia:** .NET 8, ASP.NET Core, Entity Framework Core

### Historias de Usuario

---

#### HU-01 - Sincronizacion automatica del tipo de cambio desde la SBS

**Como** administrador del sistema, **quiero** que un proceso automatizado consulte la API de la SBS cada cierto intervalo durante la ventana de publicacion nocturna, **para** que las tasas de cambio del dia siguiente esten disponibles antes del inicio de operaciones.

**Criterios de aceptacion:**

- El Worker consulta la API de la SBS cada 30 minutos (configurable) dentro de la ventana de actualizacion (9:00 PM a 6:00 AM).
- Las monedas a sincronizar son configurables: USD y EUR por defecto.
- Si la tasa de una moneda para una fecha ya existe en la base de datos y los valores son iguales, no se modifica el registro.
- Si la tasa existe pero los valores difieren (correccion de la SBS), se actualiza el registro existente.
- Si la tasa no existe, se crea un nuevo registro.
- Toda operacion de sincronizacion queda registrada en la auditoria (`SINCRONIZAR_SBS` o `ACTUALIZAR_SBS`).
- Si la API de la SBS no responde, el sistema reintenta automaticamente hasta 3 veces con espera exponencial (2s, 4s, 8s) y circuit breaker (se detiene despues de 5 fallos consecutivos por 30 segundos).
- Fuera de la ventana de actualizacion, el Worker solo consulta y registra en log (no persiste en base de datos).

**Parametros configurables (via variables de entorno):**

| Variable | Descripcion | Valor por defecto |
|----------|-------------|-------------------|
| `Sbs__Url` | URL base de la API SBS | `https://api.sbs.gob.pe/tipo-cambio/v1/` |
| `Sbs__Token` | Token de autenticacion Bearer | _(requerido)_ |
| `Sbs__HoraInicioRegistro` | Hora inicio de la ventana de actualizacion | 21 (9 PM) |
| `Sbs__HoraFinRegistro` | Hora fin de la ventana de actualizacion | 6 (6 AM) |
| `Sbs__IntervaloBusquedaMinutos` | Intervalo entre consultas | 30 |

---

#### HU-02 - Consulta de monedas disponibles

**Como** usuario del ERP, **quiero** consultar la lista de monedas registradas en el sistema, **para** conocer cuales monedas tienen tipo de cambio disponible.

**Criterios de aceptacion:**

- El endpoint `GET /api/v1/moneda` devuelve la lista completa de monedas con: codigo, descripcion, simbolo, codigo SUNAT y descripcion ISO 4217.
- No requiere parametros de entrada.
- Requiere autenticacion via header `X-Api-Key`.
- Monedas iniciales del sistema: USD, EUR, PEN, GBP, JPY, CHF, CAD, BRL.

**Ejemplo de respuesta:**

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "codigo": "USD",
      "descripcion": "Dolar Americano",
      "simbolo": "$",
      "codigoSunat": "02",
      "descripcionIso4217": "USD"
    }
  ],
  "message": null,
  "errors": []
}
```

---

#### HU-03 - Consulta de tasas de cambio por moneda y periodo

**Como** usuario del ERP (contabilidad, tesoreria), **quiero** consultar las tasas de cambio de una moneda especifica filtradas por anio y/o mes, **para** obtener el historico de tasas registradas.

**Criterios de aceptacion:**

- El endpoint `GET /api/v1/tipocambio/{codigoMoneda}` devuelve todas las tasas registradas para la moneda indicada.
- Acepta filtros opcionales: `anio` y `mes` (query parameters).
- Cada registro incluye: fecha, valor de compra, valor de venta, tasa promedio (calculada), fuente de origen y detalle de la moneda.
- Requiere autenticacion via header `X-Api-Key`.

---

#### HU-04 - Consulta de tasa de cambio por fecha exacta

**Como** modulo del ERP (registro de comprobantes, ordenes de compra), **quiero** obtener la tasa de cambio de una moneda para una fecha especifica, **para** utilizar el tipo de cambio correcto en las operaciones contables del dia.

**Criterios de aceptacion:**

- El endpoint `GET /api/v1/tipocambio/{codigoMoneda}/{fecha}` devuelve la tasa para la fecha exacta indicada (formato `yyyy-MM-dd`).
- Si no existe tasa para esa fecha y moneda, devuelve HTTP 404 con mensaje descriptivo.
- Para la moneda nacional (codigo `NSOLES`), siempre devuelve valores fijos de compra = 1, venta = 1, promedio = 1 (sin consultar la base de datos).
- Requiere autenticacion via header `X-Api-Key`.

---

#### HU-05 - Consulta de la ultima tasa de cambio disponible

**Como** modulo del ERP, **quiero** obtener la tasa de cambio mas reciente disponible para una moneda hasta una fecha dada, **para** utilizar la ultima tasa vigente cuando no exista tasa para el dia exacto (feriados, fines de semana).

**Criterios de aceptacion:**

- El endpoint `GET /api/v1/tipocambio/{codigoMoneda}/{fecha}/ultima` devuelve la tasa con fecha menor o igual a la indicada.
- Si no existe ninguna tasa anterior o igual a la fecha, devuelve HTTP 404.
- Requiere autenticacion via header `X-Api-Key`.

---

#### HU-06 - Seguridad de la API

**Como** administrador del sistema, **quiero** que todos los endpoints de la API esten protegidos con una API Key, **para** evitar accesos no autorizados.

**Criterios de aceptacion:**

- Toda peticion (excepto `/health` y `/swagger`) debe incluir el header `X-Api-Key` con el valor configurado en el servidor.
- Si no se envia el header, se responde HTTP 401 con mensaje "API Key requerida."
- Si el valor es incorrecto, se responde HTTP 401 con mensaje "API Key invalida."
- La API Key se genera con un algoritmo criptograficamente seguro (CSPRNG) de al menos 256 bits.
- La API Key se almacena como variable de entorno, nunca en codigo fuente.

---

#### HU-07 - Monitoreo y salud del servicio

**Como** equipo de infraestructura, **quiero** un endpoint de health check, **para** monitorear el estado de disponibilidad de la API.

**Criterios de aceptacion:**

- El endpoint `GET /health` responde HTTP 200 con `Healthy` si el servicio esta operativo.
- No requiere autenticacion.
- Puede ser utilizado por balanceadores de carga y herramientas de monitoreo.

---

## ARQUITECTURA TECNICA

### Diagrama de componentes

```
                    +-----------+
                    |   SBS API |
                    | (externa) |
                    +-----+-----+
                          |
                          | HTTPS + Bearer Token
                          |
+-------------------------+-------------------------+
|                    Docker Compose                  |
|                                                    |
|  +----------+    +----------+    +-----------+     |
|  |  Worker   |--->|   MySQL  |<---|    API    |     |
|  | (cron)   |    |   (db)   |    | (REST)    |     |
|  +----------+    +----------+    +-----+-----+     |
|                                        |           |
|                                  +-----+-----+    |
|                                  |   Nginx    |    |
|                                  | (reverse   |    |
|                                  |  proxy)    |    |
|                                  +-----+-----+    |
|                                        |           |
+----------------------------------------+-----------+
                                         |
                                    Puerto 8089
                                         |
                                  +------+------+
                                  |   Usuarios  |
                                  |   ERP /     |
                                  |   Postman   |
                                  +-------------+
```

### Modelo de datos

**Tabla `ttmoneda`**

| Columna | Tipo | Descripcion |
|---------|------|-------------|
| IdMoneda | BIGINT PK AUTO_INCREMENT | Identificador unico |
| Codigo | VARCHAR(10) UNIQUE | Codigo de moneda (USD, EUR, etc.) |
| Descripcion | VARCHAR(100) | Nombre de la moneda |
| Simbolo | VARCHAR(10) | Simbolo ($, EUR, S/, etc.) |
| CodigoSunat | VARCHAR(10) | Codigo SUNAT |
| DescripcionIso4217 | VARCHAR(50) | Descripcion ISO 4217 |
| UsuarioReg | VARCHAR(50) | Usuario que registro |
| FechaReg | DATETIME | Fecha de registro |
| UsuarioAct | VARCHAR(50) | Usuario que actualizo |
| FechaAct | DATETIME | Fecha de actualizacion |

**Tabla `tttasacambio`**

| Columna | Tipo | Descripcion |
|---------|------|-------------|
| IdTasaCambio | BIGINT PK AUTO_INCREMENT | Identificador unico |
| CodigoMoneda | VARCHAR(10) FK | Codigo de moneda (referencia a `ttmoneda.Codigo`) |
| Fecha | DATE | Fecha de la tasa de cambio |
| ValorCompra | DECIMAL(18,6) | Tipo de cambio compra |
| ValorVenta | DECIMAL(18,6) | Tipo de cambio venta |
| FechaSbs | DATE | Fecha de publicacion de la SBS |
| FuenteOrigen | VARCHAR(50) | Origen del dato (SBS, SUNAT, MANUAL) |
| UsuarioReg | VARCHAR(50) | Usuario/sistema que registro |
| FechaReg | DATETIME | Fecha de registro |
| UsuarioAct | VARCHAR(50) | Usuario/sistema que actualizo |
| FechaAct | DATETIME | Fecha de actualizacion |

> Restriccion unica: `(CodigoMoneda, Fecha)` - solo una tasa por moneda por dia.

---

## INTERACCION CON EL USUARIO Y DISENO

### Endpoints de la API REST

| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | `/health` | Health check (sin autenticacion) |
| GET | `/api/v1/moneda` | Listar monedas disponibles |
| GET | `/api/v1/tipocambio/{moneda}?anio=&mes=` | Listar tasas por moneda y periodo |
| GET | `/api/v1/tipocambio/{moneda}/{fecha}` | Obtener tasa por fecha exacta |
| GET | `/api/v1/tipocambio/{moneda}/{fecha}/ultima` | Obtener ultima tasa vigente |

### Ejemplo de integracion desde otro modulo del ERP

Para obtener el tipo de cambio del USD del dia actual:

```
GET https://api.tecsur.com.pe:8089/api/v1/tipocambio/usd/2026-06-15
Header: X-Api-Key: <clave configurada>
```

Si no existe tasa para ese dia (feriado/fin de semana), usar el endpoint `/ultima`:

```
GET https://api.tecsur.com.pe:8089/api/v1/tipocambio/usd/2026-06-15/ultima
Header: X-Api-Key: <clave configurada>
```

---

## FUERA DEL ALCANCE

- Integracion directa con SUNAT como fuente de tipo de cambio (evaluable en version 2.0).
- Registro manual de tasas de cambio via interfaz web (actualmente solo via base de datos o API de la SBS).
- Notificaciones o alertas automaticas cuando el tipo de cambio no se haya actualizado.
- Conversion de montos entre monedas (la API solo provee las tasas, no realiza calculos de conversion).
- Gestion de usuarios y roles para el acceso a la API (actualmente se usa una unica API Key compartida).

---

## RIESGOS

| # | Riesgo | Impacto | Mitigacion |
|---|--------|---------|------------|
| 1 | La SBS modifica la estructura de su API o descontinua el servicio sin previo aviso. | Alto - Se detiene la actualizacion automatica. | Monitoreo de logs del Worker; implementar fuente alternativa (SUNAT) como respaldo. |
| 2 | El token de la SBS expira o es revocado. | Alto - El Worker no puede autenticarse. | Configurar alertas cuando el Worker registre errores consecutivos de autenticacion. |
| 3 | Indisponibilidad de la base de datos externa (`10.160.9.18`). | Alto - Ni la API ni el Worker funcionan. | `EnableRetryOnFailure` configurado; monitoreo de conectividad. |
| 4 | Cambios en los requisitos contables o tributarios que exijan usar obligatoriamente el tipo de cambio de SUNAT en lugar de SBS. | Medio - Requiere desarrollo adicional. | El campo `FuenteOrigen` ya permite diferenciar la fuente; la arquitectura soporta multiples origenes. |
| 5 | No contar con la disponibilidad del usuario para efectuar las pruebas funcionales. | Medio - Retraso en la validacion. | Definir cronograma de pruebas con fechas comprometidas. |

---

## COSTOS DE LOS SERVICIOS

| Concepto | SBS | SUNAT |
|----------|-----|-------|
| Uso de la API | Gratuito | Gratuito (consulta basica) |
| Token/Credencial | Gratuito (solicitar en portal SBS) | Clave SOL (ya disponible en la empresa) |
| Infraestructura (Docker/servidor) | Incluido en la infraestructura existente | Incluido en la infraestructura existente |
| Desarrollo de integracion | Ya implementado (version actual) | Estimado: 40-60 horas de desarrollo |
| Mantenimiento anual | Bajo (~4 horas/anio) | Medio-Alto (~20 horas/anio por cambios en el servicio web) |

> **Nota:** No se incurre en costos adicionales por el uso de las APIs de la SBS ni de SUNAT para consultas de tipo de cambio. El costo principal es el de desarrollo y mantenimiento de la integracion.

---

## REGISTRO DE CAMBIOS

| Version | Causa del cambio | Responsable del cambio | Fecha del cambio |
|---------|------------------|------------------------|------------------|
| 1.0 | Creacion del documento | _[Nombre]_ | _[DD.MM.YYYY]_ |
| | | | |
