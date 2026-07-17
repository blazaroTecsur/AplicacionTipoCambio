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
| **Version** | 3.0 |

---

## OBJETIVO

El presente documento describe los requisitos funcionales, no funcionales y reglas de negocio del modulo de Obtencion Automatica del Tipo de Cambio para Tecsur, GCI y Los Andes, el cual formara parte de la plataforma interna ERP de la organizacion.

El sistema consta de un unico componente:

1. **Worker (servicio en segundo plano):** Proceso automatizado que consulta periodicamente la SBS para obtener las tasas de cambio, las registra en SyteLine directamente via IDO y las almacena en una base de datos interna como contingencia.

El registro manual del tipo de cambio cuando sea necesario (feriados, monedas sin fuente automatica, correcciones contables) se realizara directamente en SyteLine por usuarios con los permisos correspondientes.

---

## METRICAS DE EXITO

- Obtener automaticamente las tasas de cambio oficiales, eliminando el registro manual en un 100%.
- Garantizar la disponibilidad del tipo de cambio del dia antes del inicio de operaciones (7:00 AM).
- Asegurar que las tasas registradas coincidan al 100% con las publicadas por la fuente oficial seleccionada.
- Centralizar la informacion del tipo de cambio para todas las empresas del grupo (Tecsur, GCI, Los Andes) en una unica fuente de verdad.

---

## ANALISIS DE FUENTES DE DATOS

Existen cuatro fuentes oficiales/semi-oficiales para obtener el tipo de cambio en Peru. A continuacion se analiza cada una.

---

### FUENTE 1: SBS - Tipo de Cambio Promedio Ponderado

La SBS (Superintendencia de Banca, Seguros y AFP) publica diariamente el tipo de cambio promedio ponderado de compra y venta, calculado a partir de las operaciones cambiarias reportadas por las entidades del sistema financiero.

| Aspecto | Detalle |
|---------|---------|
| **Que es** | Promedio ponderado de las operaciones de compra y venta de moneda extranjera realizadas por bancos, financieras, cajas y casas de cambio |
| **Uso principal** | Conversion de operaciones en moneda extranjera para calculo del IGV. Referencia general para operaciones comerciales |
| **Ventana de calculo** | Operaciones entre las 13:30 del dia anterior y las 13:30 del dia actual |
| **Horario de publicacion** | Dia habil, generalmente antes de las 2:00 PM (hora Lima, GMT-5) |
| **Monedas disponibles** | USD (compra y venta). EUR referencial |
| **Fines de semana/feriados** | No se publica; rige el ultimo valor publicado |
| **Acceso programatico USD** | XML publico: `https://www.sbs.gob.pe/app/xmltipocambio/TC_TI_Portal_xml.xml` |
| **Acceso programatico EUR** | Scraping HTML: `https://www.sbs.gob.pe/app/pp/SISTIP_PORTAL/Paginas/Publicacion/TipoCambioPromedio.aspx` |
| **Autenticacion** | No aplica (consulta web publica) |
| **Costo** | Gratuito |

**Ventajas:**
- Es el tipo de cambio oficial para operaciones con IGV.
- Publicado por la entidad reguladora del sistema financiero.
- Dato confiable respaldado por operaciones reales del mercado.
- USD disponible via XML (sin scraping).

**Desventajas:**
- EUR requiere scraping del portal web (fragil, puede romperse con cambios en el HTML).
- No hay documentacion tecnica oficial para integracion.

---

### FUENTE 2: SBS - Tipo de Cambio Contable

La SBS tambien publica el tipo de cambio contable, que es diferente al promedio ponderado y tiene un uso especifico para estados financieros e Impuesto a la Renta.

| Aspecto | Detalle |
|---------|---------|
| **Que es** | Tipo de cambio utilizado para expresar en soles los saldos en moneda extranjera de los estados financieros y del Impuesto a la Renta |
| **Uso principal** | Cierre contable, estados financieros, Impuesto a la Renta |
| **Diferencia con el promedio ponderado** | El promedio ponderado se usa para IGV (operaciones del dia); el contable se usa para estados financieros y cierre (saldos al cierre). **No son intercambiables** |
| **Monedas disponibles** | Multiples: USD, EUR, GBP, JPY, AUD, CHF, CAD, y otras |
| **Acceso programatico** | Portal web: `https://www.sbs.gob.pe/app/pp/SISTIP_PORTAL/Paginas/Publicacion/TipoCambioContable.aspx` |
| **API REST directa** | No existe |
| **Formato** | Solo web (HTML). Series historicas descargables en Excel |
| **Autenticacion** | No aplica (consulta web publica) |
| **Costo** | Gratuito |

