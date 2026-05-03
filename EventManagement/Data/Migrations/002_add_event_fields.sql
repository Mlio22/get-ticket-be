-- EventManagement: add category, address, timezone, poster_image, banner_image to events
-- Run this against event_management_db if upgrading an existing database.

ALTER TABLE events
    ADD COLUMN IF NOT EXISTS category     VARCHAR(100) NOT NULL DEFAULT '',
    ADD COLUMN IF NOT EXISTS address      VARCHAR(500) NOT NULL DEFAULT '',
    ADD COLUMN IF NOT EXISTS timezone     VARCHAR(100) NOT NULL DEFAULT 'UTC',
    ADD COLUMN IF NOT EXISTS poster_image TEXT        NOT NULL DEFAULT '',
    ADD COLUMN IF NOT EXISTS banner_image TEXT        NOT NULL DEFAULT '';
