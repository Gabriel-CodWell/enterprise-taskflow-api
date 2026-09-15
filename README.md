<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server" />
  <img src="https://img.shields.io/badge/Redis-7-DC382D?style=for-the-badge&logo=redis&logoColor=white" alt="Redis" />
  <img src="https://img.shields.io/badge/RabbitMQ-3-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white" alt="RabbitMQ" />
  <img src="https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker" />
</p>

# Enterprise.TaskFlow

> A production-ready .NET 8 Web API built with **Clean Architecture**, demonstrating enterprise patterns including CQRS, optimistic concurrency, distributed caching, and asynchronous messaging — fully containerized with Docker Compose.

---

## Table of Contents

- [Project Overview \& Architecture](#project-overview--architecture)
- [Prerequisites](#prerequisites)
- [How to Run](#how-to-run)
- [How to Test — The Tech Lead Audit](#how-to-test--the-tech-lead-audit)
- [API Endpoints](#api-endpoints)
- [Key Architectural Decisions](#key-architectural-decisions)
- [Project Structure](#project-structure)
- [Configuration Reference](#configuration-reference)
- [Troubleshooting](#troubleshooting)

---

## Project Overview & Architecture

Enterprise.TaskFlow is a task management API that implements **Clean Architecture** (also known as Onion Architecture) with strict dependency inversion between four layers:

```
┌─────────────────────────────────────────────────────────────────────┐
│                         API Layer                                   │
│          ASP.NET Core Controllers · Swagger · Health Checks         │
├─────────────────────────────────────────────────────────────────────┤
│                     Application Layer                               │
│             MediatR CQRS · Commands · Queries · Handlers            │
├─────────────────────────────────────────────────────────────────────┤
│                    Infrastructure Layer                              │
│       EF Core (SQL Server) · Redis · RabbitMQ · Repositories        │
├─────────────────────────────────────────────────────────────────────┤
│                       Domain Layer                                   │
│         Entities · Value Objects · Interfaces · Domain Events        │
└─────────────────────────────────────────────────────────────────────┘
```

**Dependency Rule:** All layers depend inward toward the **Domain**, which has zero external dependencies. The Infrastructure layer implements interfaces defined in the Domain, and the API layer orchestrates everything via Dependency Injection.

### Technology Stack

| Component | Technology | Purpose |
|:---|:---|:---|
| **Runtime** | .NET 8 (LTS) | Web API host |
| **CQRS** | MediatR 12 | Command/Query separation with pipeline support |
| **Database** | SQL Server 2022 | Persistent storage with EF Core 8 |
| **Caching** | Redis 7 | Distributed cache for read-heavy queries |
| **Messaging** | RabbitMQ 3 | Asynchronous event publishing (fanout exchange) |
| **Containerization** | Docker Compose | One-command infrastructure orchestration |
| **API Docs** | Swagger / OpenAPI 3.0 | Interactive API documentation |

---

## Prerequisites

Ensure the following tools are installed on your machine before proceeding:

| Tool | Minimum Version | Verify Command | Download |
|:---|:---|:---|:---|
| **Docker Desktop** | 24.0+ | `docker --version` | [docker.com](https://www.docker.com/products/docker-desktop/) |
| **Docker Compose** | 2.20+ (bundled with Desktop) | `docker compose version` | Included with Docker Desktop |
| **.NET SDK** *(optional — for local dev only)* | 8.0+ | `dotnet --version` | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) |

> **Note:** If you are only running via Docker Compose, the .NET SDK is **not required** — the multi-stage Dockerfile handles the build internally.

---

## How to Run

### Option 1: Docker Compose (Recommended)

This spins up the entire ecosystem — API, SQL Server, Redis, and RabbitMQ — with a single command.

```bash
# 1. Clone the repository
git clone <repository-url>
cd enterprise-taskflow-api

# 2. Build and start all services in detached mode
docker compose up -d --build
```

Monitor the startup progress:

```bash
# Watch all container statuses
docker compose ps

# Stream live logs from all services
docker compose logs -f

# Stream logs from the API container only
docker compose logs -f api
```

Wait for all containers to report `healthy` / `running` status (SQL Server may take up to 30 seconds for its health check):

```
NAME                 STATUS                   PORTS
taskflow-api         running                  0.0.0.0:5000->8080/tcp
taskflow-sqlserver   running (healthy)        0.0.0.0:1433->1433/tcp
taskflow-redis       running                  0.0.0.0:6379->6379/tcp
taskflow-rabbitmq    running (healthy)        0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
```

To stop and tear down all services:

```bash
# Stop containers (preserves volumes)
docker compose down

# Stop containers AND remove persistent data
docker compose down -v
```

### Option 2: Local Development (without Docker)

```bash
# 1. Restore dependencies
dotnet restore Enterprise.TaskFlow.sln

# 2. Build the solution
dotnet build Enterprise.TaskFlow.sln

# 3. Run the API (uses appsettings.Development.json — expects services on localhost)
dotnet run --project src/Enterprise.TaskFlow.API
```

> ⚠️ Local development requires SQL Server, Redis, and RabbitMQ running on `localhost` at their default ports. The API will start gracefully without them (Swagger will work), but data endpoints will return errors.

---

## How to Test — The Tech Lead Audit

Once all containers are running, use the following checklist to validate the system:

### 1. Swagger UI — API Documentation & Interactive Testing

| | |
|:---|:---|
| **URL** | [http://localhost:5000/swagger](http://localhost:5000/swagger) |
| **What to verify** | OpenAPI 3.0 spec loads, both endpoints are listed, "Try it out" buttons are functional |

You should see two endpoints:
- `GET /api/Tasks/pending` — Retrieves all pending tasks (served from Redis cache when available)
- `POST /api/Tasks/{id}/complete` — Marks a task as completed (publishes event to RabbitMQ)

### 2. Health Check Endpoint

| | |
|:---|:---|
| **URL** | [http://localhost:5000/health](http://localhost:5000/health) |
| **Expected Response** | `Healthy` (HTTP 200) when all services are connected |

```bash
curl http://localhost:5000/health
# Expected: Healthy
```

### 3. OpenAPI Specification (Raw JSON)

| | |
|:---|:---|
| **URL** | [http://localhost:5000/swagger/v1/swagger.json](http://localhost:5000/swagger/v1/swagger.json) |
| **What to verify** | Valid JSON with `openapi: "3.0.1"`, info block, and both path definitions |

### 4. RabbitMQ Management Dashboard

| | |
|:---|:---|
| **URL** | [http://localhost:15672](http://localhost:15672) |
| **Username** | `guest` |
| **Password** | `guest` |
| **What to verify** | Dashboard loads, `taskflow.events` exchange exists under the Exchanges tab |

After completing a task via `POST /api/Tasks/{id}/complete`, navigate to **Exchanges → taskflow.events** to confirm message activity.

### 5. SQL Server Connectivity

| | |
|:---|:---|
| **Host** | `localhost,1433` |
| **Username** | `sa` |
| **Password** | `TaskFlow@Str0ng!` |
| **Database** | `EnterpriseTaskFlowDb` |

Connect with any SQL client (Azure Data Studio, SSMS, DBeaver) to inspect the `TaskItems` table schema, including the `RowVersion` column.

### Quick Validation Script

```bash
# Verify API is responding
curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/swagger/index.html
# Expected: 200

# Verify health check
curl -s http://localhost:5000/health
# Expected: Healthy

# Verify OpenAPI spec
curl -s http://localhost:5000/swagger/v1/swagger.json | jq '.info.title'
# Expected: "Enterprise.TaskFlow API"

# Verify RabbitMQ management
curl -s -u guest:guest http://localhost:15672/api/overview | jq '.product'
# Expected: "RabbitMQ"
```

---

## API Endpoints

| Method | Route | Description | Cache |
|:---|:---|:---|:---|
| `GET` | `/api/tasks/pending` | Returns all tasks with `Pending` status | ✅ Redis (5-min TTL) |
| `POST` | `/api/tasks/{id}/complete` | Marks a task as `Completed` | Invalidates cache |
| `GET` | `/health` | System health check (SQL + Redis) | — |

---

## Key Architectural Decisions

### 🔒 Optimistic Concurrency Control

**Problem:** In a distributed system, two users might attempt to update the same task simultaneously, leading to lost updates.

**Solution:** The `TaskItem` entity includes a `RowVersion` property mapped to SQL Server's native `rowversion` / `timestamp` column type via EF Core's `.IsRowVersion()` Fluent API configuration.

```csharp
// Domain Entity
public byte[] RowVersion { get; set; }

// EF Core Configuration
builder.Property(t => t.RowVersion).IsRowVersion();
```

**How it works:** Every time a row is updated, SQL Server automatically increments the `RowVersion` value. EF Core includes the original `RowVersion` in the `WHERE` clause of `UPDATE` statements. If the value has changed since the entity was loaded (i.e., another process modified the row), a `DbUpdateConcurrencyException` is thrown — preventing silent data loss without requiring pessimistic locks.

### ⚡ Redis Caching (Cache-Aside Pattern)

**Problem:** The "Get All Pending Tasks" query is expected to be the most frequently called endpoint. Hitting the database on every request is inefficient and creates unnecessary load.

**Solution:** The `GetPendingTasksQueryHandler` implements the **cache-aside** pattern:

```
Request → Check Redis → Cache HIT  → Return cached data
                       → Cache MISS → Query SQL Server → Store in Redis (5-min TTL) → Return
```

Cache invalidation occurs automatically when a task is completed (`CompleteTaskCommandHandler` calls `ICacheService.RemoveAsync`), ensuring consistency between the cache and the database.

**Why Redis?** Redis provides sub-millisecond reads, horizontal scalability, and native TTL support — making it ideal for distributed caching in multi-instance deployments where in-memory caching would lead to stale data.

### 📨 RabbitMQ Asynchronous Events

**Problem:** When a task is completed, downstream systems (notifications, analytics, audit logs) need to be informed without blocking the API response.

**Solution:** The `RabbitMqEventPublisher` publishes a `TaskCompletedEvent` to a **durable fanout exchange** (`taskflow.events`). Messages are serialized as JSON and marked as persistent (delivery mode 2).

```
CompleteTaskCommand → Update DB → Invalidate Cache → Publish to RabbitMQ → Return 204
```

**Why Fanout?** A fanout exchange broadcasts to all bound queues without routing logic. This allows multiple independent consumers (email service, analytics pipeline, audit logger) to subscribe without any changes to the publisher — following the Open/Closed Principle.

**Why RabbitMQ over in-process events?** In-process events (like MediatR notifications) are lost if the application crashes. RabbitMQ provides message durability, retry semantics, and decouples producers from consumers across service boundaries.

---

## Project Structure

```
enterprise-taskflow-api/
│
├── Enterprise.TaskFlow.sln              # Solution file
├── docker-compose.yml                   # Full infrastructure orchestration
├── .gitignore
├── .dockerignore
├── README.md
│
├── src/
│   ├── Enterprise.TaskFlow.Domain/                     # 🟢 Zero dependencies
│   │   ├── Entities/TaskItem.cs                        # Core entity (RowVersion)
│   │   ├── Enums/TaskItemStatus.cs                     # Pending | InProgress | Completed | Cancelled
│   │   ├── Events/TaskCompletedEvent.cs                # Domain event record
│   │   └── Interfaces/                                 # ITaskRepository, ICacheService, IEventPublisher
│   │
│   ├── Enterprise.TaskFlow.Application/                # 🔵 MediatR only
│   │   ├── DependencyInjection.cs                      # Service registration
│   │   ├── Queries/GetPendingTasks/                    # Cache-aside query handler
│   │   └── Commands/CompleteTask/                      # Command handler (update + publish)
│   │
│   ├── Enterprise.TaskFlow.Infrastructure/             # 🟠 EF Core + Redis + RabbitMQ
│   │   ├── DependencyInjection.cs                      # Service registration
│   │   ├── Persistence/ApplicationDbContext.cs          # EF Core context
│   │   ├── Persistence/Configurations/                 # Fluent API (RowVersion, indexes)
│   │   ├── Repositories/TaskRepository.cs              # ITaskRepository implementation
│   │   ├── Caching/RedisCacheService.cs                # ICacheService implementation
│   │   └── Messaging/
│   │       ├── RabbitMqEventPublisher.cs                # IEventPublisher implementation
│   │       └── NoOpEventPublisher.cs                   # Fallback when RabbitMQ is unavailable
│   │
│   └── Enterprise.TaskFlow.API/                        # 🔴 Host + Swagger
│       ├── Program.cs                                  # Composition root
│       ├── Controllers/TasksController.cs              # API endpoints
│       ├── Dockerfile                                  # Multi-stage build
│       ├── appsettings.json                            # Production config
│       └── appsettings.Development.json                # Local development config
│
└── tests/                                              # Reserved for unit & integration tests
```

---

## Configuration Reference

### Connection Strings

| Key | Default (Docker) | Default (Local Dev) |
|:---|:---|:---|
| `DefaultConnection` | `Server=sqlserver;Database=EnterpriseTaskFlowDb;...` | `Server=localhost,1433;Database=EnterpriseTaskFlowDb;...` |
| `Redis` | `redis:6379` | `localhost:6379` |

### RabbitMQ Settings

| Key | Default |
|:---|:---|
| `RabbitMq:HostName` | `rabbitmq` (Docker) / `localhost` (local) |
| `RabbitMq:UserName` | `guest` |
| `RabbitMq:Password` | `guest` |
| `RabbitMq:Port` | `5672` |

All settings can be overridden via environment variables using the `__` separator (e.g., `ConnectionStrings__Redis=myredis:6380`).

---

## Troubleshooting

| Symptom | Cause | Fix |
|:---|:---|:---|
| `docker compose up` fails with "port already in use" | Another process is using port 1433, 5000, 5672, 6379, or 15672 | Stop the conflicting service or change the host port in `docker-compose.yml` |
| API returns 503 on `/health` | SQL Server or Redis is still starting | Wait 30–60 seconds for SQL Server health check to pass |
| `DbUpdateConcurrencyException` on task completion | Another process modified the task after you loaded it | Reload the entity and retry — this is **expected behavior** from optimistic concurrency |
| Swagger UI shows no endpoints | Controller DI registration failed | Check `docker compose logs api` for startup errors |
| RabbitMQ dashboard shows no `taskflow.events` exchange | The API hasn't published any events yet | Complete a task via `POST /api/tasks/{id}/complete` to trigger exchange creation |

---

<p align="center">
  Built with ❤️ using Clean Architecture principles.<br/>
  <sub>Enterprise.TaskFlow — .NET 8 · SQL Server 2022 · Redis 7 · RabbitMQ 3</sub>
</p>
