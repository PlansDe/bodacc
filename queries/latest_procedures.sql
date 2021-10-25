-- SQLite
SELECT DATE,CODEPOSTAL,VILLE,NATURE,RCS FROM annonces
    WHERE DATE <= DATE('now') -- ugliness in the base
    AND DATE >= "2008-01-01" -- ugliness in the base
    AND NATURE = "jugement de clôture pour insuffisance d'actif"
    ORDER BY DATE DESC