-- SQLite
SELECT DATE,CODEPOSTAL,VILLE,RCS,FORMEJURIDIQUE 
      FROM annonces 
      WHERE NATURE = "jugement d'ouverture de liquidation judiciaire"
         AND FORMEJURIDIQUE != "s.c.i"
         AND FORMEJURIDIQUE != "s.c.i."
         AND FORMEJURIDIQUE != "société civile immobilière"
         AND FORMEJURIDIQUE != "société civile immobiliére"
         AND FORMEJURIDIQUE != "societe civile immobiliere"
         AND FORMEJURIDIQUE != "sci"
         AND DATE IS NOT NULL
         AND DATE != ""
         AND DATE >= "2008-01-01"
      ORDER BY DATE ASC;