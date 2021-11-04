const pg = require('pg');
const Pool = pg.Pool;

require('dotenv').config();
const pool = new Pool({
    user: process.env.API_USER,
    host: process.env.POSTGRE_HOST,
    database: 'bodacc',
    password: process.env.API_PASSWORD,
    port: 5432,
});

const getGeocodes = (request, response) => {
    pool.query('SELECT * FROM geocodes', (error, results) => {
        if (error) {
            response.status(500).json(error.message);
        }
        response.status(200).json(results?.rows);
    })
};

const getGeocodeById = (request, response) => {
    const id = parseInt(request.params.id)

    pool.query('SELECT * FROM geocodes WHERE code = $1', [id], (error, results) => {
        if (error) {
            response.status(500).json(error.message);
        }
        response.status(200).json(results?.rows);
    })
};

const getGeocodeByCountry = (request, response) => {
    const country = request.params.country;

    pool.query('SELECT * FROM geocodes WHERE NOM = $1', [country], (error, results) => {
        if (error) {
            response.status(500).json(error.message);
        }
        response.status(200).json(results?.rows);
    })
};

const getCodesNaf = (request, response) => {
    pool.query('SELECT * FROM codesnaf', (error, results) => {
        if (error) {
            response.status(500).json(error.message);
        }
        response.status(200).json(results?.rows);
    })
};

const getCodeNafById = (request, response) => {
    const id = request.params.id;

    pool.query('SELECT * FROM codesnaf WHERE code = $1', [id], (error, results) => {
        if (error) {
            response.status(500).json(error.message);
        }
        response.status(200).json(results?.rows);
    });
};

const getCodeNafByName = (request, response) => {
    const label = request.params.label;

    pool.query('SELECT * FROM codesnaf WHERE label = $1', [label], (error, results) => {
        if (error) {
            response.status(500).json(error.message);
        }
        response.status(200).json(results?.rows);
    });
};

const getEffectifs = (request, response) => {
    pool.query('SELECT * FROM effectifs', [], (error, results) => {
        if (error) {
            response.status(500).json(error.message);
        }
        response.status(200).json(results?.rows);
    })
};

const getEffectifsById = (request, response) => {
    var id = parseInt(request.params.id);
    pool.query('SELECT * FROM effectifs WHERE CODE = $1', [id], (error, results) => {
        if (error) {
            response.status(500).json(error.message);
        }
        response.status(200).json(results?.rows);
    })
};



const getCategoriesJuridiques = (request, response) => {
    pool.query('SELECT * FROM categoriesjuridiques', [], (error, results) => {
        if (error) {
            response.status(500).json(error.message);
        }
        response.status(200).json(results?.rows);
    })
};

const getCategoriesJuridiquesById = (request, response) => {
    var id = parseInt(request.params.id);
    pool.query('SELECT * FROM categoriesjuridiques WHERE CODE = $1', [id], (error, results) => {
        if (error) {
            response.status(500).json(error.message);
        }
        response.status(200).json(results?.rows);
    })
};

const getEtablissementsByCodeNaf = (request, response) => {
    const codenaf = request.params.codenaf;
    const query = `SELECT  etablissements.SIREN,
        etablissements.SIRET,
            uniteslegales.NOM NOM_UNITE_LEGALE,
                uniteslegales.EFFECTIFS,
                etablissements.CP,
                etablissements.VILLE,
                uniteslegales.CATEGORIEJURIDIQUE,
                (SELECT geocodes.NOM
            FROM geocodes 
            WHERE CODE = etablissements.PAYS) PAYS
    FROM etablissements
        LEFT JOIN uniteslegales
        ON etablissements.SIREN = uniteslegales.SIREN
    WHERE etablissements.ACTIVITE = '${codenaf}'
        AND etablissements.ETATADMIN = 'A' -- entreprises actives uniquement
        AND uniteslegales.EFFECTIFS != 'NN' --filtrer entreprises individuelles et trucs zombies
        AND uniteslegales.EFFECTIFS != 'Inconnu'
        AND uniteslegales.CATEGORIEJURIDIQUE != '1000'
        AND uniteslegales.EFFECTIFS != '00'
    ORDER BY uniteslegales.EFFECTIFS DESC;`;


    pool.query(query, [], (error, results) => {
        if (error) {
            response.status(500).json(error.message);
        }
        response.status(200).json(results?.rows);
    });
};

