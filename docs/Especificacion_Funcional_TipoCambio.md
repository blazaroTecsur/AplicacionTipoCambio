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
| **Version** | 2.0 |

---

## OBJETIVO

El presente documento describe los requisitos funcionales, no funcionales y reglas de negocio del modulo de Obtencion Automatica del Tipo de Cambio para Tecsur, GCI y Los Andes, el cual formara parte de la plataforma interna ERP de la organizacion.

El sistema consta de dos componentes principales:

1. **Worker (servicio en segundo plano):** Proceso automatizado que consulta periodicamente fuentes oficiales (SBS, BCRP o SUNAT) para obtener las tasas de cambio y registrarlas en la base de datos.
2. **API REST:** Servicio web que expone las tasas de cambio almacenadas para ser consumidas por otros modulos del ERP (comprobantes de pago, ordenes de compra, contabilidad, etc.).

---

## METRICAS DE EXITO

- Obtener automaticamente las tasas de cambio oficiales, eliminando el registro manual en un 100%.
- Garantizar la disponibilidad del tipo de cambio del dia antes del inicio de operaciones (7:00 AM).
- Reducir el tiempo de consulta del tipo de cambio a menos de 1 segundo por peticion.
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
| **Acceso programatico** | Portal web: `https://www.sbs.gob.pe/app/pp/SISTIP_PORTAL/Paginas/Publicacion/TipoCambioPromedio.aspx` |
| **API REST directa** | `https://api.sbs.gob.pe/tipo-cambio/v1/` — **NO ACTIVA actualmente** (servidor responde pagina por defecto, sin API desplegada) |
| **Formato** | Solo web (HTML). No hay API REST ni XML publico confirmado |
| **Autenticacion** | No aplica (consulta web publica) |
| **Costo** | Gratuito |

**Ventajas:**
- Es el tipo de cambio oficial para operaciones con IGV.
- Publicado por la entidad reguladora del sistema financiero.
- Dato confiable respaldado por operaciones reales del mercado.

**Desventajas:**
- La API REST (`api.sbs.gob.pe`) no esta activa; no hay un servicio web oficial para consumo automatizado.
- La automatizacion requiere scraping del portal web (fragil, puede romperse con cambios en el HTML).
- Solo publica USD de forma confiable; EUR es referencial.
- No hay documentacion tecnica oficial para integracion.

---

### FUENTE 2: SBS - Tipo de Cambio Contable

La SBS tambien publica el tipo de cambio contable, que es diferente al promedio ponderado y tiene un uso especifico para estados financieros e Impuesto a la Renta.

| Aspecto | Detalle |
|---------|---------|
| **Que es** | Tipo de cambio utilizado para expresar en soles los saldos en moneda extranjera de los estados financieros y del Impuesto a la Renta |
| **Uso principal** | Cierre contable, estados financieros, Impuesto a la Renta |
| **Diferencia con el promedio ponderado** | El promedio ponderado se usa para IGV (operaciones del dia); el contable se usa para estados financieros y cierre (saldos al cierre). **No son intercambiables** |
| **Monedas disponibles** | Multiples: USD, EUR, GBP, JPY, AUD, CHF, CAD, y otras (monedas de paises con los que Peru tiene ~98% de comercio exterior) |
| **Horario de publicacion** | Dia habil (horario exacto no especificado por la SBS) |
| **Frecuencia de actualizacion** | La lista de monedas se actualiza semestralmente (corte al 30 de junio y 31 de diciembre) segun el Informe N.5 de las empresas supervisadas |
| **Acceso programatico** | Portal web: `https://www.sbs.gob.pe/app/pp/SISTIP_PORTAL/Paginas/Publicacion/TipoCambioContable.aspx` |
| **API REST directa** | No existe |
| **Formato** | Solo web (HTML). Series historicas descargables en Excel |
| **Autenticacion** | No aplica (consulta web publica) |
| **Costo** | Gratuito |

