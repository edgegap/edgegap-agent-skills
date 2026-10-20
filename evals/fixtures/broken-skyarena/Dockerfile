FROM ubuntu:22.04

WORKDIR /game
COPY build/server/ /game/
RUN chmod +x /game/server.x86_64

EXPOSE 9050
CMD sh -c "env && /game/server.x86_64 --headless -- --server"
