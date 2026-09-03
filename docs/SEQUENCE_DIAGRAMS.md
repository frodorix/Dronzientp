# AeroRoute Sequence Diagrams

This document illustrates the execution flow for key system operations in **AeroRoute** using Mermaid sequence diagrams.

---

## 1. CSV Upload & Delivery Plan Generation Sequence

This diagram shows the end-to-end flow when a user uploads a CSV file to generate an optimized delivery plan.

```mermaid
sequenceDiagram
    autonumber
    actor User as User / Client
    participant UI as Web Dashboard
    participant Controller as DeliveryPlanController
    participant Parser as CsvParserService
    participant Planner as DeliveryPlanner
    participant Repo as ResilientDeliveryPlanRepository
    participant DB as MongoDB

    User->>UI: Upload CSV file (or select preset sample)
    UI->>Controller: POST /api/delivery/plan (multipart form)
    Controller->>Parser: ParseCsv(Stream)
    Parser-->>Controller: (Drones List, Packages List)
    
    Controller->>Planner: CreateDeliveryPlan(Drones, Packages, FileName)
    Note over Planner: 1. Sort drones by max capacity (descending)<br/>2. Flag packages > max capacity as Unassigned<br/>3. Group deliverable packages by location<br/>4. Assign trips to largest drones first
    Planner-->>Controller: DeliveryPlan object
    
    Controller->>Repo: SaveAsync(DeliveryPlan)
    alt MongoDB Available
        Repo->>DB: ReplaceOneAsync (Upsert)
        DB-->>Repo: Saved Success
    else MongoDB Unavailable / Error
        Note over Repo: Log Warning & Fallback
        Repo->>Repo: Store in InMemoryDictionary
    end
    Repo-->>Controller: Saved DeliveryPlan
    
    Controller-->>UI: 200 OK (JSON DeliveryPlan)
    UI-->>User: Render visual trip cards, stats & progress bars
```

---

## 2. Plan History Retrieval & Resilience Fallback Sequence

This diagram shows the flow when retrieving past delivery plans with automatic resilience fallback.

```mermaid
sequenceDiagram
    autonumber
    actor User as User / Client
    participant UI as Web Dashboard
    participant Controller as DeliveryPlanController
    participant Repo as ResilientDeliveryPlanRepository
    participant Mongo as MongoDeliveryPlanRepository
    participant Mem as InMemoryDeliveryPlanRepository

    User->>UI: Click "Plan History" button
    UI->>Controller: GET /api/delivery/plans?count=20
    Controller->>Repo: GetRecentPlansAsync(20)
    
    alt MongoDB Healthy
        Repo->>Mongo: GetRecentPlansAsync(20)
        Mongo-->>Repo: List of DeliveryPlans
    else MongoDB Exception / Offline
        Note over Repo: Catch Exception & Fallback
        Repo->>Mem: GetRecentPlansAsync(20)
        Mem-->>Repo: List of DeliveryPlans
    end
    
    Repo-->>Controller: List of DeliveryPlans
    Controller-->>UI: 200 OK (JSON Array)
    UI-->>User: Render History Drawer items
```