**Ventajas:**
- Tipo de cambio oficial para estados financieros e Impuesto a la Renta.
- Soporta **multiples monedas** (USD, EUR, GBP, JPY, CHF, CAD, AUD, y mas).
- Fuente regulatoria con validez legal para efectos tributarios.
- Abarca monedas de paises que representan ~98% del comercio exterior peruano.

**Desventajas:**
- No hay API REST ni servicio web; solo consulta via portal web.
- La automatizacion requiere scraping del portal (fragil y compleja).
- Solo se publica una moneda si tiene al menos 30% de dias habiles con operaciones en el semestre.
- Puede tener menor granularidad que el promedio ponderado para USD.

---

### FUENTE 3: BCRP - Banco Central de Reserva del Peru

El BCRP (Banco Central de Reserva del Peru) provee una API publica gratuita de series estadisticas que incluye tipos de cambio diarios.

| Aspecto | Detalle |
|---------|---------|
| **Que es** | Tipos de cambio del sistema bancario (SBS) e interbancario publicados por el Banco Central |
| **Uso principal** | Referencial; los datos de TC SBS provienen de la misma fuente oficial |
| **Horario de publicacion** | TC interbancario se publica a las **1:30 PM** (13:30 hora Lima). TC SBS en la tarde del mismo dia |
| **Monedas disponibles (diarias)** | **USD** (8 series: interbancario, SBS, 11AM, cierre — compra y venta cada uno) y **EUR** (2 series: compra y venta) |
| **Monedas disponibles (mensuales)** | USD, EUR, GBP, JPY, CHF, CAD, BRL, y mas (34 series) |
| **API REST** | `https://estadisticas.bcrp.gob.pe/estadisticas/series/api/{codigos}/{formato}/{fechaInicio}/{fechaFin}` |
| **Formato** | **JSON**, XML, CSV, XLS, HTML |
| **Autenticacion** | **No requiere token ni registro** |
| **Costo** | **Gratuito** |
| **Documentacion** | `https://estadisticas.bcrp.gob.pe/estadisticas/series/ayuda/api` |

**Series diarias confirmadas y probadas:**

| Moneda | Serie Compra | Serie Venta | Verificado |
|--------|-------------|-------------|------------|
| USD (TC SBS) | `PD04639PD` | `PD04640PD` | Si - probado y funcionando |
| USD (TC Interbancario) | `PD04637PD` | `PD04638PD` | Si |
| EUR | `PD04647PD` | `PD04648PD` | Si - probado y funcionando |

**Ejemplo de llamada real (probada y verificada):**

```
GET https://estadisticas.bcrp.gob.pe/estadisticas/series/api/PD04639PD-PD04640PD/json/2026-6-15/2026-6-22
```

**Respuesta real:**
```json
{
  "config": {
    "title": "Tipo de cambio",
    "series": [
      { "name": "TC Sistema bancario SBS (S/ por US$) - Venta", "dec": "3" },
      { "name": "TC Sistema bancario SBS (S/ por US$) - Compra", "dec": "3" }
    ]
  },
  "periods": [
    { "name": "15.Jun.26", "values": ["3.387", "3.375"] },
    { "name": "16.Jun.26", "values": ["3.382", "3.376"] },
    { "name": "17.Jun.26", "values": ["3.384", "3.378"] },
    { "name": "18.Jun.26", "values": ["3.388", "3.381"] },
    { "name": "19.Jun.26", "values": ["n.d.", "n.d."] }
  ]
}
```

> Nota: `"n.d."` = no disponible (fines de semana y feriados).

**Ventajas:**
- **API REST funcionando** (verificada con datos reales de junio 2026).
- **Gratuita, sin token, sin registro**.
- Respuesta en **JSON** nativo, facil de parsear.
- Los datos de TC SBS provienen de la fuente oficial.
- Documentacion publica disponible.
- Soporta multiples series en una sola llamada (hasta 10).
- Estable — el BCRP es el Banco Central, con alta disponibilidad.

**Desventajas:**
- Series **diarias** solo para USD y EUR (no GBP, JPY, CHF, etc.).
- Las demas monedas (GBP, JPY, CHF, CAD, BRL) solo tienen series **mensuales**.
- El formato de fecha en la respuesta es `dd.Mmm.yy` (requiere parseo).
- Valores vienen como **string**, no como numero (requiere conversion).
- Los fines de semana/feriados devuelven `"n.d."` en lugar de omitir el registro.

