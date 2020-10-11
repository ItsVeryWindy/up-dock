FROM alpine:3.8.5

RUN apk add --no-cache libstdc++ libintl libressl-dev

WORKDIR /docker-upgrade-tool

COPY src/DockerUpgradeTool/bin/Release/netcoreapp3.1/linux-musl-x64/publish .

ENTRYPOINT [ "./docker-upgrade-tool" ]
