FROM buildpack-deps:stretch

WORKDIR /up-dock

COPY src/UpDock/bin/Release/netcoreapp3.1/linux-x64/publish .

ENTRYPOINT [ "./up-dock" ]