---

### FUENTE 4: SUNAT - Superintendencia Nacional de Aduanas y de Administracion Tributaria

SUNAT publica el tipo de cambio oficial para efectos tributarios. Este valor corresponde a la **cotizacion de cierre de la SBS del dia habil anterior**.

| Aspecto | Detalle |
|---------|---------|
| **Que es** | Tipo de cambio oficial para efectos tributarios. Es la cotizacion de cierre del dia habil anterior publicada por la SBS |
| **Uso principal** | Declaraciones tributarias, operaciones de Aduanas, IGV, Impuesto a la Renta |
| **Relacion con la SBS** | SUNAT **toma** el dato de la SBS (cierre del dia anterior) y lo publica como "tipo de cambio SUNAT". No calcula un valor propio |
| **Horario de publicacion** | Cada dia habil bancario; rige para las operaciones tributarias de ese dia |
| **Monedas disponibles** | Multiples (USD, EUR, GBP, JPY, CHF, CAD, AUD, y otras — segun tabla de Aduanas) |
| **Portal de consulta** | `https://e-consulta.sunat.gob.pe/cl-at-ittipcam/tcS01Alias` |
| **Portal Aduanas** | `https://ww3.sunat.gob.pe/cl-ad-ittipocambioconsulta/TipoCambioS01Alias?accion=consultarTipoCambio` |
| **API REST directa** | **No existe API REST oficial publica** |
| **Formato** | Solo web (HTML). Formulario de consulta por rango de fechas |
| **Autenticacion** | No aplica (consulta web publica) |
| **Costo** | Gratuito |

**APIs de terceros que exponen datos SUNAT:**

| Servicio | Endpoint | Autenticacion | Costo |
|----------|----------|---------------|-------|
| apis.net.pe / DeColecta | `https://api.decolecta.com/v1/tipo-cambio/sunat` | Token opcional | Gratuito (con limites) |
| PeruAPI | `https://peruapi.com/` | API Key | Planes gratuito y de pago |

**Ventajas:**
- Es el tipo de cambio con **validez tributaria directa** (el que exige SUNAT para declaraciones).
- Soporta **multiples monedas** (incluye monedas de paises con comercio exterior significativo).
- Tiene respaldo legal explicito para efectos de IGV, Renta y Aduanas.
- Si la empresa ya tiene Clave SOL, puede consultar directamente.

**Desventajas:**
- **No tiene API REST oficial**; la consulta es solo via portal web.
- La automatizacion requiere scraping del portal o uso de APIs de terceros (con sus propios costos/limites).
- El dato es derivado de la SBS (cierre del dia anterior), no es un calculo independiente.
- Las APIs de terceros introducen una dependencia adicional y pueden tener costos.
- El portal puede cambiar sin aviso (afecta scraping).

---

## CUADRO COMPARATIVO DE FUENTES

| Criterio | SBS Promedio Ponderado | SBS Contable | BCRP | SUNAT |
|----------|----------------------|--------------|------|-------|
| **Validez para IGV** | Si (oficial) | No | Referencial (mismo dato SBS) | Si (usa dato SBS) |
| **Validez para IR / EEFF** | No | Si (oficial) | No | Parcial |
| **API REST disponible** | No (inactiva) | No | **Si (funcionando)** | No (solo terceros) |
| **Requiere token** | - | - | **No** | - |
| **Costo** | Gratuito | Gratuito | **Gratuito** | Gratuito / terceros con costo |
| **Formato JSON** | No | No | **Si** | No (terceros si) |
| **Monedas diarias** | USD | USD, EUR, GBP, JPY, CHF, CAD, AUD+ | **USD, EUR** | USD, EUR, GBP, JPY, CHF, CAD+ |
| **Facilidad de integracion** | Baja (scraping) | Baja (scraping) | **Alta (API REST/JSON)** | Baja (scraping) / Media (terceros) |
| **Estabilidad del servicio** | Desconocida (API inactiva) | Media (portal web) | **Alta (Banco Central)** | Media (portal web) |
| **Horario de publicacion** | ~2:00 PM mismo dia | Dia habil | **~1:30 PM mismo dia** | Dia habil |
| **Mantenimiento tecnico** | Alto (scraping) | Alto (scraping) | **Bajo (API estable)** | Alto (scraping) / Medio (terceros) |
| **Documentacion tecnica** | Ninguna | Ninguna | **Publica y completa** | Ninguna (terceros: parcial) |

