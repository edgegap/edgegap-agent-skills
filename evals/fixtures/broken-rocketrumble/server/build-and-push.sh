#!/bin/sh
# usage: ./build-and-push.sh
set -e
docker build -t registry.edgegap.com/rr-studio/rocket-rumble:latest .
docker push registry.edgegap.com/rr-studio/rocket-rumble:latest
