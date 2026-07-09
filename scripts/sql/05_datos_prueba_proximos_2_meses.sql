-- ============================================================
-- Script 05: Datos de prueba - próximos 2 meses
-- Proyecto : AplicacionTipoCambio
-- SOLO para entornos de desarrollo/QA
--
-- Genera tasas de cambio diarias para USD, EUR, GBP y PEN,
-- desde la fecha actual hasta 59 días después (~2 meses).
-- ============================================================

USE tasa_cambio_db;

-- ------------------------------------------------------------
-- USD
-- ------------------------------------------------------------
INSERT INTO tttasacambio (
    CodigoMoneda, Fecha, ValorCompra, ValorVenta, FechaSbs, FuenteOrigen, UsuarioReg
)
WITH RECURSIVE fechas AS (
    SELECT CURDATE() AS Fecha, 0 AS n
    UNION ALL
    SELECT Fecha + INTERVAL 1 DAY, n + 1 FROM fechas WHERE n < 59
)
SELECT
    'USD',
    Fecha,
    ROUND(3.700 + (n % 10) * 0.004, 3)         AS ValorCompra,
    ROUND(3.700 + (n % 10) * 0.004 + 0.045, 3) AS ValorVenta,
    Fecha,
    'SBS',
    'SISTEMA'
FROM fechas
ON DUPLICATE KEY UPDATE
    ValorCompra = VALUES(ValorCompra),
    ValorVenta  = VALUES(ValorVenta),
    FechaSbs    = VALUES(FechaSbs),
    UsuarioAct  = 'SISTEMA',
    FechaAct    = CURRENT_TIMESTAMP;

-- ------------------------------------------------------------
-- EUR
-- ------------------------------------------------------------
INSERT INTO tttasacambio (
    CodigoMoneda, Fecha, ValorCompra, ValorVenta, FechaSbs, FuenteOrigen, UsuarioReg
)
WITH RECURSIVE fechas AS (
    SELECT CURDATE() AS Fecha, 0 AS n
    UNION ALL
    SELECT Fecha + INTERVAL 1 DAY, n + 1 FROM fechas WHERE n < 59
)
SELECT
    'EUR',
    Fecha,
    ROUND(4.000 + (n % 10) * 0.005, 3)         AS ValorCompra,
    ROUND(4.000 + (n % 10) * 0.005 + 0.060, 3) AS ValorVenta,
    Fecha,
    'SBS',
    'SISTEMA'
FROM fechas
ON DUPLICATE KEY UPDATE
    ValorCompra = VALUES(ValorCompra),
    ValorVenta  = VALUES(ValorVenta),
    FechaSbs    = VALUES(FechaSbs),
    UsuarioAct  = 'SISTEMA',
    FechaAct    = CURRENT_TIMESTAMP;

-- ------------------------------------------------------------
-- GBP
-- ------------------------------------------------------------
INSERT INTO tttasacambio (
    CodigoMoneda, Fecha, ValorCompra, ValorVenta, FechaSbs, FuenteOrigen, UsuarioReg
)
WITH RECURSIVE fechas AS (
    SELECT CURDATE() AS Fecha, 0 AS n
    UNION ALL
    SELECT Fecha + INTERVAL 1 DAY, n + 1 FROM fechas WHERE n < 59
)
SELECT
    'GBP',
    Fecha,
    ROUND(4.700 + (n % 10) * 0.006, 3)         AS ValorCompra,
    ROUND(4.700 + (n % 10) * 0.006 + 0.070, 3) AS ValorVenta,
    Fecha,
    'SBS',
    'SISTEMA'
FROM fechas
ON DUPLICATE KEY UPDATE
    ValorCompra = VALUES(ValorCompra),
    ValorVenta  = VALUES(ValorVenta),
    FechaSbs    = VALUES(FechaSbs),
    UsuarioAct  = 'SISTEMA',
    FechaAct    = CURRENT_TIMESTAMP;

-- ------------------------------------------------------------
-- PEN
-- ------------------------------------------------------------
INSERT INTO tttasacambio (
    CodigoMoneda, Fecha, ValorCompra, ValorVenta, FechaSbs, FuenteOrigen, UsuarioReg
)
WITH RECURSIVE fechas AS (
    SELECT CURDATE() AS Fecha, 0 AS n
    UNION ALL
    SELECT Fecha + INTERVAL 1 DAY, n + 1 FROM fechas WHERE n < 59
)
SELECT
    'PEN',
    Fecha,
    ROUND(1.000 + (n % 10) * 0.0005, 4)         AS ValorCompra,
    ROUND(1.000 + (n % 10) * 0.0005 + 0.0050, 4) AS ValorVenta,
    Fecha,
    'SBS',
    'SISTEMA'
FROM fechas
ON DUPLICATE KEY UPDATE
    ValorCompra = VALUES(ValorCompra),
    ValorVenta  = VALUES(ValorVenta),
    FechaSbs    = VALUES(FechaSbs),
    UsuarioAct  = 'SISTEMA',
    FechaAct    = CURRENT_TIMESTAMP;
