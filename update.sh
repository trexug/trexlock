#!/bin/sh
git pull
docker build . -t trexlock
mkdir -p ~/trexlock/data
docker container rm -f trexlock
docker run --name trexlock --restart unless-stopped --device /dev/gpiomem -p 8223:443 -v ~/trexlock/data:/data -d trexlock