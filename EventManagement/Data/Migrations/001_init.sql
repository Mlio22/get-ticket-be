-- EventManagement service initial migration
-- Run this script against the event_management_db database

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS events (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organizer_id UUID NOT NULL,
    title       VARCHAR(255) NOT NULL,
    description TEXT NOT NULL DEFAULT '',
    location    VARCHAR(500) NOT NULL DEFAULT '',
    start_date  TIMESTAMP NOT NULL,
    end_date    TIMESTAMP NOT NULL,
    status      SMALLINT NOT NULL DEFAULT 1,
    is_deleted  BOOLEAN NOT NULL DEFAULT FALSE,
    created_on  TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by  VARCHAR(255) NOT NULL DEFAULT 'system',
    updated_on  TIMESTAMP,
    updated_by  VARCHAR(255)
);

CREATE TABLE IF NOT EXISTS ticket_types (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    event_id        UUID NOT NULL REFERENCES events(id),
    name            VARCHAR(255) NOT NULL,
    description     TEXT NOT NULL DEFAULT '',
    price           NUMERIC(12, 2) NOT NULL DEFAULT 0,
    total_seats     INT NOT NULL DEFAULT 0,
    available_seats INT NOT NULL DEFAULT 0,
    status          SMALLINT NOT NULL DEFAULT 1,
    is_deleted      BOOLEAN NOT NULL DEFAULT FALSE,
    created_on      TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by      VARCHAR(255) NOT NULL DEFAULT 'system',
    updated_on      TIMESTAMP,
    updated_by      VARCHAR(255)
);
