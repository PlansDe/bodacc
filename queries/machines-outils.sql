-- SQLite

SELECT etablissements.SIREN,
       etablissements.SIRET,
       uniteslegales.NOM,
       CASE 
            WHEN etablissements.EFFECTIFS <> '' THEN etablissements.EFFECTIFS 
            WHEN uniteslegales.EFFECTIFS <> '' THEN uniteslegales.EFFECTIFS 
            ELSE "Inconnu"
        END EFFECTIF,
       etablissements.CP,
       etablissements.VILLE1, 
       (SELECT geocodes.NOM
            FROM geocodes 
            WHERE CODE = etablissements.PAYS1) PAYS
    FROM etablissements
       LEFT JOIN uniteslegales
       ON etablissements.SIREN = uniteslegales.SIREN
    WHERE etablissements.ACTIVITE = "28.41Z"
        AND etablissements.ETATADMIN = "A" -- entreprises actives uniquement
        AND EFFECTIF != "NN" --filtrer entreprises individuelles et trucs zombies
        AND EFFECTIF != "Inconnu"
        AND EFFECTIF != "00"
    ORDER BY EFFECTIF DESC;