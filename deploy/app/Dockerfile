FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine as builder
FROM builder as vscode

WORKDIR /code

RUN apk update 
RUN apk add sqlite make 

RUN mkdir app
RUN mkdir INSEE
RUN mkdir tests
COPY app/ app/
COPY INSEE/ INSEE/
COPY tests/ tests/
COPY convert/ convert/
COPY app.sln .
COPY createtable.sh .
COPY Makefile .
COPY README.md .
COPY .vscode/ .vscode/

RUN make release

FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine as release

COPY --from=vscode /code/app/bin/Release/net5.0/ /usr/local/bin/