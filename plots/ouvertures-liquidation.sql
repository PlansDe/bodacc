-- SQLite
SELECT DATE,CODEPOSTAL,VILLE,RCS,FORMEJURIDIQUE 
      FROM annonces 
      WHERE NATURE = "Jugement d'ouverture de liquidation judiciaire"
         AND FORMEJURIDIQUE != "S.C.I"
         AND FORMEJURIDIQUE != "Société civile immobilière"
         AND FORMEJURIDIQUE != "Société Civile Immobilière"
         AND FORMEJURIDIQUE != "SOCIETE CIVILE IMMOBILIERE"
         AND FORMEJURIDIQUE != "Société civile immobiliére"
         AND DATE IS NOT NULL
         AND DATE != ""
         AND DATE >= "2008-01-01"
      ORDER BY DATE ASC;