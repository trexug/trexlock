#!/bin/sh
git pull
docker build . -t trexlock
mkdir -p ~/trexlock/data
docker container rm -f trexlock
docker run -d --name trexlock --restart unless-stopped trexlock -P 8223:443 -v ~/trexlock/data:/data 