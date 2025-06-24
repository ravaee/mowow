# MoWow CMS A CMS Quick For Azerothcore 

## 1. Post-Initialization Manual Setup

### 1.1 Create SOAP User and GM Level

Open a new terminal and run:

```sh
docker ps
docker attach acore-docker-ac-worldserver-1
```

Inside the WorldServer console:

```sh
account create soapuser soappassword soappassword
account set gmlevel soapuser 3 -1
```

---

## 2. Optional Config Adjustments

### 2.1 Copy Config Files for Customization

```sh
docker compose cp ac-worldserver:/azerothcore/env/dist/etc/worldserver.conf conf/worldserver.conf
```

### 2.2 Mount Configs via `docker-compose.override.yml` (if needed)

> In most cases, the default config works perfectly.

---

## 3. CMS Deployment

### 3.1 Prepare `appsettings.json` for CMS

```json
{
  "ConnectionStrings": {
    "AzerothCoreAuthDatabase": "server=localhost;port=63306;database=acore_auth;uid=root;password=password;",
    "AzerothCoreCharactersDatabase": "server=localhost;port=63306;database=acore_characters;uid=root;password=password;"
  },
  "AzerothCore": {
    "SoapUrl": "http://localhost:7878/",
    "SoapUser": "soapuser",
    "SoapPassword": "soappassword"
  }
}
```

### 3.2 Build and Run CMS Locally

```sh
dotnet build
dotnet run
```

> On first run, the `siteUsers` table will be automatically created if it does not exist.

---

## 4. Disaster Recovery Procedures

### 4.1 Full Reset (Wipe Everything)

```sh
docker compose down -v
docker compose up
```

> This completely resets AzerothCore data and CMS custom tables.

### 4.2 Partial Reset (Keep CMS Data)

- Access phpMyAdmin: [http://localhost:8080](http://localhost:8080)
- Drop only these databases:
  - `acore_auth`
  - `acore_characters`
  - `acore_world`
- Restart Docker:

```sh
docker compose down
docker compose up
```



## 5. Deployment Notes for Production

- Use an external persistent MySQL database for live servers
- Never expose MySQL directly to the public
- Protect SOAP service behind a firewall or VPN
- Use nginx or a reverse proxy for public CMS access
- Use valid SSL certificates
