-- SQLite
SELECT DATE,CODEPOSTAL,VILLE,NATURE,RCS FROM annonces
    WHERE DATE >= DATE('now','-1 day')
    AND DATE < DATE('now', '+1 day')