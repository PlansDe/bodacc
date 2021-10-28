-- SQLite

SELECT  etablissements.SIREN,
        etablissements.SIRET,
        uniteslegales.NOM NOM_UNITE_LEGALE,
        uniteslegales.EFFECTIFS EFFECTIFS_UNITE_LEGALE,
        etablissements.CP,
        etablissements.VILLE, 
        uniteslegales.CATEGORIEJURIDIQUE,
        (SELECT geocodes.NOM
            FROM geocodes 
            WHERE CODE = etablissements.PAYS) PAYS
    FROM etablissements
        LEFT JOIN uniteslegales
        ON etablissements.SIREN = uniteslegales.SIREN
    WHERE etablissements.ACTIVITE = "28.41Z"
        AND etablissements.ETATADMIN = "A" -- entreprises actives uniquement
        AND EFFECTIFS_UNITE_LEGALE != "NN" --filtrer entreprises individuelles et trucs zombies
        AND EFFECTIFS_UNITE_LEGALE != "Inconnu"
        AND EFFECTIFS_UNITE_LEGALE != "00"
    ORDER BY EFFECTIFS_UNITE_LEGALE DESC;