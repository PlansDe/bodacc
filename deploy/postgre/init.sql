CREATE USER populate WITH ENCRYPTED PASSWORD '88e359f4f79166de265d2a403e38e7d5';
CREATE USER api;

CREATE TABLE IF NOT EXISTS annonces (
    ID SERIAL,
    PARUTION CHAR(8), 
    NUMERO CHAR(4), 
    DATE DATE, 
    CODEPOSTAL VARCHAR(8), 
    VILLE TEXT, 
    NATURE TEXT, 
    RCS CHAR(12), 
    TYPE VARCHAR(12), 
    FORMEJURIDIQUE TEXT, 
    PREVIOUS_PARUTION CHAR(8),
    PREVIOUS_NUMERO CHAR(4)
);

CREATE TABLE IF NOT EXISTS geocodes (
    CODE CHAR(5), 
    NOM VARCHAR(64)
);

CREATE TABLE IF NOT EXISTS effectifs (
    CODE TEXT PRIMARY KEY NOT NULL, 
    NOM TEXT
);

CREATE TABLE IF NOT EXISTS codesnaf (
    CODE TEXT PRIMARY KEY NOT NULL, 
    LABEL TEXT
);

CREATE TABLE IF NOT EXISTS categoriesjuridiques (
    CODE TEXT PRIMARY KEY NOT NULL, 
    LABEL TEXT
);

CREATE TABLE IF NOT EXISTS uniteslegales (
    SIREN CHAR(9), 
    NOM TEXT, 
    EFFECTIFS VARCHAR(8), 
    ACTIVITE VARCHAR(8), 
    CATEGORIEJURIDIQUE VARCHAR(8), 
    NOMENCLATUREACTIVITE VARCHAR(8)
);

CREATE TABLE IF NOT EXISTS etablissements (
    SIRET CHAR(14),
    SIREN CHAR(9),
    ETATADMIN CHAR,
    EFFECTIFS VARCHAR(8),
    CP VARCHAR(8),
    VILLE TEXT,
    NOM TEXT,
    PAYS CHAR(5),
    VILLEETRANGER TEXT,
    ACTIVITE VARCHAR(8), 
    NOMENCLATUREACTIVITE VARCHAR(8)
);

REVOKE ALL
ON ALL TABLES IN SCHEMA public 
FROM PUBLIC;

GRANT SELECT, INSERT, DELETE
ON ALL TABLES IN SCHEMA public 
TO populate;

GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO populate;

GRANT SELECT
ON ALL TABLES IN SCHEMA public 
TO api;