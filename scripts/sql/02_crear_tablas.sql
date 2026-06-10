-- ============================================================
-- Script 02: Crear tablas
-- Proyecto : AplicacionTipoCambio
-- Convención: PascalCase, sin guiones bajos, prefijo "tt"
-- ============================================================

USE tasa_cambio_db;

-- ------------------------------------------------------------
-- Tabla: ttmoneda
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ttmoneda (
    IdMoneda            BIGINT          NOT NULL AUTO_INCREMENT,
    Codigo              VARCHAR(10)     NOT NULL,
    Descripcion         VARCHAR(100)    NOT NULL,
    Simbolo             VARCHAR(10)     NOT NULL,
    CodigoSunat         VARCHAR(10)     NULL,
    DescripcionIso4217  VARCHAR(50)     NULL,
    UsuarioReg          VARCHAR(50)     NOT NULL,
    FechaReg            DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct          VARCHAR(50)     NULL,
    FechaAct            DATETIME        NULL ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT PkTtMoneda PRIMARY KEY (IdMoneda),
    CONSTRAINT UqTtMonedaCodigo UNIQUE (Codigo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE INDEX IdxTtMonedaCodigo ON ttmoneda (Codigo);

-- ------------------------------------------------------------
-- Tabla: tttasacambio
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS tttasacambio (
    IdTasaCambio    BIGINT          NOT NULL AUTO_INCREMENT,
    CodigoMoneda    VARCHAR(10)     NOT NULL,
    Fecha           DATE            NOT NULL,
    ValorCompra     DECIMAL(18,6)   NOT NULL,
    ValorVenta      DECIMAL(18,6)   NOT NULL,
    FechaSbs        DATE            NULL,
    FuenteOrigen    VARCHAR(50)     NULL,
    UsuarioReg      VARCHAR(50)     NOT NULL,
    FechaReg        DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsuarioAct      VARCHAR(50)     NULL,
    FechaAct        DATETIME        NULL ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT PkTtTasaCambio PRIMARY KEY (IdTasaCambio),
    CONSTRAINT UqTtTasaCambioMonedaFecha UNIQUE (CodigoMoneda, Fecha),
    CONSTRAINT FkTtTasaCambioMoneda
        FOREIGN KEY (CodigoMoneda)
        REFERENCES ttmoneda (Codigo)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE INDEX IdxTtTasaCambioMonedaFecha ON tttasacambio (CodigoMoneda, Fecha);
CREATE INDEX IdxTtTasaCambioFecha ON tttasacambio (Fecha);
