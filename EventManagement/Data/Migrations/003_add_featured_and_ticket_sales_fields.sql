-- EventManagement: add featured flag and ticket sales fields.
-- Run this against event_management_db if upgrading an existing database.

ALTER TABLE events
    ADD COLUMN IF NOT EXISTS is_featured BOOLEAN NOT NULL DEFAULT FALSE;

ALTER TABLE ticket_types
    ADD COLUMN IF NOT EXISTS currency VARCHAR(10) NOT NULL DEFAULT 'IDR',
    ADD COLUMN IF NOT EXISTS sale_start_date TIMESTAMP,
    ADD COLUMN IF NOT EXISTS sale_end_date TIMESTAMP;