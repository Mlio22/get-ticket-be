-- Auth service seed data for local development
-- Inserts deterministic users with a known bcrypt hash for password: password

INSERT INTO users (id, email, password_hash, full_name, role, is_active, is_deleted, created_on, created_by)
VALUES
    ('11111111-1111-1111-1111-111111111111', 'organizer1@getticket.local', '$2a$10$7EqJtq98hPqEX7fNZaFWoOhiX6X6fQ6Qx7R9Ejo4kKDAdAm8YGtS.', 'Adit Organizer', 2, TRUE, FALSE, NOW() - INTERVAL '30 days', 'seed'),
    ('22222222-2222-2222-2222-222222222222', 'organizer2@getticket.local', '$2a$10$7EqJtq98hPqEX7fNZaFWoOhiX6X6fQ6Qx7R9Ejo4kKDAdAm8YGtS.', 'Nia Organizer', 2, TRUE, FALSE, NOW() - INTERVAL '28 days', 'seed'),
    ('33333333-3333-3333-3333-333333333333', 'customer1@getticket.local',  '$2a$10$7EqJtq98hPqEX7fNZaFWoOhiX6X6fQ6Qx7R9Ejo4kKDAdAm8YGtS.', 'Raka Customer', 1, TRUE, FALSE, NOW() - INTERVAL '20 days', 'seed'),
    ('44444444-4444-4444-4444-444444444444', 'customer2@getticket.local',  '$2a$10$7EqJtq98hPqEX7fNZaFWoOhiX6X6fQ6Qx7R9Ejo4kKDAdAm8YGtS.', 'Putri Customer', 1, TRUE, FALSE, NOW() - INTERVAL '18 days', 'seed'),
    ('55555555-5555-5555-5555-555555555555', 'customer3@getticket.local',  '$2a$10$7EqJtq98hPqEX7fNZaFWoOhiX6X6fQ6Qx7R9Ejo4kKDAdAm8YGtS.', 'Bagas Customer', 1, TRUE, FALSE, NOW() - INTERVAL '15 days', 'seed'),
    ('66666666-6666-6666-6666-666666666666', 'customer4@getticket.local',  '$2a$10$7EqJtq98hPqEX7fNZaFWoOhiX6X6fQ6Qx7R9Ejo4kKDAdAm8YGtS.', 'Salsa Customer', 1, TRUE, FALSE, NOW() - INTERVAL '12 days', 'seed')
ON CONFLICT (email) DO NOTHING;
