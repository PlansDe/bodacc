#!/usr/bin bash

cd /usr/local/bin && dotnet /usr/local/bin/app.dll 
#&& crond && tail -f /var/log/cron.log