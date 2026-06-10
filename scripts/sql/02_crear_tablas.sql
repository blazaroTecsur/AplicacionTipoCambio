-- ============================================================
-- Script 02: Crear tablas
-- Proyecto : AplicacionTipoCambio
-- Convención: snake_case, prefijo ttc_
-- ============================================================

USE tasa_cambio_db;

-- ------------------------------------------------------------
-- Tabla: ttc_moneda
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ttc_moneda (
    ttc_id                  BIGINT          NOT NULL AUTO_INCREMENT,
    ttc_codigo              VARCHAR(10)     NOT NULL,
    ttc_descripcion         VARCHAR(100)    NOT NULL,
    ttc_simbolo             VARCHAR(10)     NOT NULL,
    ttc_codigo_sunat        VARCHAR(10)     NULL,
    ttc_descripcion_iso4217 VARCHAR(50)     NULL,
    ttc_usuario_reg         VARCHAR(50)     NOT NULL,
    ttc_fecha_reg           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ttc_usuario_act         VARCHAR(50)     NULL,
    ttc_fecha_act           DATETIME        NULL ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT pk_ttc_moneda PRIMARY KEY (ttc_id),
    CONSTRAINT uq_ttc_moneda_codigo UNIQUE (ttc_codigo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE INDEX idx_ttc_moneda_codigo ON ttc_moneda (ttc_codigo);

-- ------------------------------------------------------------
-- Tabla: ttc_tasa_cambio
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ttc_tasa_cambio (
    ttc_id              BIGINT          NOT NULL AUTO_INCREMENT,
    ttc_codigo_moneda   VARCHAR(10)     NOT NULL,
    ttc_fecha           DATE            NOT NULL,
    ttc_valor_compra    DECIMAL(18,6)   NOT NULL,
    ttc_valor_venta     DECIMAL(18,6)   NOT NULL,
    ttc_fecha_sbs       DATE            NULL,
    ttc_fuente_origen   VARCHAR(50)     NULL,
    ttc_usuario_reg     VARCHAR(50)     NOT NULL,
    ttc_fecha_reg       DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ttc_usuario_act     VARCHAR(50)     NULL,
    ttc_fecha_act       DATETIME        NULL ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT pk_ttc_tasa_cambio PRIMARY KEY (ttc_id),
    CONSTRAINT uq_ttc_tasa_cambio_moneda_fecha UNIQUE (ttc_codigo_moneda, ttc_fecha),
    CONSTRAINT fk_ttc_tasa_cambio_moneda
        FOREIGN KEY (ttc_codigo_moneda)
        REFERENCES ttc_moneda (ttc_codigo)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE INDEX idx_ttc_moneda_fecha ON ttc_tasa_cambio (ttc_codigo_moneda, ttc_fecha);
CREATE INDEX idx_ttc_fecha ON ttc_tasa_cambio (ttc_fecha);
