const Pool = require('pg').Pool;
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
            throw error;
        }
        response.status(200).json(results.rows);
    })
};

const getGeocodeById = (request, response) => {
    const id = parseInt(request.params.id)

    pool.query('SELECT * FROM geocodes WHERE code = $1', [id], (error, results) => {
        if (error) {
            throw error;
        }
        response.status(200).json(results.rows);
    })
};

module.exports = {
    getGeocodes,
    getGeocodeById,
};