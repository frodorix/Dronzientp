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


