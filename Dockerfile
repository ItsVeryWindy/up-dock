FROM buildpack-deps:stretch

RUN apk add --no-cache libstdc++ libintl libressl-dev

WORKDIR /up-dock

COPY src/UpDock/bin/Release/netcoreapp3.1/linux-musl-x64/publish .

ENTRYPOINT [ "./up-dock" ]
