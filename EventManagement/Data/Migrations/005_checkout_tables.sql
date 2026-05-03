-- EventManagement: add checkout sessions for hosted payment flow.
-- Run this against event_management_db if upgrading an existing database.

CREATE TABLE IF NOT EXISTS checkout_sessions (
    id                  UUID PRIMARY KEY,
    invoice_external_id VARCHAR(100) NOT NULL UNIQUE,
    xendit_invoice_id   VARCHAR(100),
    xendit_invoice_url  TEXT,
    user_id             UUID NOT NULL,
    user_email          VARCHAR(255) NOT NULL,
    user_full_name      VARCHAR(255) NOT NULL,
    event_id            UUID NOT NULL REFERENCES events(id),
    ticket_type_id      UUID NOT NULL REFERENCES ticket_types(id),
    ticket_name         VARCHAR(255) NOT NULL,
    quantity            INT NOT NULL,
    unit_price          NUMERIC(12, 2) NOT NULL,
    total_amount        NUMERIC(12, 2) NOT NULL,
    currency            VARCHAR(10) NOT NULL DEFAULT 'IDR',
    payment_provider    VARCHAR(50) NOT NULL DEFAULT 'xendit',
    payment_method      VARCHAR(100),
    status              SMALLINT NOT NULL DEFAULT 1,
    failure_reason      TEXT,
    expires_at          TIMESTAMP NOT NULL,
    paid_at             TIMESTAMP,
    created_on          TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by          VARCHAR(255) NOT NULL DEFAULT 'system',
    updated_on          TIMESTAMP,
    updated_by          VARCHAR(255)
);

CREATE INDEX IF NOT EXISTS idx_checkout_sessions_user_id ON checkout_sessions(user_id);
CREATE INDEX IF NOT EXISTS idx_checkout_sessions_ticket_type_status ON checkout_sessions(ticket_type_id, status);
CREATE INDEX IF NOT EXISTS idx_checkout_sessions_expires_at_status ON checkout_sessions(expires_at, status);