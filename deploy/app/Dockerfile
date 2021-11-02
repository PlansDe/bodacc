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
COPY --from=vscode /code/app/bin/Release/net5.0/ /usr/local/bin/
COPY app/plansde /etc/cron.d/plansde
COPY INSEE /usr/local/bin/INSEE
RUN chmod 0644 /etc/cron.d/plansde
RUN crontab /etc/cron.d/plansde
RUN touch /var/log/cron.log
CMD crond && tail -f /var/log/cron.log