---

## RECOMENDACION

### Opcion recomendada: BCRP como fuente principal

Se recomienda utilizar la **API del BCRP** como fuente principal por las siguientes razones:

1. **Es la unica fuente con API REST funcional, gratuita y sin autenticacion.**
2. Los datos de "TC Sistema bancario SBS" que publica el BCRP **provienen de la misma fuente oficial** (SBS), por lo que el valor es equivalente.
3. La API ha sido **probada y verificada** con datos reales (junio 2026).
4. Soporta respuestas en **JSON**, facilitando la integracion.
5. El BCRP es el Banco Central del Peru — alta estabilidad y disponibilidad.

### Limitaciones a considerar

- Solo hay datos **diarios** para USD y EUR. Para GBP, JPY, CHF, CAD, BRL se requeriria:
  - Usar las series **mensuales** del BCRP (menos granularidad), o
  - Implementar scraping del portal SBS Contable (mayor esfuerzo de desarrollo), o
  - Usar una API de terceros como DeColecta o PeruAPI (costo/dependencia adicional).

### Accion requerida del area contable/tributaria

> **IMPORTANTE:** Antes de seleccionar la fuente definitiva, el area contable/tributaria debe confirmar:
>
> 1. ¿El tipo de cambio SBS (sistema bancario) publicado por el BCRP es aceptable para sus operaciones contables y tributarias?
> 2. ¿Se requiere especificamente el "tipo de cambio contable" de la SBS (para estados financieros e IR)?
> 3. ¿Se requiere especificamente el "tipo de cambio SUNAT" para declaraciones?
> 4. ¿Que monedas son necesarias ademas de USD y EUR?
>
> La respuesta a estas preguntas determinara si se puede usar solo el BCRP o si es necesario implementar fuentes adicionales.

---

## REQUISITOS

| Campo | Detalle |
|-------|---------|
| Sistema | Plataforma interna ERP |
| Aplicacion | Modulo de Tipo de Cambio |

### Historias de Usuario

---

#### HU-01 - Obtencion automatica del tipo de cambio

**Como** analista contable, **quiero** que el sistema obtenga automaticamente el tipo de cambio oficial del dia desde una fuente confiable (BCRP, SBS o SUNAT), **para** no tener que buscarlo ni registrarlo manualmente cada dia.

**Criterios de aceptacion:**

- El sistema consulta automaticamente la fuente oficial en el horario en que se publican las tasas (por la tarde del dia habil).
- Las monedas que se obtienen son configurables. Inicialmente: Dolar (USD) y Euro (EUR).
- Si el tipo de cambio del dia ya fue registrado y no ha cambiado, no se duplica ni se modifica.
- Si la fuente publica una correccion, el sistema actualiza automaticamente el valor registrado.
- Si la fuente no esta disponible temporalmente, el sistema reintenta la consulta de forma automatica.
- Cada registro queda identificado con la fuente de origen (BCRP, SBS, SUNAT o MANUAL).
- Toda operacion de obtencion queda registrada para auditoria.
- El tipo de cambio del dia debe estar disponible antes de las 7:00 AM del dia siguiente para el inicio de operaciones.

**Ventana horaria de obtencion segun fuente:**

| Fuente | Horario de consulta sugerido | Motivo |
|--------|------------------------------|--------|
| BCRP | 2:00 PM - 6:00 PM | El BCRP publica despues de la 1:30 PM |
| SBS Promedio Ponderado | 2:30 PM - 6:00 PM | La SBS publica antes de las 2:00 PM |
| SBS Contable | 3:00 PM - 8:00 PM | Horario no especificado oficialmente |
| SUNAT | 3:00 PM - 8:00 PM | Se publica despues que la SBS |

