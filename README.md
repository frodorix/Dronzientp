#Drone Delivery Service
A squad of drones is tasked with delivering packages for a major online retailer in a
world where time and distance do not matter.
Each drone can carry a specific weight and can make multiple deliveries before
returning to home base to pick up additional packages; however, the goal is to make
the fewest number of trips, as each time the drone returns to home base, it is
extremely costly to refuel and reload the drone.
The software shall accept input which will include: the name of each drone, the
maximum weight it can carry, along with a series of locations and the total weight
needed to be delivered to that specific location. 

The software should highlight the most
efficient deliveries for each drone to make on each trip.

Assume that time and distance to each location do not matter, and that the size of
each package is irrelevant. It is also assumed that the cost to refuel and restock each
drone is a constant and does not vary between drones.
The maximum number of drones in a squad is 100, and there is no maximum number
of required deliveries.

- time: do not matter
- distance: do not matter.
- size of each package is irrelevant
- maximum number of drones in a squad: 100
- goal: is to make the fewest number of trips



#Given Input
Line 1: [Drone #1 Name], [#1 Maximum Weight], [Drone #2 Name], [#2 Maximum Weight], etc.
Line 2: [Location #1 Name], [Location #1 Package Weight]
Line 3: [Location #2 Name], [Location #2 Package Weight]
Line 4: [Location #3 Name], [Location #3 Package Weight]
Etc.

#Expected Output

[Drone #1 Name]
Trip #1
[Location #2 Name], [Location #3 Name]
Trip #2
[Location #1 Name]
[Drone #2 Name]
Trip #1
[Location #4 Name], [Location #7 Name]
Trip #2
[Location #5 Name], [Location #6 Name]

#Algorithm

The algorith used is a custom version of Greedy.
1. Sort drones by higger capacity desc.
2. Group packages by location
3. Sort packages by:
	- grouped locations Count in ascending mode
	- grouped locations TotalWeight in ascending mode
	- package Weight in ascending mode
4. while packages list is not empty, iterate over results of step 3
4.1. For each drone
	- add packages to the a new trip, while  ramaining drone.maxWeight >= package.weight
	- when a package is added to a trip, remove it from te list of packages.
	- if the trip if full, add it to de drone and continue to the next drone
5. return results

#Approach

Assumtions: 
- if a group of packages have the same destination, they should be scheduled in the same shipment, if possible.
- weights are integer values


The solution imports a text file throught a file Upload in a MVC web application.
The uploaded file is parsed and both lists, drone and packages, are sorted to assign first the smaller packages to the bigger drones, grouping the locations and allowing to send the packages of the same destination in the same shipment

Once the file is input file is processed, the delivery plan is stored in a MongoDB database and a new file is sent as a file download response

#Projects

The solution is composed of four projects/modules: CORE, Infrastructure.Persistence, TestProject, WebApp

- CORE
implements the core logic for the delivery planning process. 
Defines business models, interfaces, services .
Also implements an extensión which pepares the module to be used as a dependency injection in the WebApp project

There is a interface "IPlanningAlgorithm"  which declares one function  "PrepareDeliveryPlan(List<MDrone> drone, List<MPackage> packages)".
This interface is implemented by CustomGreedyAlgorithm and is injected through the extension "ConfigureCore". The core funcionality is implemented here

- Infrastructure.Persistence
Implements database access to MongoDB using repository pattern to implement interfaces defined by the Core module.
Also implements an extensión which pepares the module to be used as a dependency injection in the WebApp project

-TestProject
Implements test cases for de CORE module using mocks to simulate the implementacion of the repositories.
Sample files are provided in TestProject/TestData/

- WebApp
Implements a dafault MVC web applicacion which allows to upload a file with the required input data, according to de format specified. 
Sample files are provided in the test project



#Technical Dependencies and Libraries


- Microsoft.Extensions.DependencyInjection.Abstracions:
I use IServiceCollection to provide an extension which configures implemented services for dependency injection. This is implemented in project CORE/Extensions/ConfigureCore.cs

- Microsoft.Extensions.Options 
Used to provide access to the context condigurations.

- MongoDB.Driver:
Used in the persistence project to connect to a remote mongoDb database.

-Moq:
Used in test project to create mocks of the persistence module.


Dependencies are injected in WebApp.Program.cs using the extensions defined in each module:
	- For Core: builder.Services.UseInfrastructurePersistence();
	- For Persistence: builder.Services.UseCoreServices();


