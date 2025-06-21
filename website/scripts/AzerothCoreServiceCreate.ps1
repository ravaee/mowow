Write-Output "==== Starting AzerothCore Docker Compose ===="
docker-compose up -d

Write-Output "==== Waiting for containers to fully initialize (30s) ===="
Start-Sleep -Seconds 30

Write-Output "==== Extracting worldserver.conf ===="
New-Item -ItemType Directory -Path ".\conf" -Force
docker cp acore-docker-ac-worldserver-1:/azerothcore/env/dist/etc/worldserver.conf .\conf\worldserver.conf

Write-Output "==== Modifying worldserver.conf ===="
Copy-Item .\conf\worldserver.conf .\conf\worldserver.conf.backup

(Get-Content .\conf\worldserver.conf) `
    -replace 'SOAP.Enabled.*', 'SOAP.Enabled = 1' `
    -replace 'SOAP.IP.*', 'SOAP.IP = "0.0.0.0"' `
    -replace 'SOAP.Port.*', 'SOAP.Port = 7878' `
    -replace 'SOAP.Username.*', 'SOAP.Username = "soapuser"' `
    -replace 'SOAP.Password.*', 'SOAP.Password = "soappassword"' | Set-Content .\conf\worldserver.conf

Write-Output "==== Creating docker-compose.override.yml ===="
@"
version: '3.9'

services:
  ac-worldserver:
    volumes:
      - ./conf/worldserver.conf:/azerothcore/env/dist/etc/worldserver.conf
"@ | Out-File -Encoding UTF8 docker-compose.override.yml

Write-Output "==== Restarting containers with new config ===="
docker-compose down
docker-compose up -d

Write-Output "==== Waiting for worldserver to start (30s) ===="
Start-Sleep -Seconds 30

Write-Output "==== Creating SOAP account inside worldserver ===="
docker exec acore-docker-ac-worldserver-1 worldserver -s "account create soapuser soappassword soappassword"
docker exec acore-docker-ac-worldserver-1 worldserver -s "account set gmlevel soapuser 3 -1"

Write-Output "==== Setup complete! You can now test SOAP on port 7878 ===="
