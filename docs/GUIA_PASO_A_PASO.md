# GesCrédito – Guía paso a paso

Pila: **.NET 10 (LTS) · EF Core 10 · SQL Server · Angular 22 · xUnit/Moq · Docker · GitHub Actions**

## 1. Herramientas a instalar

| Herramienta | Para qué | Enlace |
|---|---|---|
| .NET SDK 10 | Compilar/ejecutar la API y las pruebas | https://dotnet.microsoft.com/download/dotnet/10.0 |
| Node.js (22.22.3+ o 24.15+) | Angular | https://nodejs.org |
| Angular CLI (opcional) | `ng serve`, `ng generate` | `npm install -g @angular/cli` |
| SQL Server Developer/Express | Base de datos local (o use Docker, ver 3-B) | https://www.microsoft.com/sql-server/sql-server-downloads |
| SSMS | Administrar SQL Server | https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms |
| Docker Desktop | Contenedores (BD + despliegue) | https://www.docker.com/products/docker-desktop/ |
| Git | Control de versiones | https://git-scm.com/downloads |
| GitHub CLI (opcional) | Crear repos/secretos desde consola | https://cli.github.com |
| Visual Studio Community o VS Code | IDE (en VS Code: extensión *C# Dev Kit*) | https://visualstudio.microsoft.com/downloads/ · https://code.visualstudio.com |

Verifique: `dotnet --version` (10.x), `node --version`, `git --version`, `docker --version`.

## 2. Estructura del proyecto

```
GesCredito/
├── GesCredito.slnx                  # solución (formato nuevo de .NET 10)
├── src/
│   ├── GesCredito_Domain/           # Entidades, enums, contratos (repositorios), cálculo de cuota
│   ├── GesCredito_Infrastructure/   # EF Core, DbContext, repositorios
│   ├── GesCredito_Services/         # Lógica de negocio, DTOs, excepciones
│   └── GesCredito_Web/              # API REST, Swagger, health check
├── client/GesCreditoApp/            # Angular (clientes, créditos)
├── tests/
│   ├── GesCredito_UnitTests/        # xUnit + Moq + EF InMemory
│   └── GesCredito_IntegrationTests/ # API completa en memoria (WebApplicationFactory)
├── scripts/01_CreateDatabase.sql    # BD + tablas + datos de prueba (idempotente)
├── docker/                          # Dockerfiles y nginx
├── docker-compose.yml               # local: SQL Server + API + Angular
├── docker-compose.prod.yml          # producción (imágenes de GHCR)
└── .github/workflows/ci.yml, cd.yml
```

Reglas de negocio implementadas:
- Cliente: identificación única; no se elimina si tiene créditos (409); se puede marcar inactivo.
- Crédito: solo para clientes activos; cuota por sistema francés; número autogenerado `CR-AAAAMMDD-XXXXXX`.
- Estados: Solicitado → Aprobado → Desembolsado → Pagado (Rechazado/Cancelado desde Solicitado; Cancelado desde Aprobado).
- Monto/tasa/plazo/cliente solo se editan en estado Solicitado; los estados finales no se modifican; un crédito Desembolsado no se elimina.

## 3. Ejecutar en local

### Opción A – SQL Server instalado
1. Abra SSMS, conéctese a `localhost` y ejecute `scripts/01_CreateDatabase.sql` (crea `GesCreditoDb` con 10 clientes y 18 créditos).
   Por consola: `sqlcmd -S localhost -U sa -P "su_clave" -C -i scripts/01_CreateDatabase.sql`
2. Edite `src/GesCredito_Web/appsettings.json` → `ConnectionStrings:GesCredito` (para autenticación de Windows:
   `Server=localhost;Database=GesCreditoDb;Trusted_Connection=True;TrustServerCertificate=True`).
   Mejor aún, guarde secretos con `dotnet user-secrets` en lugar de escribirlos en el archivo.
3. API:
   ```bash
   dotnet run --project src/GesCredito_Web
   ```
   Swagger: http://localhost:5080/swagger · Health: http://localhost:5080/health
4. Angular (otra terminal):
   ```bash
   cd client/GesCreditoApp
   npm install
   npm start          # http://localhost:4200  (el proxy envía /api a localhost:5080)
   ```

### Opción B – Todo con Docker
```bash
cp .env.example .env        # cambie SA_PASSWORD (mín. 8 car., mayúsculas, minúsculas, números y símbolos)
docker compose up --build
```
App: http://localhost:8080 · Swagger: http://localhost:5080/swagger. El servicio `db-init` ejecuta el script SQL automáticamente.

## 4. Pruebas automatizadas
```bash
dotnet test GesCredito.slnx                          # unitarias + integración
cd client/GesCreditoApp && npm test -- --watch=false # Angular (Vitest)
```
Las integraciones levantan la API real con EF InMemory (no necesitan SQL Server).

## 5. GitHub

1. Cree el repositorio vacío en https://github.com/new (privado recomendado), p. ej. `GesCredito`.
2. En la carpeta del proyecto:
   ```bash
   git init -b main
   git add .
   git commit -m "Versión inicial GesCrédito"
   git remote add origin https://github.com/SU_USUARIO/GesCredito.git
   git push -u origin main
   ```
3. Flujo recomendado: ramas `feature/*` → Pull Request a `main`. En *Settings → Branches* active la regla
   "Require status checks to pass" con los jobs `backend` y `frontend`.

## 6. GitHub Actions

- **`ci.yml`**: en cada push/PR compila, ejecuta todas las pruebas .NET y Angular y verifica el build de producción.
- **`cd.yml`**: al crear una etiqueta `vX.Y.Z` (o manualmente en *Actions → CD → Run workflow*) ejecuta pruebas, construye las
  imágenes `gescredito-api` y `gescredito-web`, las publica en GHCR (`ghcr.io/su_usuario/...`) y las despliega por SSH.

Configuración necesaria (*Settings → Secrets and variables → Actions*):

| Secreto | Valor |
|---|---|
| `SSH_HOST` | IP o dominio del servidor |
| `SSH_USER` | usuario de despliegue |
| `SSH_PRIVATE_KEY` | clave privada SSH (contenido completo) |

Y en *Settings → Environments → New environment* cree `production` (opcional: añada revisores para exigir aprobación manual).

## 7. Puesta en producción (servidor Linux con Docker)

1. **Servidor**: un VPS Ubuntu LTS (2 vCPU / 4 GB RAM mínimo; SQL Server requiere ≥ 2 GB). Cualquier proveedor (Hetzner, DigitalOcean, AWS Lightsail, Azure VM…).
2. **Preparación** (como root, una sola vez):
   ```bash
   curl -fsSL https://get.docker.com | sh
   adduser --disabled-password --gecos "" deploy && usermod -aG docker deploy
   ufw allow OpenSSH && ufw allow 80 && ufw allow 443 && ufw enable
   ```
3. **Clave SSH de despliegue** (en su PC):
   ```bash
   ssh-keygen -t ed25519 -f gescredito_deploy -C "github-actions"
   ssh-copy-id -i gescredito_deploy.pub deploy@IP_SERVIDOR
   ```
   Pegue el contenido de `gescredito_deploy` (privada) en el secreto `SSH_PRIVATE_KEY`.
4. **Variables en el servidor** (como `deploy`):
   ```bash
   mkdir -p ~/gescredito && cd ~/gescredito
   cat > .env <<'ENV'
   GHCR_OWNER=su_usuario_github_en_minusculas
   SA_PASSWORD=UnaClaveMuyLargaYSegura_2026!
   ENV
   chmod 600 .env
   ```
5. **Primer despliegue**: desde su PC
   ```bash
   git tag v1.0.0 && git push origin v1.0.0
   ```
   Siga el avance en la pestaña *Actions*. Al terminar, abra `http://IP_SERVIDOR`.
   (Las imágenes de GHCR nuevas son privadas: el workflow ya hace `docker login` con su token.)
6. **HTTPS**: apunte un dominio (registro A) al servidor y ponga Caddy delante, que obtiene certificados Let's Encrypt solos.
   Cambie en `docker-compose.prod.yml` el servicio `web` a `expose: ["80"]` (sin `ports`) y añada:
   ```yaml
     caddy:
       image: caddy:2
       restart: unless-stopped
       ports: ["80:80", "443:443"]
       volumes: ["./Caddyfile:/etc/caddy/Caddyfile", "caddy_data:/data"]
   ```
   con un `Caddyfile` de una línea: `su-dominio.com { reverse_proxy web:80 }` (y declare el volumen `caddy_data`).
7. **Actualizaciones**: nueva etiqueta (`v1.0.1`) → CD despliega solo. **Rollback**: en el servidor
   `IMAGE_TAG=v1.0.0 docker compose -f docker-compose.prod.yml up -d`.
8. **Copias de seguridad** (cron diario):
   ```bash
   docker compose -f docker-compose.prod.yml exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$SA_PASSWORD" \
     -Q "BACKUP DATABASE GesCreditoDb TO DISK='/var/opt/mssql/data/GesCreditoDb.bak' WITH INIT, COMPRESSION"
   ```
   y copie el `.bak` fuera del servidor.
9. **Monitoreo**: `docker compose logs -f api` y el endpoint `/health` (use UptimeRobot o similar).

Alternativa gestionada: Azure App Service (API) + Azure Static Web Apps/Storage (Angular) + Azure SQL Database. Las imágenes y el pipeline CI sirven igual.

## 8. Antes de exponerlo a Internet (importante)
- **La API no tiene autenticación**: añada JWT/Identity o un proveedor externo (Entra ID, Keycloak) y `[Authorize]` en los controladores.
- Elimine los datos de prueba del script SQL para producción y cree un usuario SQL de aplicación con permisos mínimos (no use `sa`).
- Guarde secretos solo en GitHub Secrets/`.env` del servidor; nunca en el repositorio.
- Restrinja CORS (`Cors:Origins`) a su dominio y deje `Swagger:Enabled` en `false`.

## 9. Ideas de siguiente paso
Migraciones EF Core en lugar del script, plan de pagos/amortización, registro de pagos, roles de usuario, reportes en Power BI, auditoría de cambios.
