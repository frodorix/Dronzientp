# AeroRoute Deployment Model & Infrastructure

This document details the container deployment model for **AeroRoute**, based exclusively on **Docker & Docker Compose**.

---

## 🐳 Docker Compose Architecture Diagram

```mermaid
graph TD
    UserClient["Client Web Browser / HTTP Requests"] -->|Port 8080:80| WebContainer["AeroRoute Web App Container (aeroroute-web)"]
    
    subgraph Docker Network ["aeroroute-network (Bridge)"]
        WebContainer -->|DNS mongo:27017| MongoContainer["MongoDB Container (aeroroute-mongo)"]
        
        subgraph WebContainerDetails ["Web App Service"]
            DotnetRuntime[".NET 10 ASP.NET Core Runtime"]
            StaticUI["Static Web Assets (wwwroot)"]
            AppConfig["Env: ConnectionStrings__MongoDB"]
            HealthEndpoint["Healthcheck: /healthz"]
        end
        
        subgraph MongoContainerDetails ["Database Service"]
            MongoEngine["MongoDB 7.0 Engine"]
            VolMount["Volume: mongo_data -> /data/db"]
        end
    end

    VolMount --> HostStorage[("Host Persistent Disk Storage")]
```

---

## ⚙️ Container Configuration Details

### 1. Web Application Container (`aeroroute-web`)
- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:10.0`
- **Internal Port**: `80`
- **External Port Mapping**: `8080:80`
- **Environment Settings**:
  - `ASPNETCORE_ENVIRONMENT=Production`
  - `ConnectionStrings__MongoDB=mongodb://mongo:27017`
  - `MongoDbSettings__DatabaseName=DroneDeliveryDb`
  - `MongoDbSettings__CollectionName=DeliveryPlans`

### 2. MongoDB Container (`aeroroute-mongo`)
- **Image**: `mongo:latest`
- **Internal Port**: `27017`
- **External Port Mapping**: `27017:27017`
- **Volume Mount**: `mongo_data:/data/db`

---

## 🚀 Docker Production Container Topology Diagram

```mermaid
graph LR
    ReverseProxy["Docker NGINX Proxy / Edge Gateway"] -->|HTTP / TLS| WebContainer1["AeroRoute Web Container 1"]
    ReverseProxy -->|HTTP / TLS| WebContainer2["AeroRoute Web Container 2"]
    
    subgraph Docker Engine ["Docker Engine Host"]
        WebContainer1 --> MongoContainer["MongoDB Container"]
        WebContainer2 --> MongoContainer
    end

    MongoContainer --> VolMount[("Docker Volume (mongo_data)")]
```

---

## 🛡️ Docker Container Health Checks & Operations

AeroRoute integrates Docker native container health checks utilizing the `/healthz` HTTP endpoint:

### Docker Compose Healthcheck Configuration
```yaml
services:
  web:
    build: .
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/healthz"]
      interval: 15s
      timeout: 5s
      retries: 3
      start_period: 10s
```

### Production Docker Container Guidelines
1. **Secrets Management**: Pass sensitive connection strings using Docker Compose `.env` files or Docker Secrets.
2. **Container Monitoring**: Monitor container health status using `docker ps` and native Docker healthcheck statuses.
3. **Data Persistence**: Always use named Docker volumes (`mongo_data`) to prevent data loss across container recreations.
