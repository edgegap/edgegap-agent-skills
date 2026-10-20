FROM ubuntu:22.04

COPY Builds/Server /root/build/
COPY edgegap/start.sh /start.sh
RUN chmod +x /root/build/ServerBuild /start.sh

EXPOSE 7777/udp
CMD ["/start.sh"]
