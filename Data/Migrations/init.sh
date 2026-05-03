#!/bin/bash
set -e

# Create both service databases
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" <<-EOSQL
    CREATE DATABASE auth_db;
    CREATE DATABASE event_management_db;
EOSQL

# Run Auth service migrations
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "auth_db" \
    -f /auth-migrations/001_init.sql

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "auth_db" \
    -f /auth-migrations/002_seed_dummy_data.sql

# Run EventManagement service migrations
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "event_management_db" \
    -f /eventmanagement-migrations/001_init.sql

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "event_management_db" \
    -f /eventmanagement-migrations/002_add_event_fields.sql

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "event_management_db" \
    -f /eventmanagement-migrations/003_add_featured_and_ticket_sales_fields.sql

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "event_management_db" \
    -f /eventmanagement-migrations/004_seed_dummy_data.sql

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "event_management_db" \
    -f /eventmanagement-migrations/005_checkout_tables.sql

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "event_management_db" \
    -f /eventmanagement-migrations/006_checkout_session_items.sql
