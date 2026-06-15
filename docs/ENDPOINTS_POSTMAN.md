# Endpoints API - Guía para Postman

Base URL (servidor publicado):

```
https://api.tecsur.com.pe:8089
```

## Autenticación

Todos los endpoints, excepto `/health` y `/swagger`, requieren el header:

| Key | Value |
|-----|-------|
| `X-Api-Key` | `<valor de API_KEY configurado en el .env del servidor>` |

En Postman:
- Pestaña **Authorization**: dejar en `No Auth`.
- Pestaña **Headers**: agregar `X-Api-Key`.

---

## 1. Health Check

**No requiere `X-Api-Key`.**

```
GET /health
```

### Respuesta 200 OK
```
Healthy
```

---

## 2. Listar monedas

```
GET /api/v1/moneda
```

### Headers
| Key | Value |
|-----|-------|
| X-Api-Key | `<tu api key>` |

### Respuesta 200 OK
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "codigo": "USD",
      "descripcion": "Dólar Americano",
      "simbolo": "$",
      "codigoSunat": "02",
      "descripcionIso4217": "USD"
    },
    {
      "id": 2,
      "codigo": "EUR",
      "descripcion": "Euro",
      "simbolo": "€",
      "codigoSunat": "05",
      "descripcionIso4217": "EUR"
    },
    {
      "id": 3,
      "codigo": "PEN",
      "descripcion": "Sol Peruano",
      "simbolo": "S/",
      "codigoSunat": "01",
      "descripcionIso4217": "PEN"
    }
  ],
  "message": null,
  "errors": []
}
```

---

## 3. Listar tasas de cambio de una moneda

```
GET /api/v1/tipocambio/{codigoMoneda}
GET /api/v1/tipocambio/{codigoMoneda}?anio={anio}&mes={mes}
```

### Parámetros
| Parámetro | Tipo | Ubicación | Requerido | Ejemplo |
|-----------|------|-----------|-----------|---------|
| codigoMoneda | string | path | sí | USD |
| anio | int | query | no | 2025 |
| mes | int | query | no | 6 |

### Ejemplo
```
GET /api/v1/tipocambio/usd?anio=2025&mes=6
```

### Headers
| Key | Value |
|-----|-------|
| X-Api-Key | `<tu api key>` |

### Respuesta 200 OK
```json
{
  "success": true,
  "data": [
    {
      "id": 10,
      "codigoMoneda": "USD",
      "fecha": "2025-06-01",
      "valorCompra": 3.700,
      "valorVenta": 3.750,
      "tasaPromedio": 3.725,
      "fuenteOrigen": "SBS",
      "fechaSbs": "2025-06-01",
      "detalleMoneda": {
        "id": 1,
        "codigo": "USD",
        "descripcion": "Dólar Americano",
        "simbolo": "$",
        "codigoSunat": "02",
        "descripcionIso4217": "USD"
      }
    }
  ],
  "message": null,
  "errors": []
}
```

---

## 4. Obtener tasa de cambio por fecha exacta

```
GET /api/v1/tipocambio/{codigoMoneda}/{fecha}
```

### Parámetros
| Parámetro | Tipo | Ubicación | Requerido | Ejemplo |
|-----------|------|-----------|-----------|---------|
| codigoMoneda | string | path | sí | USD |
| fecha | date (yyyy-MM-dd) | path | sí | 2025-06-01 |

### Ejemplo
```
GET /api/v1/tipocambio/usd/2025-06-01
```

### Headers
| Key | Value |
|-----|-------|
| X-Api-Key | `<tu api key>` |

### Respuesta 200 OK
```json
{
  "success": true,
  "data": {
    "id": 10,
    "codigoMoneda": "USD",
    "fecha": "2025-06-01",
    "valorCompra": 3.700,
    "valorVenta": 3.750,
    "tasaPromedio": 3.725,
    "fuenteOrigen": "SBS",
    "fechaSbs": "2025-06-01",
    "detalleMoneda": {
      "id": 1,
      "codigo": "USD",
      "descripcion": "Dólar Americano",
      "simbolo": "$",
      "codigoSunat": "02",
      "descripcionIso4217": "USD"
    }
  },
  "message": null,
  "errors": []
}
```

### Respuesta 404 Not Found
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "TasaCambio con identificador 'USD-2025-06-01' no fue encontrado."
  ]
}
```

---

## 5. Obtener última tasa de cambio antes/igual a una fecha

```
GET /api/v1/tipocambio/{codigoMoneda}/{fecha}/ultima
```

### Parámetros
| Parámetro | Tipo | Ubicación | Requerido | Ejemplo |
|-----------|------|-----------|-----------|---------|
| codigoMoneda | string | path | sí | USD |
| fecha | date (yyyy-MM-dd) | path | sí | 2025-06-10 |

### Ejemplo
```
GET /api/v1/tipocambio/usd/2025-06-10/ultima
```

### Headers
| Key | Value |
|-----|-------|
| X-Api-Key | `<tu api key>` |

### Respuesta 200 OK
Igual estructura que el endpoint anterior: devuelve la tasa de cambio vigente más reciente con fecha menor o igual a la indicada.

```json
{
  "success": true,
  "data": {
    "id": 10,
    "codigoMoneda": "USD",
    "fecha": "2025-06-05",
    "valorCompra": 3.715,
    "valorVenta": 3.765,
    "tasaPromedio": 3.740,
    "fuenteOrigen": "SBS",
    "fechaSbs": "2025-06-05",
    "detalleMoneda": {
      "id": 1,
      "codigo": "USD",
      "descripcion": "Dólar Americano",
      "simbolo": "$",
      "codigoSunat": "02",
      "descripcionIso4217": "USD"
    }
  },
  "message": null,
  "errors": []
}
```

### Respuesta 404 Not Found
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "TasaCambio con identificador 'USD-2025-06-10' no fue encontrado."
  ]
}
```

---

## Errores comunes

### 401 Unauthorized - Falta header
```
API Key requerida.
```

### 401 Unauthorized - Header incorrecto
```
API Key inválida.
```

### 400 Bad Request (validación de parámetros)
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "El código de moneda es requerido."
  ]
}
```

### 422 Unprocessable Entity (reglas de negocio)
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "<mensaje de la regla de negocio violada>"
  ]
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "Ocurrió un error interno."
  ]
}
```
