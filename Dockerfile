# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and project files
COPY DroneDeliveryApp.slnx ./
COPY src/DroneDeliveryApp.Core/DroneDeliveryApp.Core.csproj src/DroneDeliveryApp.Core/
COPY src/DroneDeliveryApp.Infrastructure/DroneDeliveryApp.Infrastructure.csproj src/DroneDeliveryApp.Infrastructure/
COPY src/DroneDeliveryApp.Web/DroneDeliveryApp.Web.csproj src/DroneDeliveryApp.Web/
COPY tests/DroneDeliveryApp.Tests/DroneDeliveryApp.Tests.csproj tests/DroneDeliveryApp.Tests/

# Restore dependencies
RUN dotnet restore src/DroneDeliveryApp.Web/DroneDeliveryApp.Web.csproj

# Copy source code and publish
COPY . .
WORKDIR /app/src/DroneDeliveryApp.Web
RUN dotnet publish -c Release -o /out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 80

ENTRYPOINT ["dotnet", "DroneDeliveryApp.Web.dll"]
