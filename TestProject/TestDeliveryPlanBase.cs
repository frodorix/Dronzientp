using CORE.Application.Interfaces;
using CORE.Application.PlanningAlgorithm;
using CORE.Domain.Model;
using CORE.Domain.PlanningAlgorithm;
using CORE.Interfaces;
using DronPlan.Core.Application;
using Moq;
using System.Diagnostics;

namespace TestProject
{
    public class TestDeliveryPlanBase
    {
        public async Task<int> TestFile(string fileName)
        {
             var algorithm = new CustomGreedyAlgorithm();

            return await this.TestFile(fileName, algorithm);
        }

        public  async Task<int> TestFile(string fileName,   IPlanningAlgorithm algorithm)
        {
            Mock<IPackageRepository> planRepository = new Mock<IPackageRepository>();
            planRepository
                .Setup(x => x.createPlan(It.IsAny<MTripPlan>()))
                .ReturnsAsync("oK");

            string myCSV = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", fileName);


            IPlanService planService = new PlanService(planRepository.Object, algorithm);
            var dataTuple = planService.ParseInputData(myCSV);
            var plan = await planService.PrepareDeliveryPlan(dataTuple.Item1, dataTuple.Item2);
            var trips = plan.drones.Sum(x => x.Trips.Count);
            Trace.WriteLine($"{fileName}: {trips} trips");
            return trips;
        }
        [SetUp]
        public void Setup()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }
    }
}