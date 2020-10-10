FROM alpine:3.12.0

WORKDIR /docker-upgrade-tool

COPY src/DockerUpgradeTool/bin/Release/netcoreapp3.1/linux-musl-x64/publish .

ENTRYPOINT [ "docker-upgrade-tool" ]
