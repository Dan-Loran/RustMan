CREATE TABLE instances (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    slug TEXT NOT NULL UNIQUE,
    hostname TEXT NOT NULL,
    description TEXT NULL,

    map_size INTEGER NOT NULL,
    map_seed INTEGER NOT NULL,

    max_players INTEGER NOT NULL,

    game_port INTEGER NOT NULL UNIQUE,
    query_port INTEGER NOT NULL UNIQUE,
    rcon_port INTEGER NOT NULL UNIQUE,
    rcon_password TEXT NOT NULL,

    auto_start INTEGER NOT NULL DEFAULT 1,

    created_utc TEXT NOT NULL,
    updated_utc TEXT NOT NULL,

    last_wipe_utc TEXT NULL,
    last_blueprint_wipe_utc TEXT NULL, is_enabled INTEGER NOT NULL DEFAULT 1
    CHECK (is_enabled IN (0, 1)),

    CONSTRAINT ck_instances_map_size
        CHECK (map_size >= 1000 AND map_size <= 6000),

    CONSTRAINT ck_instances_max_players
        CHECK (max_players >= 1),

    CONSTRAINT ck_instances_auto_start
        CHECK (auto_start IN (0, 1))
);
CREATE INDEX ix_instances_auto_start
    ON instances (auto_start);
CREATE TABLE server_property_definitions (
    key TEXT PRIMARY KEY,
    prefix TEXT NOT NULL,
    display_name TEXT NOT NULL,
    description TEXT NULL,
    value_type TEXT NOT NULL,
    default_value TEXT NULL,
    min_value REAL NULL,
    max_value REAL NULL,
    is_advanced INTEGER NOT NULL DEFAULT 1, is_common_advanced INTEGER NOT NULL DEFAULT 0
    CHECK (is_common_advanced IN (0, 1)),

    CONSTRAINT ck_server_property_definitions_is_advanced
        CHECK (is_advanced IN (0, 1)),

    CONSTRAINT ck_server_property_definitions_value_type
        CHECK (value_type IN ('string', 'integer', 'decimal', 'boolean'))
);
CREATE INDEX ix_server_property_definitions_prefix
    ON server_property_definitions (prefix, display_name);
CREATE TABLE server_property_allowed_values (
    property_key TEXT NOT NULL,
    value TEXT NOT NULL,
    display_name TEXT NOT NULL,

    PRIMARY KEY (property_key, value),

    FOREIGN KEY (property_key)
        REFERENCES server_property_definitions (key)
        ON DELETE CASCADE
);
CREATE INDEX ix_server_property_allowed_values_property_key
    ON server_property_allowed_values (property_key, display_name);
CREATE TABLE instance_property_values (
    instance_id TEXT NOT NULL,
    property_key TEXT NOT NULL,
    value TEXT NOT NULL,

    PRIMARY KEY (instance_id, property_key),

    FOREIGN KEY (instance_id)
        REFERENCES instances (id)
        ON DELETE CASCADE,

    FOREIGN KEY (property_key)
        REFERENCES server_property_definitions (key)
        ON DELETE CASCADE
);
CREATE INDEX ix_instance_property_values_instance_id
    ON instance_property_values (instance_id);
CREATE INDEX ix_instance_property_values_property_key
    ON instance_property_values (property_key);
CREATE TABLE command_definitions (
    key TEXT PRIMARY KEY,
    display_name TEXT NOT NULL,
    description TEXT NULL,
    category TEXT NOT NULL,
    requires_confirmation INTEGER NOT NULL DEFAULT 0,
    is_common INTEGER NOT NULL DEFAULT 0,
    is_active INTEGER NOT NULL DEFAULT 1,

    CONSTRAINT ck_command_definitions_requires_confirmation
        CHECK (requires_confirmation IN (0, 1)),

    CONSTRAINT ck_command_definitions_is_common
        CHECK (is_common IN (0, 1)),

    CONSTRAINT ck_command_definitions_is_active
        CHECK (is_active IN (0, 1))
);
CREATE INDEX ix_command_definitions_category
    ON command_definitions (category, display_name);
CREATE INDEX ix_command_definitions_is_common
    ON command_definitions (is_common);
CREATE INDEX ix_command_definitions_is_active
    ON command_definitions (is_active);
CREATE TABLE command_parameters (
    command_key TEXT NOT NULL,
    name TEXT NOT NULL,
    display_name TEXT NOT NULL,
    description TEXT NULL,
    value_type TEXT NOT NULL,
    is_required INTEGER NOT NULL DEFAULT 1,

    PRIMARY KEY (command_key, name),

    FOREIGN KEY (command_key)
        REFERENCES command_definitions (key)
        ON DELETE CASCADE,

    CONSTRAINT ck_command_parameters_value_type
        CHECK (value_type IN ('string', 'integer', 'decimal', 'boolean')),

    CONSTRAINT ck_command_parameters_is_required
        CHECK (is_required IN (0, 1))
);
CREATE INDEX ix_command_parameters_command_key
    ON command_parameters (command_key);
CREATE INDEX ix_instances_is_enabled
    ON instances (is_enabled);
