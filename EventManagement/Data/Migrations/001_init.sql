-- EventManagement service initial migration
-- Run this script against the event_management_db database

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS events (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organizer_id UUID NOT NULL,
    title       VARCHAR(255) NOT NULL,
    description TEXT NOT NULL DEFAULT '',
    category    VARCHAR(100) NOT NULL DEFAULT '',
    location    VARCHAR(500) NOT NULL DEFAULT '',
    address     VARCHAR(500) NOT NULL DEFAULT '',
    timezone    VARCHAR(100) NOT NULL DEFAULT 'UTC',
    poster_image TEXT NOT NULL DEFAULT '',
    banner_image TEXT NOT NULL DEFAULT '',
    is_featured BOOLEAN NOT NULL DEFAULT FALSE,
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
    currency        VARCHAR(10) NOT NULL DEFAULT 'IDR',
    total_seats     INT NOT NULL DEFAULT 0,
    available_seats INT NOT NULL DEFAULT 0,
    sale_start_date TIMESTAMP,
    sale_end_date   TIMESTAMP,
    status          SMALLINT NOT NULL DEFAULT 1,
    is_deleted      BOOLEAN NOT NULL DEFAULT FALSE,
    created_on      TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by      VARCHAR(255) NOT NULL DEFAULT 'system',
    updated_on      TIMESTAMP,
    updated_by      VARCHAR(255)
);
