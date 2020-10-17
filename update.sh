#!/bin/sh
git pull
docker build . trexlock
mkdir -p ~/trexlock/data
docker container rm -f trexlock
docker run trexlock -d --name=trexlock -P 8223:443 -v ~/trexlock/data:/data --restart unless-stopped 