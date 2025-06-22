# AzerothCore Mo helper

## Create Account

### Prerequisites
- Docker and Docker Compose installed
- AzerothCore server running via `docker compose up`

### Steps

1. **Start the AzerothCore server**
   In your project directory, run:
   ```pwsh
   docker compose up
   ```
   This will start all necessary containers, including the worldserver.

2. **Access the Worldserver Console**
   Open a new terminal window and run:
   ```pwsh
   docker attach acore-docker-ac-worldserver-1
   ```
   This attaches you to the worldserver console, allowing you to run AzerothCore commands.

3. **Create an Account**
   In the worldserver console, use the following command (replace `<user>`, `<password>`, and `<confirm password>` with your desired credentials):
   ```
   account create <user> <password> <confirm password>
   ```

4. **Detach from the Console**
   To leave the console but keep the server running, press:
   `CTRL-p` then `CTRL-q`

5. **List All Running Containers**
   To see all containers you can attach to, run:
   ```pwsh
   docker ps
   ```

### Additional Resources
- [List of GM Commands](https://www.azerothcore.org/wiki/gm-commands)
- [Game Client Download](https://www.azerothcore.org/wiki/installation#game-client)

---

curl -u soapuser:soappassword -H "Content-Type: text/xml" -d @request.xml http://localhost:7878/


curl -H "Content-Type: text/xml" -H "Authorization: Basic c29hcHVzZXI6c29hcHBhc3N3b3Jk" --data-binary "@test.xml" http://localhost:7878/

---

## Automated Setup Scripts

### Linux (Bash)

A script is provided to automate SOAP setup and worldserver configuration for AzerothCore on Linux:

```bash
cd website/scripts
bash AzerothCoreServiceCreate.sh
```

This script will:
- Start AzerothCore containers
- Wait for initialization
- Extract and modify `worldserver.conf` for SOAP
- Create a Docker override for config mounting
- Restart containers
- Create a SOAP user with GM level

### Windows (PowerShell)

A PowerShell script is also available for Windows users:

```powershell
cd website/scripts
./AzerothCoreServiceCreate.ps1
```

This script performs the same steps as the Bash script, but is tailored for Windows environments.

---

After running the appropriate script, SOAP should be enabled and accessible on port 7878 with the credentials `soapuser` / `soappassword`.