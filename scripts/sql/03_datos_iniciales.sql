-- ============================================================
-- Script 03: Datos iniciales (monedas estándar)
-- Proyecto : AplicacionTipoCambio
-- ============================================================

USE tasa_cambio_db;

INSERT INTO ttc_moneda (
    ttc_codigo, ttc_descripcion, ttc_simbolo,
    ttc_codigo_sunat, ttc_descripcion_iso4217,
    ttc_usuario_reg
) VALUES
    ('USD', 'Dólar Americano',     '$',  '02', 'USD', 'SISTEMA'),
    ('EUR', 'Euro',                '€',  '05', 'EUR', 'SISTEMA'),
    ('PEN', 'Sol Peruano',         'S/', '01', 'PEN', 'SISTEMA'),
    ('GBP', 'Libra Esterlina',     '£',  '06', 'GBP', 'SISTEMA'),
    ('JPY', 'Yen Japonés',         '¥',  '09', 'JPY', 'SISTEMA'),
    ('CHF', 'Franco Suizo',        'Fr', '12', 'CHF', 'SISTEMA'),
    ('CAD', 'Dólar Canadiense',    '$',  '10', 'CAD', 'SISTEMA'),
    ('BRL', 'Real Brasileño',      'R$', '11', 'BRL', 'SISTEMA')
ON DUPLICATE KEY UPDATE
    ttc_descripcion = VALUES(ttc_descripcion),
    ttc_usuario_act = 'SISTEMA',
    ttc_fecha_act   = CURRENT_TIMESTAMP;
