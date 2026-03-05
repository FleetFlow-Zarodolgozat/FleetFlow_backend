<div align="center">

# 🚗 FleetFlow Backend

**Modern fleet management platform – vehicle operations, driver management, trip tracking, fuel logging & service workflows**

[![.NET CI](https://github.com/FleetFlow-Zarodolgozat/FleetFlow_backend/actions/workflows/dotnet.yml/badge.svg)](https://github.com/FleetFlow-Zarodolgozat/FleetFlow_backend/actions/workflows/dotnet.yml)
[![Docker Deploy](https://github.com/FleetFlow-Zarodolgozat/FleetFlow_backend/actions/workflows/deploy.yml/badge.svg)](https://github.com/FleetFlow-Zarodolgozat/FleetFlow_backend/actions/workflows/deploy.yml)

</div>

---

## 📋 Tartalom / Table of Contents

- [Áttekintés](#-áttekintés)
- [Tech Stack](#-tech-stack)
- [Adatbázis sémák](#-adatbázis-sémák)
- [API Végpontok](#-api-végpontok)
- [Helyi fejlesztés](#-helyi-fejlesztés)
- [Docker](#-docker)
- [Kubernetes](#-kubernetes)
- [CI/CD Pipeline](#-cicd-pipeline)
- [Környezeti változók](#-környezeti-változók)

---

## 🔎 Áttekintés

A FleetFlow egy modern flottakezelő platform, amely egyszerűsíti a járműoperációkat, a sofőrkezelést, az útnyilvántartást, az üzemanyag-naplózást és a szervizfolyamatokat. A backend **ASP.NET Core 10** alapon fut, **MySQL** adatbázist használ, és **JWT-alapú** hitelesítéssel védett REST API-t nyújt.

Az alkalmazás kétféle felhasználói szerepet ismer:
- **ADMIN** – flottakezelő adminisztrátor
- **DRIVER** – sofőr (web + mobil kliens)

---

## 🛠 Tech Stack

| Réteg | Technológia |
|---|---|
| Runtime | .NET 10 (ASP.NET Core) |
| Adatbázis | MySQL 8 |
| ORM | Entity Framework Core 10 |
| Hitelesítés | JWT Bearer token |
| Jelszóhash | BCrypt.Net |
| E-mail | SMTP (Gmail) |
| Fájlkezelés | SixLabors.ImageSharp |
| API dokumentáció | Swagger / OpenAPI |
| Konténerizáció | Docker |
| Orkesztráció | Kubernetes (Nginx Ingress) |
| Container Registry | GitHub Container Registry (GHCR) |
| CI/CD | GitHub Actions |

---

## 🗄 Adatbázis sémák

Az adatbázis `create_tables.sql` fájlban található meg. A főbb táblák:

| Tábla | Leírás |
|---|---|
| `users` | Felhasználók (ADMIN / DRIVER szerepkörrel) |
| `drivers` | Sofőr-specifikus adatok (jogosítvány, stb.) |
| `vehicles` | Járművek (rendszám, VIN, állapot, km-óra) |
| `vehicle_assignments` | Jármű–sofőr hozzárendelések (időszakos) |
| `trips` | Útnyilvántartás |
| `fuel_logs` | Üzemanyag-naplók |
| `service_requests` | Szervizigények és -folyamatok |
| `calendar_events` | Naptáresemények |
| `notifications` | Értesítések |
| `files` | Feltöltött fájlok (pl. profilkép) |
| `password_tokens` | Jelszó-beállítási tokenek |

Az adatbázis inicializálásához futtasd:

```bash
mysql -u <user> -p < create_tables.sql
mysql -u <user> -p < add_datas.sql   # opcionális: mintaadatok
```

---

## 📡 API Végpontok

Az API alap URL-je: `https://fleetflow-zarodolgozat-backend-ressdominik.jcloud.jedlik.cloud`

Fejlesztési módban a Swagger UI elérhető: `http://localhost:5000/swagger`

A legtöbb végpont JWT tokent igényel. A tokent a `Authorization: Bearer <token>` fejlécben kell küldeni.

---

### 🔐 Hitelesítés – `/api`

| Metódus | Végpont | Szerepkör | Leírás |
|---|---|---|---|
| `POST` | `/api/login` | – | Bejelentkezés (web, admin) |
| `POST` | `/api/login-mobile` | – | Bejelentkezés mobilon (csak DRIVER) |

<details>
<summary>📝 Login – kérés/válasz példa</summary>

**POST `/api/login`**
```json
// Request body
{
  "email": "admin@example.com",
  "password": "YourPassword123"
}

// Response (200 OK)
"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```
</details>

---

### 👤 Sofőrkezelés – `/api/admin/drivers` *(ADMIN)*

| Metódus | Végpont | Leírás |
|---|---|---|
| `GET` | `/api/admin/drivers` | Sofőrök listázása (lapozható, szűrhető) |
| `POST` | `/api/admin/drivers` | Új sofőr létrehozása |
| `PATCH` | `/api/admin/drivers/edit/{id}` | Sofőr adatainak módosítása |
| `PATCH` | `/api/admin/drivers/activate/{id}` | Sofőr aktiválása |
| `PATCH` | `/api/admin/drivers/deactivate/{id}` | Sofőr deaktiválása |

**Query paraméterek (`GET /api/admin/drivers`):**
| Paraméter | Típus | Leírás |
|---|---|---|
| `stringQ` | string | Szabad szöveges keresés (név, e-mail, jogosítvány) |
| `isActiveQ` | bool | Aktív / inaktív szűrő |
| `ordering` | string | `fullname`, `fullname_desc`, `licenseexpirydate`, `licenseexpirydate_desc` |
| `page` | int | Lapszám (alapértelmezett: 1) |
| `pageSize` | int | Lapméret (max: 200, alapértelmezett: 25) |

---

### 🚙 Járműkezelés – `/api/admin/vehicles` *(ADMIN)*

| Metódus | Végpont | Leírás |
|---|---|---|
| `GET` | `/api/admin/vehicles` | Járművek listázása (lapozható, szűrhető) |
| `POST` | `/api/admin/vehicles` | Új jármű létrehozása |
| `PATCH` | `/api/admin/vehicles/edit/{id}` | Jármű adatainak módosítása |
| `PATCH` | `/api/admin/vehicles/activate/{id}` | Jármű aktiválása |
| `PATCH` | `/api/admin/vehicles/deactivate/{id}` | Jármű deaktiválása (RETIRED állapot) |

**Query paraméterek (`GET /api/admin/vehicles`):**
| Paraméter | Típus | Leírás |
|---|---|---|
| `stringQ` | string | Szabad szöveges keresés (rendszám, márka, VIN) |
| `status` | string | `ACTIVE`, `MAINTENANCE`, `RETIRED` |
| `ordering` | string | `year`, `year_desc`, `currentmileagekm`, `currentmileagekm_desc`, `brandmodel`, `licenseplate` |
| `page` | int | Lapszám |
| `pageSize` | int | Lapméret (max: 200) |

---

### 🔗 Jármű–Sofőr Hozzárendelés – `/api/admin` *(ADMIN)*

| Metódus | Végpont | Leírás |
|---|---|---|
| `GET` | `/api/admin/assign/driver/{userId}` | Sofőr jelenlegi hozzárendelésének ellenőrzése |
| `GET` | `/api/admin/assign/vehicle/{vehicleId}` | Jármű jelenlegi hozzárendelésének ellenőrzése |
| `POST` | `/api/admin/assign/{userId}/{vehicleId}` | Jármű hozzárendelése sofőrhöz |
| `PATCH` | `/api/admin/unassign/{userId}` | Hozzárendelés megszüntetése |
| `GET` | `/api/admin/assignment/history/{id}` | Jármű hozzárendelési előzményei |

---

### 🗺 Útnyilvántartás – `/api/trips`

| Metódus | Végpont | Szerepkör | Leírás |
|---|---|---|---|
| `GET` | `/api/trips/admin` | ADMIN | Összes út listázása |
| `GET` | `/api/trips/mine` | DRIVER | Saját utak listázása |
| `POST` | `/api/trips` | DRIVER | Új út rögzítése |
| `PATCH` | `/api/trips/delete/{id}` | ADMIN, DRIVER | Út soft törlése |
| `PATCH` | `/api/trips/restore/{id}` | ADMIN | Törölt út visszaállítása |

**Query paraméterek (`GET /api/trips/admin`):**
| Paraméter | Típus | Leírás |
|---|---|---|
| `stringQ` | string | Szabad szöveges keresés |
| `isDeleted` | bool | Törölt utak szűrője |
| `ordering` | string | `distance`, `distance_desc`, `starttime`, `starttime_desc` |
| `page` | int | Lapszám |
| `pageSize` | int | Lapméret (max: 200) |

---

### ⛽ Üzemanyag-napló – `/api/fuellogs`

| Metódus | Végpont | Szerepkör | Leírás |
|---|---|---|---|
| `GET` | `/api/fuellogs/admin` | ADMIN | Összes üzemanyag-napló |
| `GET` | `/api/fuellogs/mine` | DRIVER | Saját üzemanyag-naplók |
| `POST` | `/api/fuellogs` | DRIVER | Új üzemanyag-napló rögzítése |
| `PATCH` | `/api/fuellogs/delete/{id}` | ADMIN, DRIVER | Napló soft törlése |
| `PATCH` | `/api/fuellogs/restore/{id}` | ADMIN | Törölt napló visszaállítása |

---

### 🔧 Szervizigények – `/api/service-requests`

| Metódus | Végpont | Szerepkör | Leírás |
|---|---|---|---|
| `GET` | `/api/service-requests/admin` | ADMIN | Összes szervizigény |
| `GET` | `/api/service-requests/mine` | DRIVER | Saját szervizigények |
| `POST` | `/api/service-requests` | DRIVER | Új szervizigény beadása |
| `DELETE` | `/api/service-requests/cancel/{id}` | DRIVER | Szervizigény visszavonása |
| `PATCH` | `/api/service-requests/reject/{id}` | ADMIN | Szervizigény elutasítása |
| `PATCH` | `/api/service-requests/approve/{id}` | ADMIN | Szervizigény jóváhagyása |
| `PATCH` | `/api/service-requests/upload-details/{id}` | DRIVER | Szervizelés részleteinek feltöltése |
| `PATCH` | `/api/service-requests/edit-uploaded-data/{id}` | DRIVER | Feltöltött adatok szerkesztése |
| `PATCH` | `/api/service-requests/close/{id}` | ADMIN | Szervizigény lezárása |

---

### 📅 Naptár – `/api/calendarevents` *(ADMIN, DRIVER)*

| Metódus | Végpont | Leírás |
|---|---|---|
| `GET` | `/api/calendarevents` | Naptáresemények listázása |
| `POST` | `/api/calendarevents` | Naptárelem létrehozása |
| `DELETE` | `/api/calendarevents/{id}` | Naptárelem törlése |

---

### 🔔 Értesítések – `/api/notifications` *(ADMIN, DRIVER)*

| Metódus | Végpont | Leírás |
|---|---|---|
| `GET` | `/api/notifications` | Értesítések listázása |
| `POST` | `/api/notifications` | Értesítés küldése |
| `PATCH` | `/api/notifications/read` | Értesítések olvasottnak jelölése |
| `DELETE` | `/api/notifications/{id}` | Értesítés törlése |
| `GET` | `/api/notifications/unread-status` | Olvasatlan értesítések száma |

---

### 👤 Profil – `/api/profile` *(ADMIN, DRIVER)*

| Metódus | Végpont | Szerepkör | Leírás |
|---|---|---|---|
| `GET` | `/api/profile/mine` | ADMIN, DRIVER | Saját profiladatok lekérése |
| `GET` | `/api/profile/assigned-vehicle` | DRIVER | Hozzárendelt jármű adatai |
| `PATCH` | `/api/profile/edit` | ADMIN, DRIVER | Profiladatok módosítása |
| `PATCH` | `/api/profile/delete-profile-image` | ADMIN, DRIVER | Profilkép törlése |
| `POST` | `/api/profile/forgot-password` | – | Jelszóemlékeztető küldése |
| `POST` | `/api/profile/set-password` | – | Jelszó beállítása tokennel |

---

### 📊 Statisztikák – `/api/statistics`

| Metódus | Végpont | Szerepkör | Leírás |
|---|---|---|---|
| `GET` | `/api/statistics/driver/{id}` | ADMIN | Adott sofőr statisztikái |
| `GET` | `/api/statistics/vehicle/{id}` | ADMIN | Adott jármű statisztikái |
| `GET` | `/api/statistics/fuellog` | ADMIN | Üzemanyag-statisztikák |
| `GET` | `/api/statistics/trip` | ADMIN | Útstatisztikák |
| `GET` | `/api/statistics/servicerequest` | ADMIN | Szerviz statisztikák |
| `GET` | `/api/statistics/mine` | DRIVER | Saját statisztikák |
| `GET` | `/api/statistics/admin` | ADMIN | Admin dashboard statisztikák |

---

### 📁 Fájlkezelés – `/api/files` *(ADMIN, DRIVER)*

| Metódus | Végpont | Leírás |
|---|---|---|
| `POST` | `/api/files` | Fájl feltöltése |
| `GET` | `/api/files/{id}` | Fájl letöltése |
| `DELETE` | `/api/files/{id}` | Fájl törlése |
| `GET` | `/api/files/thumbnail/{userId}` | Felhasználó profilképének lekérése |

---

### ❤️ Health Check

| Metódus | Végpont | Leírás |
|---|---|---|
| `GET` | `/healthz` | Alkalmazás állapotának ellenőrzése |

---

## 💻 Helyi fejlesztés

### Előfeltételek

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MySQL 8+
- (opcionális) Docker Desktop

### Első lépések

```bash
# 1. Klónozd a repót
git clone https://github.com/FleetFlow-Zarodolgozat/FleetFlow_backend.git
cd FleetFlow_backend

# 2. Hozd létre az adatbázist
mysql -u root -p < create_tables.sql
mysql -u root -p < add_datas.sql   # opcionális mintaadatok

# 3. Állítsd be a kapcsolati paramétert (user secrets vagy appsettings.Development.json)
cd backend/backend
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;database=flottakezelo_db;user=root;password=yourpassword"
dotnet user-secrets set "Jwt:Key" "your-super-secret-jwt-key-min-32-chars"
dotnet user-secrets set "EmailSettings:Username" "your@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "your-app-password"
dotnet user-secrets set "Frontend:BaseUrl" "http://localhost:3000"

# 4. Indítsd el az alkalmazást
dotnet run
```

Az alkalmazás fejlesztési módban elérhető: `http://localhost:5000`
Swagger UI: `http://localhost:5000/swagger`

---

## 🐳 Docker

### Manuális build és futtatás

```bash
# Image buildelése
docker build -t fleetflow-backend:latest ./backend/backend

# Futtatás
docker run -d \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="server=host.docker.internal;database=flottakezelo_db;user=root;password=yourpassword" \
  -e Jwt__Key="your-super-secret-jwt-key-min-32-chars" \
  -e Jwt__Issuer="FleetFlow app" \
  -e Jwt__Audience="FleetFlowUsers" \
  -e EmailSettings__From="fleetflow.info@gmail.com" \
  -e EmailSettings__SmtpHost="smtp.gmail.com" \
  -e EmailSettings__SmtpPort="587" \
  -e EmailSettings__Username="your@gmail.com" \
  -e EmailSettings__Password="your-app-password" \
  -e Frontend__BaseUrl="http://localhost:3000" \
  --name fleetflow-backend \
  fleetflow-backend:latest
```

Az alkalmazás elérhető: `http://localhost:8080`

### Dockerfile leírás

```
backend/backend/Dockerfile
```

| Szakasz | Alap image | Leírás |
|---|---|---|
| `base` | `mcr.microsoft.com/dotnet/aspnet:10.0` | Futtatási környezet (port: 8080) |
| `build` | `mcr.microsoft.com/dotnet/sdk:10.0` | Fordítás |
| `publish` | `build` | Release publikálás |
| `final` | `base` | Végső, minimális image |

### GHCR (GitHub Container Registry)

A Docker image automatikusan buildeli és tölti fel a CI/CD pipeline a következő helyre:

```
ghcr.io/fleetflow-zarodolgozat/fleetflow-backend:latest
```

Kézi húzás:
```bash
docker pull ghcr.io/fleetflow-zarodolgozat/fleetflow-backend:latest
```

---

## ☸️ Kubernetes

A Kubernetes konfigurációs fájl: `backend/backend/deployment.yaml`

### Erőforrások

| Erőforrás | Típus | Leírás |
|---|---|---|
| `fleetflow-api-deployment` | Deployment | Az API konténer futtatása |
| `fleetflow-api-cluster-ip-service` | Service (ClusterIP) | Belső hálózati elérés (port 80 → 8080) |
| `fleetflow-api-ingress` | Ingress (nginx) | Külső elérés HTTPS-en keresztül |

### Ingress URL

```
https://fleetflow-zarodolgozat-backend-ressdominik.jcloud.jedlik.cloud
```

### Health Probe-ok

| Típus | Útvonal | Leírás |
|---|---|---|
| `startupProbe` | `GET /healthz` | Max 30 × 2 mp várakozás indulásra |
| `readinessProbe` | `GET /healthz` | 10 mp-enként, jelez ha nem kész |
| `livenessProbe` | `GET /healthz` | 20 mp-enként, újraindítja ha meghal |

### Manuális deploy

```bash
# Környezeti változók beállítása
export DB_CONNECTION_STRING="server=...;database=...;user=...;password=..."
export JWT_KEY="your-super-secret-jwt-key"
export EMAIL_USERNAME="your@gmail.com"
export EMAIL_PASSWORD="your-app-password"
export FRONTEND_BASE_URL="https://yourfrontend.example.com"

# Deploy alkalmazása
envsubst '$DB_CONNECTION_STRING $JWT_KEY $EMAIL_USERNAME $EMAIL_PASSWORD $FRONTEND_BASE_URL' \
  < backend/backend/deployment.yaml | kubectl apply -f -

# Rollout újraindítása
kubectl rollout restart deployment/fleetflow-api-deployment

# Állapot ellenőrzése
kubectl get pods
kubectl get svc
kubectl get ingress
```

### Szükséges Kubernetes Secrets

| Secret neve | Tartalom |
|---|---|
| `ghcr-login-secret` | GHCR Docker registry hitelesítő adatok |

A `ghcr-login-secret` létrehozása:
```bash
kubectl create secret docker-registry ghcr-login-secret \
  --docker-server=ghcr.io \
  --docker-username=<github-username> \
  --docker-password=<github-pat> \
  --docker-email=<email>
```

---

## 🔄 CI/CD Pipeline

### Munkafolyamatok

#### 1. `.NET CI` – `dotnet.yml`

Aktiválódik: `push` a `main` branchre

| Lépés | Leírás |
|---|---|
| Checkout | Kód letöltése |
| Setup .NET 10 | SDK telepítés |
| Restore | NuGet csomagok visszaállítása |
| Build | Fordítás |
| Test | Tesztek futtatása |

#### 2. `Build Docker Image, Push to GHCR, and Deploy to Kubernetes` – `deploy.yml`

Aktiválódik: `push` a `main` branchre

| Lépés | Leírás |
|---|---|
| Checkout | Kód letöltése |
| Docker Buildx | Multi-platform build előkészítés |
| GHCR Login | Bejelentkezés a GitHub Container Registrybe |
| Build & Push | Docker image buildje és feltöltése |
| kubectl setup | kubectl v1.32.0 telepítése |
| kubeconfig | Kubernetes konfig beállítása |
| Deploy | `envsubst` + `kubectl apply` |
| Rollout Restart | Deployment újraindítása |

---

## 🔑 Környezeti változók

| Változó | Leírás | Példa |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | MySQL kapcsolati string | `server=db;database=flottakezelo_db;user=root;password=pass` |
| `Jwt__Key` | JWT aláíró kulcs (min. 32 karakter) | `your-super-secret-key-here-1234567` |
| `Jwt__Issuer` | JWT kibocsátó | `FleetFlow app` |
| `Jwt__Audience` | JWT célközönség | `FleetFlowUsers` |
| `EmailSettings__From` | Feladó e-mail | `fleetflow.info@gmail.com` |
| `EmailSettings__DisplayName` | Megjelenő feladónév | `FleetFlow` |
| `EmailSettings__SmtpHost` | SMTP szerver | `smtp.gmail.com` |
| `EmailSettings__SmtpPort` | SMTP port | `587` |
| `EmailSettings__Username` | SMTP felhasználónév | `your@gmail.com` |
| `EmailSettings__Password` | SMTP jelszó / App password | `your-app-password` |
| `Frontend__BaseUrl` | Frontend alap URL | `https://yourfrontend.example.com` |
| `ASPNETCORE_ENVIRONMENT` | Környezet | `Production` / `Development` |

### GitHub Actions Secrets

A deploy pipeline az alábbi GitHub Secrets-eket igényli:

| Secret | Leírás |
|---|---|
| `DB_CONNECTION_STRING` | MySQL kapcsolati string |
| `JWT_KEY` | JWT aláíró kulcs |
| `EMAIL_USERNAME` | SMTP felhasználónév |
| `EMAIL_PASSWORD` | SMTP jelszó |
| `FRONTEND_BASE_URL` | Frontend URL |
| `GHCR_PAT` | GitHub Personal Access Token (packages: write) |
| `KUBE_CONFIG_DATA` | Base64-kódolt kubeconfig |

---

<div align="center">

Made with ❤️ by the FleetFlow Team

</div>
