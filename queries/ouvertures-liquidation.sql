-- SQLite
SELECT annonces.PARUTION, annonces.NUMERO, annonces.DATE,annonces.RCS, annonces.CODEPOSTAL,annonces.VILLE,annonces.FORMEJURIDIQUE
      FROM annonces 
      WHERE (
                  annonces.NATURE = "jugement d'ouverture de liquidation judiciaire"
                  OR annonces.NATURE = "jugement de conversion en liquidation judiciaire"
            )
         
         AND annonces.FORMEJURIDIQUE != "s.c.i"
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