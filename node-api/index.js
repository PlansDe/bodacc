
function date(a, b) {
    console.log(a);
    console.log(b);
};

const app = require('express')();
const http = require('http');
const swaggerUi = require('swagger-ui-express')
const swaggerFile = require('./swagger_output.json')

http.createServer(app).listen(3000);


app.get('/', (request, response) => {
    response.json({ info: 'Node.js, Express, and Postgres API' })
});

const db = require('./queries');
app.get('/geocodes', db.getGeocodes);
app.get('/geocodes/:id', db.getGeocodeById);
app.get('/geocodes/:country', db.getGeocodeByCountry);
app.get('/codesnaf', db.getCodesNaf);
app.get('/codesnaf/:id', db.getCodeNafById);
app.get('/codesnaf/:label', db.getCodeNafByName);
app.get('/etablissements/:codenaf', db.getEtablissementsByCodeNaf);
app.get('/annonces/latests/:days', db.getLatestAnnonces);
app.use('/doc', swaggerUi.serve, swaggerUi.setup(swaggerFile));