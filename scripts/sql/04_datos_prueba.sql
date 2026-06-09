-- ============================================================
-- Script 04: Datos de prueba (tasas de cambio)
-- Proyecto : AplicacionTipoCambio
-- SOLO para entornos de desarrollo/QA
-- ============================================================

USE tasa_cambio_db;

INSERT INTO ttc_tasa_cambio (
    ttc_empresa, ttc_codigo_moneda, ttc_fecha,
    ttc_valor_compra, ttc_valor_venta,
    ttc_fuente_origen, ttc_usuario_reg
) VALUES
    ('LDS', 'USD', '2025-06-01', 3.700, 3.750, 'SBS', 'SISTEMA'),
    ('LDS', 'USD', '2025-06-02', 3.705, 3.755, 'SBS', 'SISTEMA'),
    ('LDS', 'USD', '2025-06-03', 3.698, 3.748, 'SBS', 'SISTEMA'),
    ('LDS', 'USD', '2025-06-04', 3.710, 3.760, 'SBS', 'SISTEMA'),
    ('LDS', 'USD', '2025-06-05', 3.715, 3.765, 'SBS', 'SISTEMA'),
    ('LDS', 'EUR', '2025-06-01', 4.020, 4.080, 'SBS', 'SISTEMA'),
    ('LDS', 'EUR', '2025-06-02', 4.015, 4.075, 'SBS', 'SISTEMA'),
    ('LDS', 'EUR', '2025-06-03', 4.025, 4.085, 'SBS', 'SISTEMA')
ON DUPLICATE KEY UPDATE
    ttc_valor_compra = VALUES(ttc_valor_compra),
    ttc_valor_venta  = VALUES(ttc_valor_venta),
    ttc_usuario_act  = 'SISTEMA',
    ttc_fecha_act    = CURRENT_TIMESTAMP;
