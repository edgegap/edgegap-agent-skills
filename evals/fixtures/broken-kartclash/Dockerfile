FROM ubuntu:22.04
COPY Builds/LinuxServer/ /server/
WORKDIR /server
RUN chmod +x KartServer.x86_64
EXPOSE 7777
CMD ["./KartServer.x86_64"]
