FROM mcr.microsoft.com/dotnet/sdk:5.0-focal as builder
FROM builder as vscode

WORKDIR /app

RUN apt update \
    && apt install --assume-yes --quiet --no-install-recommends \
    dpkg \
    curl \
    wget \
    ca-certificates \
    apt-transport-https \
    sqlite3 \
    git \
    software-properties-common

RUN add-apt-repository ppa:deadsnakes/ppa

RUN apt update \
    && apt install --assume-yes --quiet --no-install-recommends \
    python3.8 

ADD . .