# AzerothCore CMS Full Release Procedure 🚀

---

## 1. First-time Setup

### 1.1 Clone AzerothCore Docker Compose

```sh
git clone https://github.com/azerothcore/acore-docker
cd acore-docker
```

### 1.2 (Optional) Adjust `docker-compose.yml`

- Set MySQL root password
- Adjust port mappings (e.g., 63306 for MySQL, 7878 for SOAP)
- Change volumes location if needed

**Default MySQL data volume is persisted in:**
```yaml
volumes:
  ac-database:
  ac-client-data:
```

---

## 2. Full Clean Start

### 2.1 Wipe All Docker Containers and Volumes

> ⚠️ This fully resets the database to an empty state!

```sh
docker compose down -v
docker compose up
```

The `ac-db-import` service will auto-initialize a fresh database schema.

---

## 3. Post-Initialization Manual Setup

### 3.1 Create SOAP User and GM Level

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

## 4. Optional Config Adjustments

### 4.1 Copy Config Files for Customization

```sh
docker compose cp ac-worldserver:/azerothcore/env/dist/etc/worldserver.conf conf/worldserver.conf
docker compose cp ac-worldserver:/azerothcore/env/dist/etc/authserver.conf conf/authserver.conf
docker compose cp ac-worldserver:/azerothcore/env/dist/etc/dbimport.conf conf/dbimport.conf
```

### 4.2 Mount Configs via `docker-compose.override.yml` (if needed)

> In most cases, the default config works perfectly.

---

## 5. CMS Deployment

### 5.1 Prepare `appsettings.json` for CMS

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

### 5.2 Build and Run CMS Locally

```sh
dotnet build
dotnet run
```

> On first run, the `siteUsers` table will be automatically created if it does not exist.

---

## 6. Disaster Recovery Procedures

### 6.1 Full Reset (Wipe Everything)

```sh
docker compose down -v
docker compose up
```

> This completely resets AzerothCore data and CMS custom tables.

### 6.2 Partial Reset (Keep CMS Data)

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

> The CMS will automatically recreate the `siteUsers` table.

### 6.3 Full MySQL Backup & Restore

**Backup:**
```sh
docker exec -i ac-database mysqldump -uroot -ppassword --all-databases > full-backup.sql
```

**Restore:**
```sh
cat full-backup.sql | docker exec -i ac-database mysql -uroot -ppassword
```

---

## 7. Deployment Notes for Production

- Use an external persistent MySQL database for live servers
- Never expose MySQL directly to the public
- Protect SOAP service behind a firewall or VPN
- Use nginx or a reverse proxy for public CMS access
- Use valid SSL certificates

