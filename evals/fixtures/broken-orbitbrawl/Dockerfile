FROM ubuntu:22.04
ENV EDGEGAP_API_TOKEN="token xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
COPY Builds/Server/ /app/
WORKDIR /app
RUN chmod +x OrbitBrawlServer.x86_64
EXPOSE 7777/udp
ENTRYPOINT ["./OrbitBrawlServer.x86_64", "-batchmode", "-nographics"]
