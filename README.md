# VidaApp – Infraestructura

Repositorio de infraestructura del proyecto VidaApp.

- Docker / Docker Compose
- NGINX
- Backend API
- Base de datos
- Seguridad first (SELinux, firewall, least privilege)

Repositorio gestionado desde GitHub.

# VidaApp - estructura servidor

/opt/vidaapp
  infra/        -> Docker Compose, config, flyway scripts
  data/         -> persistencia (mysql), backups
  logs/         -> logs operativos del host/servicios
  repos/        -> clones de repos GitLab (opcional)
  secrets/      -> secretos fuera de git (permisos estrictos)

Notas:
- No subir "secrets/" a Git.
- "data/" debe persistir y respaldarse.
# VidaApp – Infraestructura

## 1. Visión general

VidaApp se despliega en un único servidor Linux (AlmaLinux) usando Docker.
La arquitectura prioriza:
- simplicidad
- control explícito de red
- mínima superficie expuesta
- separación clara entre servicios públicos e internos

---

## 2. Servidor

- Sistema operativo: AlmaLinux
- Firewall: firewalld (activo)
- SELinux: Enforcing
- Docker: Docker Engine + Docker Compose
- Dominio: jlean92.es

---

## 3. Servicios desplegados

### 3.1 VidaApp (API)

- Tecnología: Python / FastAPI
- Ejecución: Docker
- Puerto interno: 8000 (solo red Docker / localhost)
- Puerto público: 80
- Acceso:
  - http://jlean92.es

Uso:
- Consumido por clientes Windows (WinUI) y Android
- Punto único de entrada para la aplicación

---

### 3.2 Base de datos (MySQL)

- Motor: MySQL 8.x
- Ejecución: Docker
- Puerto interno: 3306
- Persistencia: volumen Docker
- Migraciones: Flyway

Seguridad:
- Acceso restringido por firewall (IP allowlist)
- Usuario de aplicación separado (`vidaapp`)
- Usuario root limitado (sin `%`)

---

### 3.3 Vaultwarden

- Servicio externo al proyecto VidaApp
- Ejecución: Docker
- Puertos públicos:
  - 81 (HTTP)
  - 4431 (HTTPS)
- Uso:
  - Gestión de credenciales personales
  - No accesible desde VidaApp

---

## 4. Puertos abiertos (firewall)

| Puerto | Servicio       | Motivo |
|------:|---------------|--------|
| 80    | VidaApp       | Acceso clientes Windows / Android |
| 81    | Vaultwarden   | Acceso web |
| 4431  | Vaultwarden   | HTTPS |
| 3306  | MySQL         | Acceso administrativo restringido |

Todos los demás puertos permanecen cerrados.

---

## 5. Seguridad

### 5.1 Firewall
- Política por defecto: deny
- Puertos abiertos explícitamente
- MySQL accesible solo desde IPs autorizadas

### 5.2 SELinux
- Modo: Enforcing
- Puertos no estándar (ej. 4431) habilitados explícitamente
- No se desactiva SELinux

### 5.3 MySQL
- Usuario `vidaapp` limitado a hosts concretos
- `root` sin acceso remoto global
- Defensa en profundidad: firewall + permisos DB

---

## 6. Red Docker

- Red bridge dedicada: `vidaapp_net`
- Comunicación interna por nombre de servicio
- Ningún contenedor accede directamente a Internet salvo por puertos publicados

---

## 7. Decisiones conscientes

- No se usa GraphQL (API REST)
- No se expone MySQL a Internet
- No se usa HTTPS en VidaApp todavía (fase inicial)
- No se centraliza todo en un único reverse proxy por simplicidad inicial

Estas decisiones se revisarán cuando VidaApp tenga usuarios reales.

---

## 8. Próximos pasos previstos

- Cerrar completamente MySQL al exterior
- HTTPS para VidaApp
- Autenticación JWT
- Sincronización offline-first
- Monitorización y backups

