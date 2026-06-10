-- ============================================================
-- Script 03: Datos iniciales (monedas estándar)
-- Proyecto : AplicacionTipoCambio
-- ============================================================

USE tasa_cambio_db;

INSERT INTO ttmoneda (
    Codigo, Descripcion, Simbolo,
    CodigoSunat, DescripcionIso4217,
    UsuarioReg
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
    Descripcion = VALUES(Descripcion),
    UsuarioAct  = 'SISTEMA',
    FechaAct    = CURRENT_TIMESTAMP;
