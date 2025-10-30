# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY DronZient.sln ./
COPY DronPlan.Core/CORE.csproj ./DronPlan.Core/
COPY Infrastructure.Persistence/Infrastructure.Persistence.csproj ./Infrastructure.Persistence/
COPY WebApp/WebApp.csproj ./WebApp/
COPY TestProject/TestProject.csproj ./TestProject/

# Restore dependencies
RUN dotnet restore

# Copy remaining source code
COPY . .

# Build the application
WORKDIR /src/WebApp
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads && chmod 777 /app/wwwroot/uploads

ENTRYPOINT ["dotnet", "WebApp.dll"]
