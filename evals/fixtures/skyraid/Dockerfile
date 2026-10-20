FROM ubuntu:22.04

COPY Builds/Server /root/build/
WORKDIR /root/build
RUN chmod +x ./ServerBuild

EXPOSE 7777/udp
CMD env && ./ServerBuild -batchmode -nographics -logFile -
