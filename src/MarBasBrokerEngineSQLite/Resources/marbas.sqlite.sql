CREATE TABLE mb_schema_opts (
  name  varchar(255) NOT NULL,
  val   varchar(255),
  /* Keys */
  CONSTRAINT mb_pk_schema_opts
    PRIMARY KEY (name, val)
);

CREATE TABLE mb_file (
  base_id    guid NOT NULL,
  mime_type  varchar(255) NOT NULL DEFAULT 'application/octet-stream',
  size       bigint NOT NULL DEFAULT -1,
  content    blob,
  /* Keys */
  PRIMARY KEY (base_id),
  /* Foreign keys */
  CONSTRAINT mb_fk_file_base
    FOREIGN KEY (base_id)
    REFERENCES mb_grain_base(id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE INDEX mb_idx_file_mime
  ON mb_file
  (mime_type);

CREATE TRIGGER mb_tg_file_update_mtime
  AFTER UPDATE
  ON mb_file
  WHEN NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = new.base_id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE id = new.base_id;
END;

CREATE TABLE mb_grain_base (
  id          guid NOT NULL,
  parent_id   guid,
  typedef_id  guid,
  name        varchar(255) NOT NULL,
  ctime       timestamp,
  mtime       timestamp,
  owner       varchar(255) NOT NULL DEFAULT 'system@marbas',
  revision    integer NOT NULL DEFAULT 1,
  sort_key    char(50),
  xattrs      varchar(512),
  custom_flag integer DEFAULT 0,
  child_count integer DEFAULT 0,
  /* Keys */
  PRIMARY KEY (id),
  CONSTRAINT mb_idx_grain_base_level_name
    UNIQUE (parent_id, name),
  /* Checks */
  CONSTRAINT mb_grain_base_revision
    CHECK (revision >= 1),
  /* Foreign keys */
  CONSTRAINT mb_fk_grain_base_typedef
    FOREIGN KEY (typedef_id)
    REFERENCES mb_typedef(base_id)
    ON DELETE RESTRICT
    ON UPDATE CASCADE, 
  CONSTRAINT mb_fk_grain_base_parent
    FOREIGN KEY (parent_id)
    REFERENCES mb_grain_base(id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE INDEX mb_idx_grain_base_owner
  ON mb_grain_base
  (owner);

CREATE INDEX mb_idx_grain_base_ctime
  ON mb_grain_base
  (ctime);

CREATE INDEX mb_idx_grain_base_mtime
  ON mb_grain_base
  (mtime);

CREATE INDEX mb_idx_grain_base_name
  ON mb_grain_base
  (name);

CREATE INDEX mb_idx_grain_base_revision
  ON mb_grain_base
  (revision);

CREATE INDEX mb_idx_grain_base_sort
  ON mb_grain_base
  (sort_key);

CREATE INDEX mb_idx_grain_base_typedef
  ON mb_grain_base
  (typedef_id);

CREATE INDEX mb_idx_grain_base_xattrs
  ON mb_grain_base
  (xattrs);

CREATE INDEX mb_idx_grain_base_flag
  ON mb_grain_base
  (custom_flag);

CREATE TRIGGER mb_tg_grain_base_ctime
  AFTER INSERT
  ON mb_grain_base
  WHEN new.ctime IS NULL
BEGIN
  UPDATE mb_grain_base SET ctime = DATETIME('now')
  WHERE rowid = new.rowid;
END;

CREATE TRIGGER mb_tg_grain_base_mtime
  AFTER UPDATE
  ON mb_grain_base
  WHEN (new.mtime IS NULL OR new.mtime = old.mtime) AND NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = new.id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE rowid = new.rowid;
END;

CREATE TRIGGER mb_tg_grain_base_insert_parent_update
  AFTER INSERT
  ON mb_grain_base
BEGIN
  UPDATE mb_grain_base SET child_count = (SELECT COUNT(id) FROM mb_grain_base WHERE parent_id = new.parent_id) WHERE id = new.parent_id;
END;

CREATE TRIGGER mb_tg_grain_base_delete_parent_update
  AFTER DELETE
  ON mb_grain_base
BEGIN
  UPDATE mb_grain_base SET child_count = (SELECT COUNT(id) FROM mb_grain_base WHERE parent_id = old.parent_id) WHERE id = old.parent_id;
END;

CREATE TRIGGER mb_tg_grain_base_update_parent_update
  AFTER UPDATE OF parent_id
  ON mb_grain_base
  WHEN new.parent_id <> old.parent_id
BEGIN
  UPDATE mb_grain_base SET child_count = (SELECT COUNT(id) FROM mb_grain_base WHERE parent_id = old.parent_id) WHERE id = old.parent_id;
  UPDATE mb_grain_base SET child_count = (SELECT COUNT(id) FROM mb_grain_base WHERE parent_id = new.parent_id) WHERE id = new.parent_id;
END;

CREATE TRIGGER mb_tg_grain_typedef_defaults_check
  BEFORE INSERT
  ON mb_grain_base
  WHEN new.parent_id = new.typedef_id
BEGIN
  SELECT
  RAISE (IGNORE)
  WHERE EXISTS (SELECT 1 FROM mb_grain_base WHERE parent_id = new.parent_id AND typedef_id = new.typedef_id);
END;

CREATE TRIGGER mb_tg_grain_insert_typedef_defaults_name
  AFTER INSERT
  ON mb_grain_base
  WHEN new.parent_id = new.typedef_id
BEGIN
  UPDATE mb_grain_base SET name = '__defaults__' WHERE rowid = new.rowid;
END;

CREATE TRIGGER mb_tg_grain_update_typedef_defaults_name
  AFTER UPDATE
  ON mb_grain_base
  WHEN new.parent_id = new.typedef_id AND new.name <> '__defaults__'
BEGIN
  UPDATE mb_grain_base SET name = '__defaults__'
  WHERE rowid = new.rowid AND name <> '__defaults__';
END;

CREATE TABLE mb_grain_control (
  grain_id  guid NOT NULL PRIMARY KEY,
  flag      integer,
  /* Foreign keys */
  CONSTRAINT mb_fk_grain_control
    FOREIGN KEY (grain_id)
    REFERENCES mb_grain_base(id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE INDEX mb_idx_grain_control_flag
  ON mb_grain_control
  (flag);

CREATE TABLE mb_grain_history (
  grain_id  guid NOT NULL,
  revision  integer NOT NULL DEFAULT 1,
  author    varchar(255) NOT NULL DEFAULT 'system@marbas',
  comment   varchar(255),
  ctime     timestamp NOT NULL,
  /* Keys */
  CONSTRAINT mb_pk_grain_history
    PRIMARY KEY (grain_id, revision),
  /* Checks */
  CONSTRAINT mb_grain_history_revision
    CHECK (revision >= 1),
  /* Foreign keys */
  CONSTRAINT mb_fk_grain_history_base
    FOREIGN KEY (grain_id)
    REFERENCES mb_grain_base(id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE INDEX mb_idx_grain_history_author
  ON mb_grain_history
  (author);

CREATE INDEX mb_idx_grain_history_ctime
  ON mb_grain_history
  (ctime);

CREATE TABLE mb_grain_label (
  grain_id   guid NOT NULL,
  lang_code  char(24) NOT NULL,
  label      varchar(512) NOT NULL,
  /* Keys */
  CONSTRAINT mb_pk_grain_label
    PRIMARY KEY (grain_id, lang_code),
  /* Foreign keys */
  CONSTRAINT mb_fk_label_grain
    FOREIGN KEY (grain_id)
    REFERENCES mb_grain_base(id)
    ON DELETE CASCADE
    ON UPDATE CASCADE, 
  CONSTRAINT mb_fk_label_lang
    FOREIGN KEY (lang_code)
    REFERENCES mb_lang(iso_code)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE INDEX mb_idx_grain_label_label
  ON mb_grain_label
  (label);
  
CREATE INDEX mb_fki_grain_label_lang
  ON mb_grain_label
  (lang_code);
  
CREATE INDEX mb_fki_grain_label_grain
  ON mb_grain_label
  (grain_id);

CREATE TRIGGER mb_tg_grain_label_insert_grain_name
  BEFORE INSERT
  ON mb_grain_label
BEGIN
  SELECT
  RAISE (IGNORE)
  WHERE EXISTS (SELECT 1 FROM mb_grain_base WHERE id = new.grain_id AND name = new.label);
END;

CREATE TRIGGER mb_tg_grain_label_update_grain_name
  BEFORE UPDATE
  ON mb_grain_label
  WHEN new.label = (SELECT name FROM mb_grain_base WHERE id = new.grain_id)
BEGIN
  DELETE FROM mb_grain_label
  WHERE grain_id = new.grain_id AND lang_code = new.lang_code;
  SELECT
  RAISE (IGNORE);
END;

CREATE TRIGGER mb_tg_grain_label_delete_mtime
  AFTER DELETE
  ON mb_grain_label
  WHEN NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = old.grain_id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE id = old.grain_id;
END;

CREATE TRIGGER mb_tg_grain_label_insert_mtime
  AFTER INSERT
  ON mb_grain_label
  WHEN NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = new.grain_id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE id = new.grain_id;
END;

CREATE TRIGGER mb_tg_grain_label_update_mtime
  AFTER UPDATE
  ON mb_grain_label
  WHEN NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = new.grain_id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE id = new.grain_id;
END;

CREATE TABLE mb_lang (
  iso_code      char(24) NOT NULL,
  label         varchar(255) NOT NULL,
  label_native  varchar(255),
  /* Keys */
  PRIMARY KEY (iso_code), 
  CONSTRAINT mb_idx_lang_label
    UNIQUE (label), 
  CONSTRAINT mb_idx_lang_label_native
    UNIQUE (label_native)
);

CREATE TABLE mb_propdef (
  base_id           guid NOT NULL,
  value_type        char(255) NOT NULL DEFAULT 'text',
  cardinality_min   integer NOT NULL DEFAULT 1,
  cardinality_max   integer NOT NULL DEFAULT 1,
  value_constraint  guid,
  constraint_params varchar(1024),
  versionable       boolean NOT NULL DEFAULT TRUE,
  localizable       boolean NOT NULL DEFAULT TRUE,
  /* Keys */
  PRIMARY KEY (base_id),
  /* Checks */
  CONSTRAINT mb_prodef_cardinality_min
    CHECK (cardinality_min >= 0),
  CONSTRAINT mb_prodef_cardinality_max
    CHECK (cardinality_max = -1 OR cardinality_max >= 1),
  /* Foreign keys */
  CONSTRAINT mb_fk_propdef_value_type
    FOREIGN KEY (value_type)
    REFERENCES mb_value_type(name)
    ON DELETE RESTRICT
    ON UPDATE CASCADE, 
  CONSTRAINT mb_fk_propdef_base
    FOREIGN KEY (base_id)
    REFERENCES mb_grain_base(id)
    ON DELETE CASCADE
    ON UPDATE CASCADE, 
  CONSTRAINT mb_fk_propdef_value_constr
    FOREIGN KEY (value_constraint)
    REFERENCES mb_grain_base(id)
    ON DELETE SET NULL
    ON UPDATE CASCADE
);

CREATE INDEX mb_idx_propdef_localizable
  ON mb_propdef
  (localizable);

CREATE INDEX mb_idx_propdef_value_type
  ON mb_propdef
  (value_type);

CREATE INDEX mb_idx_propdef_versionable
  ON mb_propdef
  (versionable);

CREATE TRIGGER mb_tg_propdef_update_mtime
  AFTER UPDATE
  ON mb_propdef
  WHEN NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = new.base_id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE id = new.base_id;
END;

CREATE TABLE mb_grain_trait (
  id           guid NOT NULL,
  grain_id     guid NOT NULL,
  propdef_id   guid NOT NULL,
  lang_code    char(24),
  revision     integer NOT NULL DEFAULT 1,
  ord          integer NOT NULL DEFAULT 0,
  val_boolean  boolean DEFAULT 0,
  val_text     varchar(512),
  val_number   real,
  val_datetime  datetime,
  val_memo     text,
  val_guid     guid,
  /* Keys */
  PRIMARY KEY (id),
  /* Checks */
  CONSTRAINT mb_grain_trait_revision
    CHECK (revision >= 0),
  CONSTRAINT mb_grain_trait_ord
    CHECK (ord >= 0),
  /* Foreign keys */
  CONSTRAINT mb_fk_grain_trait_lang
    FOREIGN KEY (lang_code)
    REFERENCES mb_lang(iso_code)
    ON DELETE CASCADE
    ON UPDATE CASCADE, 
  CONSTRAINT mb_fk_grain_trait_propdef
    FOREIGN KEY (propdef_id)
    REFERENCES mb_propdef(base_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE, 
  CONSTRAINT mb_fk_grain_trait_grain
    FOREIGN KEY (grain_id)
    REFERENCES mb_grain_base(id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE INDEX mb_idx_grain_trait_lang
  ON mb_grain_trait
  (lang_code);

CREATE UNIQUE INDEX mb_idx_grain_trait_propdef
  ON mb_grain_trait
  (grain_id, propdef_id, lang_code, ord, revision);

CREATE TRIGGER mb_tg_grain_trait_delete_mtime
  AFTER DELETE
  ON mb_grain_trait
  WHEN NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = old.grain_id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE id = old.grain_id;
END;

CREATE TRIGGER mb_tg_grain_trait_insert_mtime
  AFTER INSERT
  ON mb_grain_trait
  WHEN NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = new.grain_id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE id = new.grain_id;
END;

CREATE TRIGGER mb_tg_grain_trait_update_mtime
  AFTER UPDATE
  ON mb_grain_trait
  WHEN NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = new.grain_id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE id = new.grain_id;
END;

CREATE TRIGGER mb_tg_grain_trait_update_l10n
  AFTER UPDATE
  ON mb_grain_trait
BEGIN
  UPDATE mb_grain_trait
  SET lang_code = 'en'
  WHERE id = new.id AND lang_code IS NULL
    AND (SELECT localizable FROM mb_propdef WHERE base_id = new.propdef_id) = TRUE;

  UPDATE mb_grain_trait
  SET lang_code = NULL
  WHERE id = new.id AND lang_code IS NOT NULL
    AND (SELECT localizable FROM mb_propdef WHERE base_id = new.propdef_id) <> TRUE;
END;

CREATE TRIGGER mb_tg_grain_trait_insert_l10n
  AFTER INSERT
  ON mb_grain_trait
BEGIN
  UPDATE mb_grain_trait
  SET lang_code = 'en'
  WHERE id = new.id AND lang_code IS NULL
    AND (SELECT localizable FROM mb_propdef WHERE base_id = new.propdef_id) = TRUE;

  UPDATE mb_grain_trait
  SET lang_code = NULL
  WHERE id = new.id AND lang_code IS NOT NULL
    AND (SELECT localizable FROM mb_propdef WHERE base_id = new.propdef_id) <> TRUE;
END;

CREATE TABLE mb_typedef (
  base_id  guid NOT NULL,
  impl     varchar(1024),
  /* Keys */
  PRIMARY KEY (base_id),
  /* Foreign keys */
  CONSTRAINT mb_fk_typedef_base
    FOREIGN KEY (base_id)
    REFERENCES mb_grain_base(id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE TRIGGER mb_tg_typedef_update_mtime
  AFTER UPDATE
  ON mb_typedef
  WHEN NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = new.base_id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE id = new.base_id;
END;

CREATE TABLE mb_typedef_mixin (
  base_typedef_id     guid NOT NULL,
  derived_typedef_id  guid NOT NULL,
  /* Keys */
  CONSTRAINT mb_pk_typedef_inheritance
    PRIMARY KEY (base_typedef_id, derived_typedef_id),
  /* Foreign keys */
  CONSTRAINT mb_fk_typedef_mixin_derived
    FOREIGN KEY (derived_typedef_id)
    REFERENCES mb_typedef(base_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE, 
  CONSTRAINT mb_fk_typedef_mixin_base
    FOREIGN KEY (base_typedef_id)
    REFERENCES mb_typedef(base_id)
    ON DELETE RESTRICT
    ON UPDATE CASCADE
);

CREATE TRIGGER mb_tg_typedef_mixin_delete_mtime
  AFTER DELETE
  ON mb_typedef_mixin
  WHEN NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = old.derived_typedef_id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE id = old.derived_typedef_id;
END;

CREATE TRIGGER mb_tg_typedef_mixin_insert_mtime
  AFTER INSERT
  ON mb_typedef_mixin
  WHEN NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = new.derived_typedef_id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE id = new.derived_typedef_id;
END;

CREATE TABLE mb_value_type (
  name  char(255) NOT NULL,
  /* Keys */
  PRIMARY KEY (name)
);

CREATE TABLE mb_role (
  id    guid NOT NULL,
  name  varchar(255) NOT NULL,
  entitlement  integer NOT NULL DEFAULT 0,
  /* Keys */
  PRIMARY KEY (id)
);

CREATE INDEX mb_idx_role_entitlement
  ON mb_role
  (entitlement);

CREATE UNIQUE INDEX mb_idx_role_name
  ON mb_role
  (name);

CREATE TABLE mb_grain_acl (
  role_id           guid NOT NULL,
  grain_id          guid NOT NULL DEFAULT '00000000-0000-1000-a000-000000000000',
  inherit           boolean NOT NULL DEFAULT 1,
  permission_mask   integer NOT NULL DEFAULT 1,
  restriction_mask  integer NOT NULL DEFAULT 0,
  /* Keys */
  CONSTRAINT mb_pk_grain_acl
    PRIMARY KEY (role_id, grain_id),
  /* Foreign keys */
  CONSTRAINT mb_fk_grain_acl_grain
    FOREIGN KEY (grain_id)
    REFERENCES mb_grain_base(id)
    ON DELETE CASCADE
    ON UPDATE CASCADE, 
  CONSTRAINT mb_fk_grain_acl_role
    FOREIGN KEY (role_id)
    REFERENCES mb_role(id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE INDEX mb_fki_grain_acl_grain
  ON mb_grain_acl
  (grain_id);

CREATE INDEX mb_fki_grain_acl_role
  ON mb_grain_acl
  (role_id);

CREATE INDEX mb_idx_grain_acl_inherit
  ON mb_grain_acl
  (inherit);

CREATE INDEX mb_idx_grain_acl_permissions
  ON mb_grain_acl
  (permission_mask);

CREATE INDEX mb_idx_grain_acl_restrictions
  ON mb_grain_acl
  (restriction_mask);

CREATE TRIGGER mb_tg_grain_acl_delete_mtime
  AFTER DELETE
  ON mb_grain_acl
  WHEN NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = old.grain_id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE id = old.grain_id;
END;

CREATE TRIGGER mb_tg_grain_acl_insert_mtime
  AFTER INSERT
  ON mb_grain_acl
  WHEN NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = new.grain_id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE id = new.grain_id;
END;

CREATE TRIGGER mb_tg_grain_acl_update_mtime
  AFTER UPDATE
  ON mb_grain_acl
  WHEN NOT EXISTS(SELECT 1 FROM mb_grain_control WHERE grain_id = new.grain_id AND (0x1 & flag) > 0)
BEGIN
  UPDATE mb_grain_base SET mtime = DATETIME('now')
  WHERE id = new.grain_id;
END;

/* Views */
CREATE VIEW mb_grain_ancestor
AS
WITH RECURSIVE cte_ancestor(id, name, parent_id, distance, start) AS (
    SELECT id, name, parent_id, 0, id as start
        FROM mb_grain_base
    UNION ALL
    SELECT y.id, y.name, y.parent_id, a.distance + 1, a.start
        FROM mb_grain_base AS y
        JOIN cte_ancestor a ON a.parent_id = y.id
)
SELECT *
    FROM cte_ancestor
    ORDER BY start, distance;

CREATE VIEW mb_grain_with_path
AS
WITH RECURSIVE descendant(id, path, id_path) AS (
    SELECT id, name, cast(id AS text) AS id_path
        FROM mb_grain_base
        WHERE parent_id IS NULL
    UNION ALL
    SELECT c.id, r.path  || '/' || c.name, r.id_path || '/' || cast(c.id AS text) AS id_path
        FROM mb_grain_base AS c
        INNER JOIN descendant AS r ON (c.parent_id = r.id)
)
SELECT a.*, b.name AS type_name, b.xattrs AS type_xattrs, t.impl AS type_impl, descendant.path, descendant.id_path
    FROM mb_grain_base AS a
JOIN descendant
    ON a.id = descendant.id
LEFT JOIN mb_grain_base b
    ON b.id = a.typedef_id
LEFT JOIN mb_typedef AS t
    ON t.base_id = a.typedef_id;

CREATE VIEW mb_grain_trait_with_meta
AS
SELECT p.*, a.path, b.name, d.value_type, d.cardinality_min, d.cardinality_max, d.value_constraint
    FROM mb_grain_trait AS p
LEFT JOIN mb_grain_with_path AS a
    ON a.id = p.grain_id
LEFT JOIN "mb_grain_base" AS b
    ON b.id = p.propdef_id
LEFT JOIN mb_propdef AS d
    ON d.base_id = p.propdef_id;

CREATE VIEW mb_uuid
AS
SELECT lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || substr(lower(hex(randomblob(2))),2) || '-' || substr('89ab',abs(random()) % 4 + 1, 1) || substr(lower(hex(randomblob(2))),2) || '-' || lower(hex(randomblob(6))) AS result;

CREATE VIEW mb_typedef_mixin_ancestor
AS
WITH RECURSIVE cte_base(derived_typedef_id, base_typedef_id, distance, start) AS (
    SELECT derived_typedef_id, base_typedef_id, 0, derived_typedef_id AS start
        FROM mb_typedef_mixin
    UNION ALL
    SELECT b.derived_typedef_id, b.base_typedef_id, a.distance + 1, a.start
        FROM mb_typedef_mixin AS b
        JOIN cte_base AS a ON a.base_typedef_id = b.derived_typedef_id
)
SELECT * FROM cte_base
ORDER by start, distance;

CREATE VIEW mb_typedef_mixin_descendant
AS
WITH RECURSIVE cte_derived(derived_typedef_id, base_typedef_id, distance, start) AS (
    SELECT derived_typedef_id, base_typedef_id, 0, base_typedef_id AS start
        FROM mb_typedef_mixin
    UNION ALL
    SELECT b.derived_typedef_id, b.base_typedef_id, a.distance + 1, a.start
        FROM mb_typedef_mixin AS b
        JOIN cte_derived AS a ON a.derived_typedef_id = b.base_typedef_id
)
SELECT * FROM cte_derived
ORDER by start, distance;

CREATE VIEW mb_grain_acl_effective
AS
WITH RECURSIVE cte_ancestor(id, parent_id, distance, start) AS (
    SELECT g.id, g.parent_id, 0, g.id as start
        FROM mb_grain_base AS g
    UNION ALL
    SELECT b.id, b.parent_id, a.distance + 1, a.start
        FROM mb_grain_base AS b
        JOIN cte_ancestor AS a ON a.parent_id = b.id
)
SELECT * FROM (
SELECT c.start AS grain_id, p.role_id, p.permission_mask, p.restriction_mask, p.inherit, c.distance AS acl_type, c.id AS acl_source
    FROM mb_grain_acl AS p
LEFT JOIN cte_ancestor AS c 
ON c.id = p.grain_id
    WHERE p.grain_id <> '00000000-0000-1000-a000-000000000000'
    AND (p.inherit OR p.grain_id = c.start)
    ORDER BY start, distance
)
UNION ALL
SELECT d.grain_id, d.role_id, d.permission_mask, d.restriction_mask, d.inherit, 0xFFFFFFF0 AS acl_type, d.grain_id AS acl_source
FROM mb_grain_acl AS d WHERE grain_id = '00000000-0000-1000-a000-000000000000'
UNION ALL
SELECT NULL AS grain_id, NULL AS role_id, 0 AS pemission_mask, -1 AS restriction_mask, TRUE AS inherit, 0xFFFFFFF1 AS acl_type, NULL AS acl_source;

CREATE VIEW mb_typedef_as_grain_with_path
AS
SELECT t.impl, d.id AS defaults_id, g.* FROM mb_typedef AS t
LEFT JOIN mb_grain_with_path AS g
ON g.id = t.base_id
LEFT JOIN mb_grain_base AS d
ON d.parent_id = t.base_id AND d.typedef_id = t.base_id;

CREATE VIEW mb_propdef_as_grain_with_path
AS
SELECT p.*, g.*, b.name AS parent_name, b.sort_key AS parent_sort_key
FROM mb_propdef AS p
LEFT JOIN mb_grain_with_path AS g
ON g.id = p.base_id
LEFT JOIN mb_grain_base AS b
ON b.id = g.parent_id;