---

#### HU-02 - Consulta de monedas disponibles

**Como** usuario del ERP, **quiero** ver la lista de monedas registradas en el sistema, **para** saber para cuales monedas puedo consultar el tipo de cambio.

**Criterios de aceptacion:**

- El sistema muestra la lista completa de monedas con: codigo, nombre, simbolo y codigo SUNAT.
- Monedas iniciales: Dolar Americano (USD), Euro (EUR), Sol Peruano (PEN), Libra Esterlina (GBP), Yen Japones (JPY), Franco Suizo (CHF), Dolar Canadiense (CAD) y Real Brasileno (BRL).
- El acceso a la consulta requiere autenticacion.

---

#### HU-03 - Consulta de tasas de cambio por moneda y periodo

**Como** analista contable, **quiero** consultar las tasas de cambio de una moneda especifica filtradas por anio y/o mes, **para** revisar el historico de tipos de cambio registrados en un periodo determinado.

**Criterios de aceptacion:**

- El usuario indica la moneda a consultar (por ejemplo, USD).
- Opcionalmente, puede filtrar por anio y/o mes para acotar el periodo.
- Por cada dia se muestra: fecha, valor de compra, valor de venta, tasa promedio, fuente de origen y detalle de la moneda.
- El acceso a la consulta requiere autenticacion.

---

#### HU-04 - Consulta de tasa de cambio por fecha exacta

**Como** analista contable o modulo del ERP (comprobantes de pago, ordenes de compra), **quiero** obtener el tipo de cambio de una moneda para una fecha especifica, **para** aplicar el tipo de cambio correcto en las operaciones contables de ese dia.

**Criterios de aceptacion:**

- El usuario indica la moneda y la fecha a consultar (por ejemplo, USD del 22/06/2026).
- El sistema devuelve: valor de compra, valor de venta, tasa promedio y fuente de origen.
- Si no existe tipo de cambio para esa fecha (feriado, fin de semana o aun no publicado), el sistema informa que no se encontro el dato.
- Para la moneda nacional (Soles), siempre devuelve compra = 1, venta = 1 y promedio = 1 (por definicion, sin necesidad de consulta externa).
- El acceso a la consulta requiere autenticacion.

---

#### HU-05 - Consulta de la ultima tasa de cambio disponible

**Como** analista contable o modulo del ERP, **quiero** obtener el tipo de cambio mas reciente disponible para una moneda hasta una fecha dada, **para** utilizar el ultimo tipo de cambio vigente cuando no exista dato para el dia exacto (feriados, fines de semana).

**Criterios de aceptacion:**

- El usuario indica la moneda y una fecha limite.
- El sistema busca la tasa de cambio mas reciente registrada con fecha menor o igual a la indicada.
- Si no existe ningun registro anterior, el sistema informa que no se encontro el dato.
- El acceso a la consulta requiere autenticacion.

**Ejemplo:** Si consulto USD al 22/06/2026 (domingo) y no hay dato para ese dia, el sistema devuelve la tasa del viernes 20/06/2026 (ultimo dia habil con publicacion).

---

#### HU-06 - Seguridad del acceso al servicio

**Como** administrador del sistema, **quiero** que las consultas de tipo de cambio esten protegidas con una clave de acceso, **para** evitar que usuarios o sistemas no autorizados consuman la informacion.

**Criterios de aceptacion:**

- Toda consulta al servicio debe incluir una clave de acceso previamente asignada.
- Si no se proporciona la clave, el sistema rechaza la consulta con el mensaje "API Key requerida."
- Si la clave es incorrecta, el sistema rechaza la consulta con el mensaje "API Key invalida."
- La clave de acceso se genera de forma segura y se almacena en la configuracion del servidor (no en el codigo fuente).

---

#### HU-07 - Monitoreo y disponibilidad del servicio

**Como** equipo de infraestructura, **quiero** poder verificar si el servicio de tipo de cambio esta funcionando correctamente, **para** detectar rapidamente si hay algun problema de disponibilidad.

