-- SQLite
SELECT annonces.NUMERO, annonces.DATE,annonces.CODEPOSTAL,annonces.VILLE,annonces.RCS,annonces.FORMEJURIDIQUE, etablissements.EFFECTIFS
      FROM annonces 
      INNER JOIN etablissements ON annonces.RCS=etablissements.SIREN OR annonces.RCS = etablissements.SIRET
      WHERE (
                  annonces.NATURE = "jugement d'ouverture d'une procédure de sauvegarde accélérée"
                  OR annonces.NATURE = "jugement d'ouverture d'une procédure de sauvegarde"
            )
         
         AND annonces.FORMEJURIDIQUE != "s.cFORMEJURfrom.i"
         AND annonces.FORMEJURIDIQUE != "s.c.i."
         AND annonces.FORMEJURIDIQUE != "société civile immobilière"
         AND annonces.FORMEJURIDIQUE != "société civile immobiliére"
         AND annonces.FORMEJURIDIQUE != "societe civile immobiliere"
         AND annonces.FORMEJURIDIQUE != "sci"
         AND annonces.DATE IS NOT NULL
         AND annonces.DATE != ""
         AND annonces.DATE >= "2008-01-01" -- some ugliness here
         AND annonces.DATE <= "2022-01-01" -- some ugliness here too
         ORDER BY annonces.DATE ASC;