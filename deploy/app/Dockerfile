FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine as builder
FROM builder as vscode

WORKDIR /code

RUN apk update 
RUN apk add sqlite make 

COPY app/ app/
COPY tests/ tests/
COPY convert/ convert/
COPY app.sln .
COPY Makefile .
COPY README.md .

RUN make release

FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine as release

RUN apk update
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
COPY --from=vscode /code/app/bin/Release/net5.0/ /usr/local/bin/
COPY deploy/app/plansde /etc/cron.d/plansde
COPY INSEE /usr/local/bin/INSEE
COPY deploy/app/wait-for-it.sh /usr/local/bin/
COPY deploy/app/start.sh /usr/local/bin/
RUN chmod 755 /usr/local/bin/wait-for-it.sh
RUN chmod 755 /usr/local/bin/start.sh
RUN chmod 0644 /etc/cron.d/plansde
RUN crontab /etc/cron.d/plansde
RUN touch /var/log/cron.log