**Criterios de aceptacion:**

- Existe una consulta de verificacion que responde si el servicio esta operativo ("Healthy") o no.
- Esta consulta no requiere clave de acceso (para que herramientas de monitoreo puedan usarla).
- Puede ser utilizada por balanceadores de carga y sistemas de alertas.

---

#### HU-08 - Registro y actualizacion manual del tipo de cambio

**Como** analista contable, **quiero** contar con una interfaz web donde pueda registrar o actualizar manualmente el tipo de cambio de cualquier moneda para una fecha determinada, **para** cubrir los casos en que la fuente automatica no disponga del dato o se requiera aplicar un valor especifico aprobado por el area contable.

**Criterios de aceptacion:**

- La interfaz web permite seleccionar la moneda y la fecha para la cual se desea registrar o actualizar el tipo de cambio.
- El usuario ingresa los valores de compra y venta. El sistema calcula automaticamente el promedio.
- Si ya existe un tipo de cambio registrado para esa moneda y fecha, el sistema muestra los valores actuales y permite actualizarlos previa confirmacion.
- Si no existe registro previo, el sistema permite crear uno nuevo.
- El valor de compra no puede ser mayor al valor de venta. Ambos deben ser mayores a cero.
- El registro manual queda identificado con la fuente de origen "MANUAL" para diferenciarlo de los obtenidos automaticamente.
- El sistema registra el usuario que realizo el cambio y la fecha/hora de la operacion para fines de auditoria.
- Solo usuarios autorizados pueden acceder a esta funcionalidad.
- La interfaz muestra un listado de los ultimos tipos de cambio registrados (automaticos y manuales) para facilitar la verificacion.

**Escenarios de uso:**

| Escenario | Descripcion |
|-----------|-------------|
| Fuente no disponible | La fuente automatica (BCRP/SBS) no publico el tipo de cambio a tiempo y se necesita registrarlo manualmente para no retrasar las operaciones del dia. |
| Moneda sin fuente automatica | Se requiere el tipo de cambio de una moneda que no esta cubierta por la fuente automatica (por ejemplo, GBP si solo se sincroniza USD y EUR). |
| Correccion contable | El area contable determina que debe aplicarse un valor especifico diferente al publicado por la fuente automatica (por ejemplo, un tipo de cambio de cierre aprobado por gerencia). |
| Carga historica | Se necesita registrar tipos de cambio de fechas pasadas que no fueron capturados por el proceso automatico. |

---

## ARQUITECTURA TECNICA

### Diagrama de componentes

```
  +-------------+    +-------------+    +-------------+
  |    BCRP     |    | SBS Portal  |    |   SUNAT     |
  | (API REST)  |    |   (Web)     |    |  (Portal)   |
  +------+------+    +------+------+    +------+------+
         |                  |                  |
         | HTTPS/JSON       | HTTPS/HTML       | HTTPS/HTML
         |                  | (scraping)       | (scraping)
         +--------+---------+---------+--------+
                  |                   |
                  v                   v
+---------------------------------------------------+
|                  Docker Compose                    |
|                                                    |
|  +----------+    +----------+    +-----------+     |
|  |  Worker  |--->|   MySQL  |<---|    API    |     |
|  | (sync)   |    |   (db)   |    |  (REST)   |     |
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
                                  |   Modulos   |
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
| FechaSbs | DATE | Fecha de publicacion de la fuente |
| FuenteOrigen | VARCHAR(50) | Origen del dato: `BCRP`, `SBS`, `SUNAT`, `MANUAL` |
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
GET https://api.tecsur.com.pe:8089/api/v1/tipocambio/usd/2026-06-22
Header: X-Api-Key: <clave configurada>
```

Si no existe tasa para ese dia (feriado/fin de semana), usar el endpoint `/ultima`:

```
GET https://api.tecsur.com.pe:8089/api/v1/tipocambio/usd/2026-06-22/ultima
Header: X-Api-Key: <clave configurada>
```

---

## FUERA DEL ALCANCE

