# Drone Delivery Planning System

A .NET 8 web application for optimizing drone delivery routes using a custom greedy algorithm. The system processes CSV files containing drone and package information to generate efficient delivery plans.

## 🚀 Features

- **Optimized Delivery Planning**: Custom greedy algorithm that prioritizes location grouping and drone capacity utilization
- **Web Interface**: ASP.NET Core MVC application for file upload and plan generation
- **MongoDB Integration**: Persistent storage of delivery plans with query capabilities
- **Docker Support**: Containerized deployment with Docker Compose
- **CI/CD Pipeline**: Automated testing and deployment with GitHub Actions
- **Secure Configuration**: Environment variable support for sensitive data

## 📋 Algorithm

The solution uses a custom greedy algorithm optimized for memory efficiency and performance:

1. **Sort drones** by capacity (descending) to prioritize larger drones
2. **Group packages** by location to identify delivery hotspots
3. **Sort packages** by:
   - Location frequency (ascending) - less frequent locations first
   - Grouped weight (ascending) - lighter groups first
   - Individual weight (ascending) - lighter packages first
4. **Assign packages** to drones iteratively until all packages are assigned
5. **Return** the complete delivery plan

### Key Assumptions
- Packages with the same destination should be scheduled in the same shipment when possible
- Weight values can be integers or doubles
- Maximum 100 drones per plan

### Performance Optimizations
- Replaced `List.RemoveAt()` with `HashSet` for O(1) removal performance
- Used `StringBuilder` for string concatenation
- Optimized LINQ queries for better memory usage

## 🏗️ Architecture

The solution follows Clean Architecture principles with 4 main projects:

### CORE (DronPlan.Core)
- **Domain Models**: `MDrone`, `MPackage`, `MTripPlan`
- **Business Logic**: `CustomGreedyAlgorithm`, `PlanService`
- **Interfaces**: Repository and service contracts
- **DTOs**: Data transfer objects for API communication

### Infrastructure.Persistence
- **MongoDB Integration**: Repository pattern implementation
- **Data Models**: MongoDB-specific models with BSON attributes
- **Configuration**: MongoDB connection settings

### WebApp
- **MVC Controllers**: File upload and plan management
- **Views**: Razor pages for user interface
- **Configuration**: Environment-based settings

### TestProject
- **Unit Tests**: NUnit tests with Moq for mocking
- **Test Data**: Sample CSV files for various scenarios
- **Coverage**: Algorithm validation and edge cases

## 🛠️ Technology Stack

- **.NET 8.0 LTS** - Latest long-term support framework
- **ASP.NET Core MVC** - Web framework
- **MongoDB 7.0** - NoSQL database
- **MongoDB.Driver 3.2.0** - Official .NET driver
- **NUnit 4.x** - Testing framework
- **Moq** - Mocking library
- **Docker & Docker Compose** - Containerization

## 📦 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MongoDB](https://www.mongodb.com/try/download/community) (or use Docker Compose)
- [Docker](https://www.docker.com/get-started) (optional, for containerized deployment)

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/frodorix/Dronzientp.git
   cd Dronzientp
   ```
   
   *Note: The repository name 'Dronzientp' is the actual GitHub repository name.*

2. **Configure MongoDB connection**
   
   Option A: Using environment variables (recommended)
   ```bash
   export MONGODB_CONNECTION_STRING="mongodb://localhost:27017/"
   export MONGODB_DATABASE="DroneDelivery"
   ```

   Option B: Using appsettings.json
   ```bash
   cp WebApp/appsettings.template.json WebApp/appsettings.json
   # Edit appsettings.json with your connection string
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the solution**
   ```bash
   dotnet build
   ```

5. **Run tests**
   ```bash
   dotnet test
   ```

6. **Run the application**
   ```bash
   cd WebApp
   dotnet run
   ```

   Navigate to `https://localhost:5001` or `http://localhost:5000`

### Docker Deployment

1. **Configure environment variables**
   ```bash
   cp .env.example .env
   # Edit .env and set a strong MongoDB password (minimum 12 characters with uppercase, lowercase, numbers, and symbols)
   ```
   
   **IMPORTANT**: Change the default MongoDB password in your `.env` file before deployment!
   ```
   MONGO_ROOT_USER=admin
   MONGO_ROOT_PASSWORD=YourStrongPasswordHere!123
   ```

2. **Start services with Docker Compose**
   ```bash
   docker-compose up -d
   ```

   This will start:
   - MongoDB on port 27017
   - WebApp on port 8080

3. **Access the application**
   ```
   http://localhost:8080
   ```

4. **Stop services**
   ```bash
   docker-compose down
   ```

5. **View logs**
   ```bash
   docker-compose logs -f webapp
   ```

## 📝 Input File Format

The CSV file should have the following format:

```csv
[DroneName1],MaxWeight1,[DroneName2],MaxWeight2,...
[LocationA],PackageWeight1
[LocationB],PackageWeight2
...
```

Example:
```csv
DroneA,200,DroneB,250,DroneC,100
LocationA,50
LocationB,100
LocationA,75
LocationC,25
```

## 🔒 Security

- **No hardcoded secrets**: All sensitive data is externalized
- **Environment variables**: Support for secure configuration management
- **Updated dependencies**: Latest secure versions of all packages
- **.gitignore**: Prevents accidental commit of sensitive files
- **Docker secrets**: Support for Docker secret management (future enhancement)

## 🧪 Testing

Run all tests:
```bash
dotnet test
```

Run tests with coverage:
```bash
dotnet test /p:CollectCoverage=true
```

Test files are located in `TestProject/TestData/`:
- `testdata16.csv` - Small dataset (6 trips)
- `testdata10k.csv` - Medium dataset (~7500 trips)
- `testdata20k.csv` - Large dataset (~15000 trips)
- Additional files for edge cases and validation

## 🚦 CI/CD

GitHub Actions workflow (`.github/workflows/ci-cd.yml`) includes:

- **Build and Test**: Compile and run all tests
- **Code Quality**: Static analysis and quality checks
- **Docker Build**: Validate Dockerfile
- **Security Scan**: Check for vulnerable packages

## 📊 API Endpoints

- `GET /` - Upload page
- `POST /` - Process CSV file and download delivery plan
- `GET /Plan` - View last 10 delivery plans
- `GET /Plan/DeliveryPlan/{id}` - View specific plan
- `GET /Plan/Drone/{planId}/{droneId}` - View specific drone details

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is for educational purposes.

## �� Authors

- Original Author: [frodorix](https://github.com/frodorix)
- Optimizations and modernization: Copilot Agent

## 🔄 Version History

- **2.0** (Current) - .NET 8 upgrade, performance optimizations, Docker support, CI/CD
- **1.0** - Initial release with .NET 6

## 📞 Support

For issues and questions, please use the [GitHub Issues](https://github.com/frodorix/Dronzientp/issues) page.
