# SOLICITUD DE DESARROLLO DE SOFTWARE

---

| Campo | Detalle |
|-------|---------|
| **Fecha** | _[DD/MM/YYYY]_ |
| **Area Solicitante** | Contabilidad |

---

## I. Descripcion del Requerimiento

Desarrollar e implementar un modulo de obtencion y gestion automatica del tipo de cambio como parte de la plataforma interna ERP, que permita:

- **Obtencion automatica del tipo de cambio:** Un servicio en segundo plano (Worker) que consulte periodicamente fuentes oficiales (BCRP, SBS o SUNAT) para obtener las tasas de cambio de compra y venta, y las registre automaticamente en la base de datos sin intervencion manual.

- **Consulta del tipo de cambio via servicio web (API REST):** Un servicio que exponga las tasas de cambio almacenadas para ser consumidas por otros modulos del ERP (comprobantes de pago, ordenes de compra, contabilidad, integracion con Infor Syteline), permitiendo consultar por moneda, fecha exacta, periodo o ultima tasa vigente.

- **Registro y actualizacion manual del tipo de cambio:** Una interfaz web que permita al analista contable registrar o corregir manualmente el tipo de cambio de cualquier moneda para una fecha determinada, en caso de que la obtencion automatica no haya sido posible o se requiera un ajuste.

- **Soporte multimoneda:** El sistema debe soportar las principales monedas utilizadas en las operaciones de comercio exterior y contabilidad: Dolar Americano (USD), Euro (EUR), Libra Esterlina (GBP), Yen Japones (JPY), Franco Suizo (CHF), Dolar Canadiense (CAD) y Real Brasileno (BRL).

- **Seguridad y auditoria:** Toda operacion de registro o actualizacion (automatica o manual) queda registrada con la fuente de origen, el usuario responsable, y fecha/hora para trazabilidad y auditoria.

---

## II. Origen / Justificacion de la Necesidad

La necesidad surge a partir de la adopcion del ERP Infor Syteline como plataforma contable y operativa central para Tecsur, GCI y Los Andes.

Actualmente, el tipo de cambio se obtiene y registra de forma manual por el area de contabilidad, consultando diariamente el portal web de la SBS o SUNAT y transcribiendo los valores al sistema. Este proceso:

- Es propenso a errores de transcripcion que pueden afectar la valoracion de operaciones en moneda extranjera.
- Consume tiempo diario del analista contable (~15-20 minutos por dia habiles).
- No garantiza que el tipo de cambio este disponible antes del inicio de operaciones (7:00 AM).
- Carece de trazabilidad sobre quien registro el valor, cuando y de que fuente provino.
- No permite que otros modulos del ERP (comprobantes de pago, ordenes de compra) obtengan el tipo de cambio de forma automatica e integrada.

La implementacion de este modulo permitira:

- Eliminar el registro manual diario, liberando tiempo del area contable.
- Garantizar que el tipo de cambio este disponible de forma oportuna y precisa para todas las operaciones.
- Centralizar la informacion del tipo de cambio en una unica fuente de verdad para las tres empresas del grupo.
- Facilitar la integracion con Infor Syteline y otros modulos del ERP mediante un servicio web estandarizado.
- Mantener trazabilidad completa para efectos de auditoria interna y externa.

---

## III. Criticidad

| Nivel | Seleccion |
|-------|-----------|
| Critico | |
| **Importante** | **X** |
| Deseado | |

**Justificacion:** El tipo de cambio es un dato requerido diariamente para las operaciones contables, tributarias y de comercio exterior. Su disponibilidad oportuna y correcta impacta directamente en la valoracion de comprobantes de pago, ordenes de compra y estados financieros. Sin embargo, el proceso manual actual permite la continuidad operativa (con riesgo de error y demora), por lo que se clasifica como "Importante" y no como "Critico".

---

## IV. Areas involucradas

| Area | Rol |
|------|-----|
| Proyecto ERP | Desarrollo e implementacion del modulo |
| Contabilidad | Usuario principal. Define reglas de negocio, valida fuentes de tipo de cambio, realiza pruebas funcionales |
| Tesoreria | Usuario consumidor del tipo de cambio para operaciones en moneda extranjera |
| Tecnologia / Infraestructura | Despliegue en servidores, configuracion de contenedores Docker, monitoreo |
| Auditoria Interna | Validacion de la trazabilidad y controles del modulo |

---

## V. Analisis de costo-beneficio

### Costo

| Concepto | Estimacion |
|----------|------------|
| Desarrollo del Worker (obtencion automatica) | 20 - 30 horas |
| Desarrollo de la API REST (consultas) | Implementado (version actual operativa) |
| Desarrollo de interfaz web (registro/edicion manual) | 40 - 60 horas |
| Integracion con fuente de datos (BCRP API) | 10 - 15 horas |
| Pruebas funcionales y ajustes | 15 - 20 horas |
| Despliegue y configuracion en servidor | 5 - 10 horas |
| **Total estimado** | **90 - 135 horas** |
| Costo de licencias o servicios externos | **$0** (BCRP es gratuito y sin registro) |
| Infraestructura adicional | **$0** (se usa la infraestructura existente - Docker en servidor actual) |

### Beneficio

| Beneficio | Detalle |
|-----------|---------|
| Eliminacion del registro manual diario | Ahorro de ~15-20 min/dia x ~250 dias habiles/anio = ~62-83 horas/anio de trabajo contable |
| Reduccion de errores de transcripcion | Eliminacion del riesgo de errores al copiar manualmente valores del portal SBS/SUNAT |
| Disponibilidad oportuna | Tipo de cambio disponible antes de las 7:00 AM, sin depender de que el analista lo registre |
| Centralizacion | Una unica fuente de verdad para Tecsur, GCI y Los Andes, accesible por todos los modulos del ERP |
| Trazabilidad y auditoria | Registro automatico de fuente, usuario y fecha/hora en cada operacion |
| Integracion con Syteline | Servicio web estandarizado que permite a Infor Syteline y otros modulos consultar el tipo de cambio de forma programatica |

### Costo-Beneficio

El modulo se autofinancia en el primer anio: el ahorro de horas del area contable (~62-83 horas/anio) supera el costo de mantenimiento anual (~4 horas/anio con la API del BCRP). Ademas, los beneficios intangibles (reduccion de errores, disponibilidad oportuna, trazabilidad) justifican ampliamente la inversion inicial de desarrollo.

---

_Firma del Jefe de Departamento_

___________________________

Nombre: _[Nombre del Jefe]_

Cargo: _[Cargo]_

Fecha: _[DD/MM/YYYY]_
