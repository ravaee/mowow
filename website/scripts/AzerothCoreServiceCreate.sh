#!/bin/bash

set -e

echo "==== Starting AzerothCore Docker Compose ===="
docker-compose up -d

echo "==== Waiting for containers to fully initialize (30s) ===="
sleep 30

echo "==== Extracting worldserver.conf ===="
mkdir -p conf
docker cp acore-docker-ac-worldserver-1:/azerothcore/env/dist/etc/worldserver.conf ./conf/worldserver.conf

echo "==== Modifying worldserver.conf ===="
cp ./conf/worldserver.conf ./conf/worldserver.conf.backup

sed -i 's/^SOAP.Enabled.*/SOAP.Enabled = 1/' ./conf/worldserver.conf
sed -i 's|^SOAP.IP.*|SOAP.IP = "0.0.0.0"|' ./conf/worldserver.conf
sed -i 's/^SOAP.Port.*/SOAP.Port = 7878/' ./conf/worldserver.conf
sed -i 's/^SOAP.Username.*/SOAP.Username = "soapuser"/' ./conf/worldserver.conf
sed -i 's/^SOAP.Password.*/SOAP.Password = "soappassword"/' ./conf/worldserver.conf

echo "==== Creating docker-compose.override.yml ===="
cat > docker-compose.override.yml <<EOF
version: '3.9'

services:
  ac-worldserver:
    volumes:
      - ./conf/worldserver.conf:/azerothcore/env/dist/etc/worldserver.conf
EOF

echo "==== Restarting containers with new config ===="
docker-compose down
docker-compose up -d

echo "==== Waiting for worldserver to start (30s) ===="
sleep 30

echo "==== Creating SOAP account inside worldserver ===="
docker exec acore-docker-ac-worldserver-1 bash -c "worldserver -s 'account create soapuser soappassword soappassword' && worldserver -s 'account set gmlevel soapuser 3 -1'"

echo "==== Setup complete! You can now test SOAP on port 7878 ===="
