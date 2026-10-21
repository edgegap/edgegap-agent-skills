FROM ubuntu:22.04
RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*
COPY Builds/Server /root/build/
WORKDIR /root/
EXPOSE 7770/udp
CMD ["/bin/bash", "-c", "env; /root/build/ServerBuild -batchmode -nographics"]
