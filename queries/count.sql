-- SQLite
SELECT * FROM etablissements
        WHERE NOM != ""
        AND NOM IS NOT NULL
 LIMIT 10