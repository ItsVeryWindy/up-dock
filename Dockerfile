FROM buildpack-deps:stretch

WORKDIR /up-dock

COPY src/UpDock/bin/Release/net5.0/linux-x64/publish .

ENTRYPOINT [ "./up-dock" ]
