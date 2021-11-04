SELECT DATE from annonces
    LEFT JOIN uniteslegales
    ON annonces.RCS = uniteslegales.SIREN
    WHERE DATE >= '2008-01-01'
    AND DATE <= NOW()
    AND uniteslegales.EFFECTIFS != 'NN'
    AND uniteslegales.EFFECTIFS >= '01'
    -- filtrer les sociétés civiles immobilières
    AND uniteslegales.CATEGORIEJURIDIQUE != '6540'
    AND uniteslegales.CATEGORIEJURIDIQUE != '6541'
    AND uniteslegales.CATEGORIEJURIDIQUE != '6542'
    AND uniteslegales.CATEGORIEJURIDIQUE != '6543'
    AND uniteslegales.CATEGORIEJURIDIQUE != '6544'
    AND (
        NATURE = 'jugement de clôture pour insuffisance d''actif'
        OR NATURE = 'jugement de clôture de la liquidation des biens pour insuffisance d''actif'
        OR NATURE = 'jugement de clôture pour insuffisance d''actif et autorisant la reprise des poursuites individuelles'
    )
    ORDER BY EFFECTIFS DESC;
