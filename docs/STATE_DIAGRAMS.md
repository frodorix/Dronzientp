# AeroRoute State Diagrams

This document defines the lifecycle states for Delivery Plans and Repository Resilience Modes in **AeroRoute** using Mermaid state diagrams.

---

## 1. Delivery Plan & Package Routing Lifecycle State Diagram

This state diagram tracks how CSV inputs transition from raw text to scheduled drone delivery trips or unassigned warnings.

```mermaid
stateDiagram-v2
    [*] --> RawCsvInput : File Upload / Form Submit

    state RawCsvInput {
        [*] --> LineTokenization
        LineTokenization --> FormatDetection : Check Header & Brackets
        FormatDetection --> ParsedEntities : Parse Drones & Packages
    }

    ParsedEntities --> CapacityValidation

    state CapacityValidation {
        [*] --> CheckMaxCapacity
        CheckMaxCapacity --> DeliverablePackage : Weight <= MaxDroneCapacity
        CheckMaxCapacity --> OverweightPackage : Weight > MaxDroneCapacity
    }

    OverweightPackage --> UnassignedState : Reason: Exceeds Max Capacity

    DeliverablePackage --> LocationClustering : Group by Destination

    state LocationClustering {
        [*] --> PrioritizePrimaryLocation
        PrioritizePrimaryLocation --> PackPrimaryPackages : Fill Drone Payload
        PackPrimaryPackages --> FillRemainingSpace : Secondary Locations
        FillRemainingSpace --> TripCreated : Assign to Largest Available Drone
    }

    TripCreated --> PlanAggregated

    state PlanAggregated {
        [*] --> ComputeUtilization
        ComputeUtilization --> PersistedInDb : Save Plan
    }

    PersistedInDb --> [*]
```

---

## 2. Repository Resilience & Storage Mode State Diagram

This state diagram depicts how `ResilientDeliveryPlanRepository` dynamically manages database connection states between MongoDB and In-Memory fallback mode.

```mermaid
stateDiagram-v2
    [*] --> Initializing : Application Startup

    state Initializing {
        [*] --> CheckConnectionString
        CheckConnectionString --> AttemptMongoConnection : ConnectionString Provided
        CheckConnectionString --> UseInMemoryFallback : ConnectionString Empty
    }

    AttemptMongoConnection --> MongoActiveMode : Ping / Driver Ready
    AttemptMongoConnection --> MongoDegradedMode : Driver Init Exception

    state MongoActiveMode {
        [*] --> ReadWriteMongo
        ReadWriteMongo --> ReadWriteMongo : Normal Operations
    }

    MongoActiveMode --> MongoDegradedMode : MongoDB Connection Timeout / Loss

    state MongoDegradedMode {
        [*] --> FallbackToInMemory
        FallbackToInMemory --> ReadWriteMemory : Serve Requests
    }

    ReadWriteMemory --> [*]
```
