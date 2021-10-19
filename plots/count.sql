-- SQLite
SELECT DATE FROM annonces 
        WHERE NATURE = "jugement d'ouverture de liquidation judiciaire"
           AND DATE >= "2021-01-01" AND DATE <= "2021-12-31"