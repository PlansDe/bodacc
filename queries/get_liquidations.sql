-- SQLite
SELECT annonces.DATE, annonces.RCS, etablissements.EFFECTIFS
      FROM annonces 
      LEFT JOIN etablissements ON annonces.RCS = etablissements.SIREN
      WHERE (
                annonces.NATURE = "jugement d'ouverture de liquidation judiciaire"
                OR annonces.NATURE = "jugement de conversion en liquidation judiciaire"
            )
         AND etablissements.EFFECTIFS >= "01"
         AND etablissements.EFFECTIFS != "NN"
         AND annonces.DATE IS NOT NULL
         AND annonces.DATE != ""
         AND annonces.DATE >= "2021-10-14" -- some ugliness here
         AND annonces.DATE <= "2022-01-01" -- some ugliness here too
         ORDER BY etablissements.EFFECTIFS DESC;