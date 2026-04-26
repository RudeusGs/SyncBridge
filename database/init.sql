CREATE TABLE IF NOT EXISTS products (
    id                INT PRIMARY KEY,
    name              VARCHAR(255) NOT NULL,
    sku               VARCHAR(100) NOT NULL,
    price             NUMERIC(18,2) NOT NULL,
    currency          VARCHAR(10) NOT NULL,
    source_updated_at TIMESTAMPTZ NOT NULL,
    synced_at         TIMESTAMPTZ NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_products_source_updated_at
    ON products (source_updated_at);

CREATE TABLE IF NOT EXISTS sync_checkpoint (
    job_name        VARCHAR(100) PRIMARY KEY,
    last_synced_at  TIMESTAMPTZ NULL,
    last_synced_id  INT NULL,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS sync_log (
    id              BIGSERIAL PRIMARY KEY,
    job_name        VARCHAR(100) NOT NULL,
    started_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ended_at        TIMESTAMPTZ NULL,
    status          VARCHAR(50) NOT NULL,
    processed_count INT NOT NULL DEFAULT 0,
    error_message   TEXT NULL
);

CREATE INDEX IF NOT EXISTS idx_sync_log_job_name
    ON sync_log (job_name);

CREATE TABLE IF NOT EXISTS sync_dead_letter (
    id                   BIGSERIAL PRIMARY KEY,
    job_name             VARCHAR(100) NOT NULL,
    source_id            VARCHAR(100) NULL,
    phase                VARCHAR(100) NOT NULL,
    payload              JSONB NOT NULL,
    error_message        TEXT NOT NULL,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    status               VARCHAR(50) NOT NULL DEFAULT 'Pending',
    retry_count          INT NOT NULL DEFAULT 0,
    last_retried_at      TIMESTAMPTZ NULL,
    resolved_at          TIMESTAMPTZ NULL,
    replay_error_message TEXT NULL,
    CONSTRAINT chk_sync_dead_letter_status
        CHECK (status IN ('Pending', 'Retrying', 'Resolved', 'Failed', 'Ignored'))
);

CREATE INDEX IF NOT EXISTS idx_sync_dead_letter_job_name
    ON sync_dead_letter (job_name);