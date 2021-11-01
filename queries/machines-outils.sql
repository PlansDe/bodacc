SELECT  etablissements.SIREN,
        etablissements.SIRET,
        uniteslegales.NOM NOM_UNITE_LEGALE,
        uniteslegales.EFFECTIFS,
        etablissements.CP,
        etablissements.VILLE, 
        uniteslegales.CATEGORIEJURIDIQUE,
        (SELECT geocodes.NOM
            FROM geocodes 
            WHERE CODE = etablissements.PAYS) PAYS
    FROM etablissements
        LEFT JOIN uniteslegales
        ON etablissements.SIREN = uniteslegales.SIREN
    WHERE etablissements.ACTIVITE = '28.41Z'
        AND etablissements.ETATADMIN = 'A' -- entreprises actives uniquement
        AND uniteslegales.EFFECTIFS != 'NN' --filtrer entreprises individuelles et trucs zombies
        AND uniteslegales.EFFECTIFS != 'Inconnu'
		AND uniteslegales.CATEGORIEJURIDIQUE != '1000'
        AND uniteslegales.EFFECTIFS != '00'
    ORDER BY uniteslegales.EFFECTIFS DESC;