---

### FUENTE 3: BCRP - Banco Central de Reserva del Peru

El BCRP (Banco Central de Reserva del Peru) provee una API publica gratuita de series estadisticas que incluye tipos de cambio diarios.

| Aspecto | Detalle |
|---------|---------|
| **Que es** | Tipos de cambio del sistema bancario (SBS) e interbancario publicados por el Banco Central |
| **API REST** | `https://estadisticas.bcrp.gob.pe/estadisticas/series/api/{codigos}/{formato}/{fechaInicio}/{fechaFin}` |
| **Formato** | **JSON**, XML, CSV, XLS, HTML |
| **Autenticacion** | **No requiere token ni registro** |
| **Costo** | **Gratuito** |

---

### FUENTE 4: SUNAT - Superintendencia Nacional de Aduanas y de Administracion Tributaria

SUNAT publica el tipo de cambio oficial para efectos tributarios. Este valor corresponde a la cotizacion de cierre de la SBS del dia habil anterior.

| Aspecto | Detalle |
|---------|---------|
| **Que es** | Tipo de cambio oficial para efectos tributarios. Es la cotizacion de cierre del dia habil anterior publicada por la SBS |
| **Uso principal** | Declaraciones tributarias, operaciones de Aduanas, IGV, Impuesto a la Renta |
| **API REST directa** | **No existe API REST oficial publica** |
| **Formato** | Solo web (HTML) |
| **Costo** | Gratuito |

---

## CUADRO COMPARATIVO DE FUENTES

| Criterio | SBS Promedio Ponderado | SBS Contable | BCRP | SUNAT |
|----------|----------------------|--------------|------|-------|
| **Validez para IGV** | Si (oficial) | No | Referencial (mismo dato SBS) | Si (usa dato SBS) |
| **Validez para IR / EEFF** | No | Si (oficial) | No | Parcial |
| **API REST disponible** | Parcial (XML para USD) | No | **Si (funcionando)** | No (solo terceros) |
| **Requiere token** | No | No | **No** | No |
| **Costo** | Gratuito | Gratuito | **Gratuito** | Gratuito |
| **Monedas diarias** | USD (XML), EUR (HTML) | USD, EUR, GBP, JPY+ | **USD, EUR** | USD, EUR, GBP+ |
| **Facilidad de integracion** | Media (XML+scraping) | Baja (scraping) | **Alta (API REST/JSON)** | Baja |

---

## DECISION DE FUENTE SELECCIONADA

Se utiliza la **SBS - Tipo de Cambio Promedio Ponderado** como fuente oficial:

- **USD:** obtenido via XML publico (`TC_TI_Portal_xml.xml`) — sin scraping, dato estructurado.
- **EUR:** obtenido via scraping HTML del portal SBS Promedio Ponderado.

Esta decision fue adoptada por el area contable/tributaria por ser el tipo de cambio oficial para operaciones con IGV, alineado con las necesidades de la organizacion.

### Riesgo a considerar

El scraping HTML para EUR es fragil: si la SBS modifica la estructura de su pagina, el proceso de obtencion de EUR dejara de funcionar hasta que se actualice el codigo. Se recomienda monitorear los logs del Worker para detectar este escenario.

---

## REQUISITOS

| Campo | Detalle |
|-------|---------|
| Sistema | Plataforma interna ERP (SyteLine) |
| Aplicacion | Modulo de Tipo de Cambio |

### Historias de Usuario

---

#### HU-01 - Obtencion automatica del tipo de cambio desde la SBS

**Como** sistema automatizado, **quiero** obtener periodicamente el tipo de cambio oficial publicado por la SBS, **para** registrarlo en SyteLine y en la base de datos interna sin intervencion manual.

**Criterios de aceptacion:**

- El Worker consulta automaticamente la SBS en la ventana horaria configurada (por defecto: 21:00 - 06:00 del dia siguiente).
- Las monedas que se obtienen son configurables. Inicialmente: Dolar (USD) y Euro (EUR).
- USD se obtiene del XML publico de la SBS (`TC_TI_Portal_xml.xml`). EUR se obtiene por scraping del portal web de la SBS.
- La fecha del registro corresponde a la fecha publicada por la SBS en la respuesta, no a la fecha del servidor.
- Si el tipo de cambio del dia ya fue registrado y no ha cambiado, no se duplica ni se modifica.
- Si la fuente publica una correccion, el sistema actualiza automaticamente el valor registrado.
- Si la fuente no esta disponible temporalmente, el sistema reintenta la consulta segun el intervalo configurado (por defecto: cada 30 minutos).
- Toda operacion de obtencion queda registrada en los logs del sistema para auditoria.
- El tipo de cambio del dia debe estar disponible en SyteLine antes de las 7:00 AM del dia siguiente para el inicio de operaciones.
- Fuera de la ventana de actualizacion, el Worker solo valida que la fuente responde correctamente (no graba en BD ni en SyteLine).

