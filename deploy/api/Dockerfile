FROM node:17-alpine

RUN apk update
COPY ./node-api/ /usr/local/bin/
COPY deploy/wait-for-it.sh /usr/local/bin/
RUN chmod 755 /usr/local/bin/wait-for-it.sh
RUN cd /usr/local/bin/ && npm install
RUN cd /usr/local/bin && npm run swagger-autogen