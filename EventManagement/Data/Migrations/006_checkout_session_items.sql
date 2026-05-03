-- EventManagement: support one checkout session with multiple ticket items.
-- Run this against event_management_db if upgrading an existing database.

ALTER TABLE checkout_sessions
    ALTER COLUMN ticket_type_id DROP NOT NULL,
    ALTER COLUMN ticket_name DROP NOT NULL,
    ALTER COLUMN unit_price DROP NOT NULL;

CREATE TABLE IF NOT EXISTS checkout_session_items (
    id          UUID PRIMARY KEY,
    checkout_id UUID NOT NULL REFERENCES checkout_sessions(id) ON DELETE CASCADE,
    ticket_type_id UUID NOT NULL REFERENCES ticket_types(id),
    ticket_name VARCHAR(255) NOT NULL,
    quantity    INT NOT NULL,
    unit_price  NUMERIC(12, 2) NOT NULL,
    line_total  NUMERIC(12, 2) NOT NULL,
    currency    VARCHAR(10) NOT NULL DEFAULT 'IDR',
    created_on  TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by  VARCHAR(255) NOT NULL DEFAULT 'system'
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_checkout_session_items_checkout_ticket
    ON checkout_session_items(checkout_id, ticket_type_id);

CREATE INDEX IF NOT EXISTS idx_checkout_session_items_checkout_id
    ON checkout_session_items(checkout_id);

CREATE INDEX IF NOT EXISTS idx_checkout_session_items_ticket_type_id
    ON checkout_session_items(ticket_type_id);