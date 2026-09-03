# AeroRoute - Autonomous Drone Delivery Planner

A high-performance **.NET 10** web application designed for automated drone delivery dispatch planning from uploaded CSV files.

![AeroRoute Web App](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet) ![MongoDB](https://img.shields.io/badge/Database-MongoDB-47A248?logo=mongodb) ![Docker](https://img.shields.io/badge/Container-Docker-2496ED?logo=docker)

## Key Features

- **Capacity-First Routing**: Sorts available drones by payload capacity (descending) to prioritize using the largest drones first.
- **Same-Location Package Clustering**: Consolidates packages destined for the same location into combined trips to minimize total flights and optimize payload utilization.
- **High-Performance Streaming CSV Parser**: Handles standard drone delivery format (`DroneA, 200... LocationA, 100...`) and tabular format (`Type, Name, Weight, Location`), remaining fast even for 10,000+ packages.
- **MongoDB Persistence & Resilient Fallback**: Stores generated delivery plans in MongoDB. Features automatic fallback to an in-memory repository if MongoDB is offline.
- **Modern Web Dashboard**: Glassmorphism UI with live metrics, drag-and-drop file upload, trip visualizer, and searchable history drawer.

---

## Quickstart

### 1. Run Locally with .NET SDK

```bash
# Build the solution
dotnet build

# Run unit tests
dotnet test

# Run Bash & Curl End-to-End (E2E) Test Suite
./scripts/e2e_test.sh

# Start the web application
dotnet run --project src/DroneDeliveryApp.Web
```

Open your browser at `http://localhost:5000` or `http://localhost:5197`.

---

### 2. Run with Docker Compose

```bash
docker compose up --build
```

This starts both **AeroRoute Web App** (`http://localhost:8080`) and **MongoDB** (`mongodb://localhost:27017`).

---

## CSV File Formats Supported

### Format 1: Standard Drone Format
```csv
DroneA, 200, DroneB, 250, DroneC, 100
LocationA, 200
LocationB, 150
LocationB, 50
LocationD, 150
LocationE, 100
LocationA, 10
```

### Format 2: Tabular Format
```csv
Type,Name,Weight,Location
Drone,Drone Alpha,500,
Drone,Drone Beta,300,
Package,Pkg-1,200,Location A
Package,Pkg-2,150,Location B
Package,Pkg-3,50,Location B
```

---

## Testing & Quality Assurance

### Unit Tests
```bash
dotnet test
```

### End-to-End (E2E) Integration Tests
```bash
./scripts/e2e_test.sh
```

The E2E test script automatically launches the server if needed, tests sample CSV generation, performs standard & tabular file uploads, queries plan history, retrieves plan details, tests overweight package edge cases, and verifies plan deletions via `curl`.
