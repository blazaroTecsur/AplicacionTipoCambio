# Correo: Relevamiento de consumidores API Tipo de Cambio

---

**Para:** _[Equipo / Nombre del destinatario]_
**CC:** _[Jefe de Proyecto / Area solicitante]_
**Asunto:** Relevamiento técnico — Integración con API de Tipo de Cambio

---

Estimado(a) _[Nombre]_,

Me dirijo a usted a fin de relevar los requisitos técnicos necesarios para asegurar una correcta planificación, dimensionamiento y configuración del servicio de API de Tipo de Cambio que actualmente se encuentra en proceso de implementación como parte de la plataforma ERP corporativa.

Con el objetivo de garantizar la disponibilidad, el rendimiento y la compatibilidad de la integración, le solicito tenga a bien responder las siguientes consultas:

---

**1. Aplicativos o sistemas que consumirán la API**

Indicar el nombre y tipo de cada sistema o módulo que realizará consultas a la API de Tipo de Cambio, incluyendo:

- Nombre del aplicativo o módulo.
- Plataforma o tecnología utilizada (por ejemplo: .NET, Java, Python, Power BI, Infor Syteline, entre otros).
- Ambiente desde el cual se realizará el consumo (Desarrollo, QA, Producción).
- Responsable técnico del aplicativo.

---

**2. Frecuencia y volumen de consumo**

Indicar, por cada aplicativo o módulo identificado en el punto anterior:

- Frecuencia de consulta estimada (por ejemplo: una vez por día al inicio de operaciones, en cada registro de comprobante, en tiempo real, por lote nocturno, etc.).
- Volumen estimado de peticiones por día o por hora en condiciones normales de operación.
- Si se contemplan picos de carga (por ejemplo: cierres mensuales, procesos de conciliación masiva), indicar la frecuencia y volumen esperados durante dichos picos.
- Endpoint(s) que se utilizarán preferentemente:
  - Consulta por fecha exacta: `GET /api/v1/tipocambio/{moneda}/{fecha}`
  - Última tasa vigente: `GET /api/v1/tipocambio/{moneda}/{fecha}/ultima`
  - Listado por periodo: `GET /api/v1/tipocambio/{moneda}?anio=&mes=`

---

Esta información es necesaria para definir las políticas de rate limiting (límite de peticiones por minuto/hora), dimensionar la infraestructura de manera adecuada, y coordinar la entrega de credenciales de acceso (API Key) a cada equipo consumidor.

Quedo a disposición para cualquier consulta o para coordinar una reunión de alineamiento técnico si fuera necesario.

Agradezco su atención y pronta respuesta.

Saludos,

_[Nombre del remitente]_
_[Cargo]_
_[Area]_
_[Correo electronico]_
_[Telefono / interno]_
