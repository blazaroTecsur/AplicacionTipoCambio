-- ============================================================
-- Script 06: Agregar columnas de estado de sincronización SyteLine
-- Proyecto : AplicacionTipoCambio
-- ============================================================

USE tasa_cambio_db;

ALTER TABLE tttasacambio
    ADD COLUMN SincronizadoSyteline  TINYINT(1)  NOT NULL DEFAULT 0        AFTER FuenteOrigen,
    ADD COLUMN FechaUltSincSyteline  DATETIME    NULL                       AFTER SincronizadoSyteline;