- Notificaciones o alertas automaticas cuando el tipo de cambio no se haya actualizado.
- Conversion de montos entre monedas (la API solo provee las tasas, no realiza calculos de conversion).
- Gestion de usuarios y roles para el acceso a la API (actualmente se usa una unica API Key compartida).
- Implementacion simultanea de multiples fuentes con fallback automatico (evaluable en version futura).

---

## RIESGOS

| # | Riesgo | Impacto | Mitigacion |
|---|--------|---------|------------|
| 1 | La fuente seleccionada (BCRP/SBS/SUNAT) modifica su estructura o descontinua el servicio sin previo aviso. | Alto - Se detiene la actualizacion automatica. | Monitoreo de logs del Worker; la arquitectura soporta cambio de fuente via configuracion (`FuenteOrigen`). Implementar fuente alternativa como respaldo. |
| 2 | Para fuentes sin API (SBS portal, SUNAT), cambios en el HTML del portal rompen el scraping. | Alto - El Worker deja de obtener datos. | Si se usa scraping: monitorear y mantener selectores HTML. Preferir la API del BCRP para minimizar este riesgo. |
| 3 | Indisponibilidad de la base de datos externa (`10.160.9.18`). | Alto - Ni la API ni el Worker funcionan. | `EnableRetryOnFailure` configurado; monitoreo de conectividad. |
| 4 | El area contable/tributaria determina que se requiere una fuente diferente a la implementada inicialmente. | Medio - Requiere desarrollo adicional para la nueva fuente. | El campo `FuenteOrigen` ya permite diferenciar la fuente; la arquitectura soporta multiples origenes. El esfuerzo de agregar una nueva fuente es contenido. |
| 5 | Los datos diarios del BCRP solo cubren USD y EUR; se requieren mas monedas con frecuencia diaria. | Medio - Requiere fuente adicional para otras monedas. | Complementar con scraping del portal SBS Contable (que publica USD, EUR, GBP, JPY, CHF, CAD, AUD) o usar series mensuales del BCRP. |
| 6 | No contar con la disponibilidad del usuario para efectuar las pruebas funcionales. | Medio - Retraso en la validacion. | Definir cronograma de pruebas con fechas comprometidas. |

---

## COSTOS DE LOS SERVICIOS

| Concepto | SBS Prom. Ponderado | SBS Contable | BCRP | SUNAT (directo) | SUNAT (terceros) |
|----------|--------------------:|-------------:|-----:|----------------:|-----------------:|
| Uso del servicio | Gratuito | Gratuito | **Gratuito** | Gratuito | Gratuito con limites / planes de pago |
| Token o credencial | N/A | N/A | **No requiere** | N/A | Registro en plataforma del tercero |
| Infraestructura | Incluido | Incluido | **Incluido** | Incluido | Incluido |
| Desarrollo de integracion | 60-80 hrs (scraping) | 60-80 hrs (scraping) | **20-30 hrs (API REST)** | 60-80 hrs (scraping) | 30-40 hrs (API tercero) |
| Mantenimiento anual estimado | ~20 hrs (scraping fragil) | ~20 hrs (scraping fragil) | **~4 hrs (API estable)** | ~20 hrs (scraping fragil) | ~8 hrs |
| Costo monetario anual | $0 | $0 | **$0** | $0 | $0 - $200+ (segun plan) |

> **Conclusion de costos:** La opcion con menor costo total (desarrollo + mantenimiento) es el **BCRP**, por contar con una API REST estable que no requiere scraping ni dependencias de terceros.

---

## REGISTRO DE CAMBIOS

| Version | Causa del cambio | Responsable del cambio | Fecha del cambio |
|---------|------------------|------------------------|------------------|
| 1.0 | Creacion del documento (solo SBS y SUNAT) | _[Nombre]_ | _[DD.MM.YYYY]_ |
| 2.0 | Ampliacion con 4 fuentes: SBS Promedio Ponderado, SBS Contable, BCRP y SUNAT. Inclusion de datos reales verificados del BCRP. Recomendacion actualizada. | _[Nombre]_ | _[DD.MM.YYYY]_ |
| | | | |
