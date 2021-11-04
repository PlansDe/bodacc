const swaggerAutogen = require('swagger-autogen')()

const outputFile = './swagger_output.json'
const endpointsFiles = ['./index.js']


const doc = {
    info: {
        title: 'PLANSDE',
        description: 'This API exposes parts of SIRENE and BODACC public databases',
    },
    host: '',
    schemes: ['http'],
};

swaggerAutogen(outputFile, endpointsFiles, doc);