**Ventana horaria de obtencion:**

| Parametro | Valor por defecto | Descripcion |
|-----------|-------------------|-------------|
| `HoraInicioRegistro` | 21 | Hora (0-23) en que inicia la ventana de actualizacion |
| `HoraFinRegistro` | 6 | Hora (0-23) en que termina la ventana de actualizacion |
| `IntervaloBusquedaMinutos` | 30 | Minutos entre cada intento de consulta |

---

#### HU-02 - Registro automatico del tipo de cambio en SyteLine via IDO

**Como** sistema automatizado, **quiero** registrar el tipo de cambio obtenido de la SBS directamente en SyteLine a traves del IDO `SLCurrates`, **para** que los modulos del ERP dispongan del tipo de cambio actualizado sin intervencion manual.

**Criterios de aceptacion:**

- Inmediatamente despues de obtener el tipo de cambio de la SBS, el Worker lo registra en SyteLine usando el IDO `SLCurrates`.
- Los campos registrados en SyteLine son:

| Campo IDO | Valor |
|-----------|-------|
| `ToCurrCode` | `PEN` (siempre, moneda destino) |
| `FromCurrCode` | `USD` o `EUR` (moneda origen) |
| `EffDate` | Fecha publicada por la SBS (formato `yyyy-MM-dd`) |
| `BuyRate` | Valor de compra (4 decimales) |
| `SellRate` | Valor de venta (4 decimales) |
| `UserCode` | Usuario del sistema configurado (`WORKER`) |

- Si ya existe un registro en SyteLine para esa combinacion de moneda y fecha, el sistema lo actualiza (`Action=2`). Si no existe, crea uno nuevo (`Action=1`).
- La autenticacion con SyteLine se realiza via OAuth2 (`grant_type=password`) contra el SSO de Infor. El token se reutiliza mientras este vigente; solo se solicita uno nuevo cuando expira.
- Si el registro en SyteLine falla por cualquier motivo, la operacion **no se cancela**: el dato queda guardado en la base de datos interna (contingencia) y el error se registra en los logs. La disponibilidad de SyteLine no bloquea el ciclo del Worker.
- El registro manual del tipo de cambio (feriados, monedas sin fuente automatica, correcciones contables) se realiza directamente en SyteLine por los usuarios con los permisos correspondientes, fuera del alcance de este sistema.

---

#### HU-03 - Registro manual del tipo de cambio en SyteLine

**Como** analista contable con permisos en SyteLine, **quiero** poder registrar o corregir manualmente el tipo de cambio directamente en el modulo `SLCurrates` de SyteLine, **para** cubrir los casos en que la fuente automatica no disponga del dato o se requiera aplicar un valor especifico aprobado por el area contable.

**Criterios de aceptacion:**

- El usuario accede directamente al IDO `SLCurrates` en SyteLine con sus credenciales y permisos propios.
- Puede crear un nuevo registro indicando: moneda origen (`FromCurrCode`), moneda destino (`ToCurrCode = PEN`), fecha vigencia (`EffDate`), tipo de cambio compra (`BuyRate`) y venta (`SellRate`).
- Si ya existe un registro para esa combinacion de moneda y fecha, puede modificar los valores de compra y venta.
- El sistema no provee una interfaz web propia para esta operacion; se usa la funcionalidad nativa de SyteLine.
- Los permisos de acceso al IDO `SLCurrates` son administrados por el equipo de SyteLine segun los roles definidos en la organizacion.

**Escenarios de uso:**

| Escenario | Descripcion |
|-----------|-------------|
| Fuente no disponible | La SBS no publico el tipo de cambio a tiempo y se necesita ingresarlo manualmente para no retrasar las operaciones del dia. |
| Moneda sin fuente automatica | Se requiere el tipo de cambio de una moneda no cubierta por el Worker (GBP, JPY, CHF, etc.). |
| Correccion contable | El area contable determina que debe aplicarse un valor diferente al publicado automaticamente. |
| Carga historica | Se necesita registrar tipos de cambio de fechas pasadas que no fueron capturados por el proceso automatico. |

