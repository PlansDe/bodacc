-- SQLite
SELECT annonces.DATE,annonces.CODEPOSTAL,annonces.VILLE,annonces.RCS,annonces.FORMEJURIDIQUE, uniteslegales.NOM, etablissements.EFFECTIFS
      FROM annonces 
      INNER JOIN uniteslegales ON annonces.RCS=uniteslegales.SIREN
      INNER JOIN etablissements ON annonces.RCS=etablissements.SIREN
      WHERE annonces.NATURE = "jugement d'ouverture de liquidation judiciaire"
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