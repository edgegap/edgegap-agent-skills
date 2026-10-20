#!/bin/sh
# Start BR200 as a dedicated server on Edgegap.
cd /root/build
exec ./ServerBuild -batchmode -nographics -logFile - \
  -dedicatedServer -battleRoyale \
  -sessionName "$ARBITRIUM_REQUEST_ID" \
  -region us \
  -ip "$ARBITRIUM_PUBLIC_IP" -port 7777 \
  $EXTRA_ARGS
