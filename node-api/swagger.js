const swaggerAutogen = require('swagger-autogen')()

const outputFile = './swagger_output.json'
const endpointsFiles = ['./index.js']


const doc = {
    info: {
        title: 'PLANSDE',
        description: 'Description',
    },
    host: '',
    schemes: ['http'],
};

swaggerAutogen(outputFile, endpointsFiles, doc);