---

## ARQUITECTURA TECNICA

### Diagrama de componentes

```
  +-------------+    +-------------+
  | SBS XML     |    | SBS HTML    |
  | (USD)       |    | (EUR/scraping)
  +------+------+    +------+------+
         |                  |
         +--------+---------+
                  |
                  v HTTPS
    +---------------------------+
    |       Docker Compose      |
    |                           |
    |  +---------+              |
    |  | Worker  +----> MySQL   |
    |  | (sync)  |    (contingencia)
    |  +----+----+              |
    |       |                   |
    +-------|-------------------+
            |
            | IDO REST (HTTPS)
            v
    +---------------+
    |   SyteLine    |
    | (SLCurrates)  |
    +---------------+
```

### Modelo de datos (BD interna — contingencia)

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
| Fecha | DATE | Fecha de la tasa de cambio (segun SBS) |
| ValorCompra | DECIMAL(18,6) | Tipo de cambio compra |
| ValorVenta | DECIMAL(18,6) | Tipo de cambio venta |
| FuenteOrigen | VARCHAR(50) | Origen del dato: `SBS` |
| UsuarioReg | VARCHAR(50) | Usuario/sistema que registro |
| FechaReg | DATETIME | Fecha de registro |
| UsuarioAct | VARCHAR(50) | Usuario/sistema que actualizo |
| FechaAct | DATETIME | Fecha de actualizacion |

> Restriccion unica: `(CodigoMoneda, Fecha)` — solo una tasa por moneda por dia.

---

## FUERA DEL ALCANCE

- Notificaciones o alertas automaticas cuando el tipo de cambio no se haya actualizado.
- Conversion de montos entre monedas (el sistema solo registra tasas, no realiza calculos de conversion).
- Obtencion automatica de monedas distintas a USD y EUR (GBP, JPY, CHF, CAD, etc.).
- Implementacion simultanea de multiples fuentes con fallback automatico (evaluable en version futura).
- Exposicion de una API REST propia para consulta del tipo de cambio (los modulos del ERP consultan directamente SyteLine).

---

## RIESGOS

| # | Riesgo | Impacto | Mitigacion |
|---|--------|---------|------------|
| 1 | La SBS modifica la estructura del XML de USD. | Alto — Se detiene la obtencion de USD. | Monitorear logs del Worker; ajustar el parseo del XML. |
| 2 | La SBS modifica el HTML del portal para EUR (scraping). | Alto — Se detiene la obtencion de EUR. | Monitorear logs del Worker; actualizar los selectores HTML. El USD no se ve afectado. |
| 3 | Indisponibilidad de la base de datos interna (MySQL). | Medio — No se guarda la contingencia, pero SyteLine puede recibir el dato igual. | `EnableRetryOnFailure` configurado; monitoreo de conectividad. |
| 4 | Indisponibilidad del SSO o IDO de SyteLine. | Medio — El dato no llega a SyteLine pero queda en la BD interna. | El Worker continua funcionando; el operador puede ingresar el dato manualmente en SyteLine si es urgente. |
| 5 | No contar con la disponibilidad del usuario para efectuar las pruebas funcionales. | Medio — Retraso en la validacion. | Definir cronograma de pruebas con fechas comprometidas. |

---

## REGISTRO DE CAMBIOS

| Version | Causa del cambio | Responsable del cambio | Fecha del cambio |
|---------|------------------|------------------------|------------------|
| 1.0 | Creacion del documento (solo SBS y SUNAT) | _[Nombre]_ | _[DD.MM.YYYY]_ |
| 2.0 | Ampliacion con 4 fuentes: SBS Promedio Ponderado, SBS Contable, BCRP y SUNAT. Inclusion de datos reales verificados del BCRP. Recomendacion actualizada. | _[Nombre]_ | _[DD.MM.YYYY]_ |
| 3.0 | Cambio de alcance: se elimina la API REST propia y el componente Nginx. La arquitectura queda como Worker → SyteLine (IDO directo) + BD interna (contingencia). Las historias de usuario quedan en tres: HU-01 (obtencion automatica desde SBS), HU-02 (registro automatico en SyteLine via IDO SLCurrates) y HU-03 (registro manual directamente en SyteLine por usuario autorizado, sin interfaz propia). Fuente seleccionada: SBS Promedio Ponderado (USD via XML, EUR via HTML). | _[Nombre]_ | _[DD.MM.YYYY]_ |
