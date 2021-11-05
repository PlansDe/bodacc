SELECT RCS,DATE,EFFECTIFS, NATURE from annonces
    LEFT JOIN uniteslegales
    ON annonces.RCS = uniteslegales.SIREN
    WHERE DATE >= NOW() - INTERVAL '15 DAY'
    AND DATE <= NOW()
    AND uniteslegales.EFFECTIFS != 'NN'
    AND uniteslegales.EFFECTIFS >= '01'
    -- filtrer les sociétés civiles immobilières
    AND uniteslegales.CATEGORIEJURIDIQUE <> all (array['6540','6541','6542','6543', '6544'])
    AND (
        NATURE ~* 'jugement' /* à améliorer si il y a des jugements dont vous ne voulez pas.
        = 'jugement d''ouverture de liquidation judiciaire'
        OR NATURE = 'jugement de clôture pour insuffisance d''actif'
        OR NATURE = 'jugement de conversion en liquidation judiciaire'
        OR NATURE = 'jugement d''extension de liquidation judiciaire'
        OR NATURE = 'jugement de clôture de la liquidation des biens pour insuffisance d''actif'
        OR NATURE = 'jugement d''extension d''une procédure de redressement judiciaire'
        OR NATURE = 'jugement de clôture pour insuffisance d''actif et autorisant la reprise des poursuites individuelles'
        OR NATURE = 'jugement de conversion en liquidation judiciaire de la procédure de sauvegarde'
        OR NATURE = 'jugement autorisant la reprise des poursuites individuelles des créanciers'
        OR NATURE = 'jugement de conversion en liquidation judiciaire de la procédure de sauvegarde financière accélérée'
    */
    )
    ORDER BY EFFECTIFS DESC;
