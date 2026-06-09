-- ============================================================
-- Script 03: Datos iniciales (monedas estándar)
-- Proyecto : AplicacionTipoCambio
-- ============================================================

USE tasa_cambio_db;

INSERT INTO ttc_moneda (
    ttc_empresa, ttc_codigo, ttc_descripcion, ttc_simbolo,
    ttc_codigo_sunat, ttc_descripcion_iso4217,
    ttc_usuario_reg
) VALUES
    ('LDS', 'USD', 'Dólar Americano',     '$',  '02', 'USD', 'SISTEMA'),
    ('LDS', 'EUR', 'Euro',                '€',  '05', 'EUR', 'SISTEMA'),
    ('LDS', 'PEN', 'Sol Peruano',         'S/', '01', 'PEN', 'SISTEMA'),
    ('LDS', 'GBP', 'Libra Esterlina',     '£',  '06', 'GBP', 'SISTEMA'),
    ('LDS', 'JPY', 'Yen Japonés',         '¥',  '09', 'JPY', 'SISTEMA'),
    ('LDS', 'CHF', 'Franco Suizo',        'Fr', '12', 'CHF', 'SISTEMA'),
    ('LDS', 'CAD', 'Dólar Canadiense',    '$',  '10', 'CAD', 'SISTEMA'),
    ('LDS', 'BRL', 'Real Brasileño',      'R$', '11', 'BRL', 'SISTEMA')
ON DUPLICATE KEY UPDATE
    ttc_descripcion = VALUES(ttc_descripcion),
    ttc_usuario_act = 'SISTEMA',
    ttc_fecha_act   = CURRENT_TIMESTAMP;
