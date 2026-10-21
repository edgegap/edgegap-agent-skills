# Edgegap game server image for a Godot 4 Linux dedicated server export.
#   godot --headless --path . --export-release "Edgegap Linux Server" build/server/server.x86_64
#   docker build --platform linux/amd64 -t registry.edgegap.com/<project>/<game>:<unique tag> .
# Build context: the project root, with .dockerignore keeping only build/server.
# EXPOSE the port in edgegap_settings.gd (EdgegapSettings.PORT); the app
# version must use the same internal port.
FROM --platform=linux/amd64 debian:bookworm-slim

# ca-certificates: HTTPS calls from the server (your backend, other APIs).
# tini runs as PID 1 and passes SIGTERM on to Godot. The kernel drops signals
# that PID 1 has no handler for, and Godot installs none, so without tini the
# server ignores Edgegap's (and docker stop's) SIGTERM until it is killed.
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates tini \
    && rm -rf /var/lib/apt/lists/* \
    && useradd --create-home --uid 1000 server

WORKDIR /server
# An export with binary_format/embed_pck=true is a single file. Without
# embed_pck, the .pck next to the binary (same base name) is copied too.
COPY build/server/ /server/
RUN chmod +x /server/server.x86_64 && chown -R server:server /server

USER server
# ENet = UDP. For WebSocketMultiplayerPeer use EXPOSE 7777/tcp.
EXPOSE 7777/udp
# --headless: no window or GPU. Everything after "--" is a user argument
# (OS.get_cmdline_user_args()). Don't print the environment here: it holds
# ARBITRIUM_DELETE_TOKEN.
ENTRYPOINT ["/usr/bin/tini", "--", "/server/server.x86_64", "--headless", "--", "--server"]