const getLatestAnnonces = (request, response) => {

    // #swagger.description = 'renvoie les liquidations judiciaires concernant les entreprises de plus de 2 salariés hors SCI ou sociétés unipersonnelles - max 60 jours'

    const days = parseInt(request.params.days);
    if (days >= 60) {
        response.status(401).json("too many data requested");
        return;
    }

    const query = `SELECT DISTINCT(RCS),DATE,EFFECTIFS, NATURE from annonces
    LEFT JOIN uniteslegales
    ON annonces.RCS = uniteslegales.SIREN
    WHERE DATE >= NOW() - INTERVAL '${days} DAY'
    AND DATE <= NOW() -- some dates are crawy (2129 and so on)
    -- plus de deux employés
    AND uniteslegales.EFFECTIFS != 'NN'
    AND uniteslegales.EFFECTIFS >= '01'
    -- filtrer les entreprises individuelles
    AND uniteslegales.CATEGORIEJURIDIQUE != '1000'
    -- filtrer les sociétés civiles immobilières
    AND uniteslegales.CATEGORIEJURIDIQUE != '6540'
    AND uniteslegales.CATEGORIEJURIDIQUE != '6541'
    AND uniteslegales.CATEGORIEJURIDIQUE != '6542'
    AND uniteslegales.CATEGORIEJURIDIQUE != '6543'
    AND uniteslegales.CATEGORIEJURIDIQUE != '6544'
    AND (
        NATURE = 'jugement d''ouverture de liquidation judiciaire'
        OR NATURE = 'jugement de clôture pour insuffisance d''actif'
        OR NATURE = 'jugement de conversion en liquidation judiciaire'
        OR NATURE = 'jugement d''extension de liquidation judiciaire'
        OR NATURE = 'jugement de clôture de la liquidation des biens pour insuffisance d''actif'
        OR NATURE = 'jugement d''extension d''une procédure de redressement judiciaire'
        OR NATURE = 'jugement de clôture pour insuffisance d''actif et autorisant la reprise des poursuites individuelles'
        OR NATURE = 'jugement de conversion en liquidation judiciaire de la procédure de sauvegarde'
        OR NATURE = 'jugement autorisant la reprise des poursuites individuelles des créanciers'
        OR NATURE = 'jugement de conversion en liquidation judiciaire de la procédure de sauvegarde financière accélérée'
    )
    ORDER BY EFFECTIFS DESC;
`;

    pool.query(query, [], (error, results) => {
        if (error) {
            response.status(500).json(error.message);
        }
        response.status(200).json(results?.rows);
    });
}


const getLiquidations = (request, response) => {

    // #swagger.description = 'renvoie les liquidations judiciaires concernant les entreprises de plus de 2 salariés hors SCI ou sociétés unipersonnelles - depuis 2008'

    const query = `SELECT DATE from annonces
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
`;

    pool.query(query, [], (error, results) => {
        if (error) {
            response.status(500).json(error.message);
        }
        response.status(200).json(results?.rows);
    });
}

module.exports = {
    getGeocodes,
    getGeocodeById,
    getGeocodeByCountry,
    getCodesNaf,
    getCodeNafById,
    getCodeNafByName,
    getEffectifs,
    getEffectifsById,
    getCategoriesJuridiques,
    getCategoriesJuridiquesById,
    getEtablissementsByCodeNaf,
    getLatestAnnonces,
    getLiquidations,
};