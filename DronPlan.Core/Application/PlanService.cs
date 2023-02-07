using CORE.Application.Interfaces;
using CORE.Domain.Exception;
using CORE.Domain.Model;
using CORE.Domain.PlanningAlgorithm;
using CORE.DTO;
using CORE.Interfaces;
using System.Diagnostics;

namespace DronPlan.Core.Application
{
    public  class PlanService : IPlanService
    {
        private readonly IPackageRepository packageRepository;
        private readonly IPlanningAlgorithm planAlgorithn;
        public PlanService(IPackageRepository plan, IPlanningAlgorithm planAlgorithn) {
            this.packageRepository = plan;
            this.planAlgorithn=planAlgorithn;
        }

        /// <summary>
        /// converts to text format, ordered ascending by drone.Id and locantion.Id
        /// </summary>
        /// <param name="plan"></param>
        /// <returns>Delivery plan in text format</returns>
        private string ConvertPlanToText(MTripPlan plan)
        {
            try
            {
                string response = "";
                foreach (var dron in plan.drones.OrderBy(x => x.Id))
                {
                    response +=($"[Drone #{dron.Id} {dron.Name}]\n");
                    int i = 1;
                    foreach (var trip in dron.Trips)
                    {
                        response += ($"Trip #{i++}\n");
                        var c = trip.OrderBy(x=> x.Id).Select(x => $"[Location #{x.Id} {x.Location}]").ToArray();
                        response += (string.Join(", ", c));
                        response += "\n\n";
                    }
                }

                return response;

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ERROR: {ex.Message}");
                throw ex;
            }
        }


      

        /// <summary>
        /// Loading data from CSV file
        /// ASSUMTIONS: First Line contais Drone NAME/WEIGHT pairs 
        /// </summary>
        public Tuple<List<MDrone>, List<MPackage>> ParseInputData(string csvFilename)
        {
            string[] parts;
            double w;
            string line;
            int id = 1;
            int idp = 1;
            double maxweight = 0;
            var drones = new List<MDrone>();
            var packages = new List<MPackage>();
            using (var reader = new StreamReader(csvFilename))
            {


                #region  Loading Drones
                if (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    parts = line.Replace("[","").Replace("]", "").Split(',');
                    Trace.WriteLine($"Number of drones: {parts.Length}");
                    if(parts.Length/2 > 100)
                    {
                        throw new MaximumNumberOfDronesExcededExceptio($"Exceeded maximum number of drones: {parts.Length/2} > 100");
                    }    
                    for (int i = 0; i < parts.Length; i += 2)
                    {

                        if (double.TryParse(parts[i + 1], out w))
                        {
                            drones.Add(new MDrone()
                            {
                                Id = id++,
                                Name = parts[i],
                                MaxWeight = w,
                                Trips = new List<List<MPackage>>()
                            });
                            maxweight = Math.Max(maxweight, w);
                        }
                        else
                            throw new Exception($"Invalid weight at dron {i}");
                    }
                }
                #endregion


                #region Loading Packages
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();//TODO: read Async
                    parts = line.Replace("[", "").Replace("]", "").Split(',');
                    if (double.TryParse(parts[1], out w))
                    {
                        if (w > maxweight)
                        {
                            throw new Exception($"Max Weight ({maxweight}) exceeded for package {parts[0]} = {parts[1]}");
                        }
                        packages.Add(new MPackage()
                        {
                            Id = idp++,
                            Location = parts[0],
                            Weight = w,
                            
                        });
                    }
                    else
                        throw new Exception($"Invalid weight at Package {parts[0]} = {parts[1]}");
                }
                #endregion

            }
            return Tuple.Create(drones, packages);
        }

        public async Task<string> ImportFile(string filePath)
        {
            var inputData =  this.ParseInputData(filePath);
            var plan =await this.PrepareDeliveryPlan(inputData.Item1, inputData.Item2);
            var textOutput= this.ConvertPlanToText(plan);
            return textOutput;
        }

        public async Task<MTripPlan> PrepareDeliveryPlan(List<MDrone> dron, List<MPackage> pack)
        {
            var plan =  this.planAlgorithn.PrepareDeliveryPlan(dron, pack);
            await this.packageRepository.createPlan(plan);
            return plan;

        }

        #region helpers for testing history
        public async Task<List<MTripPlanDTO>> GetLast10DeliveryPlan()
        {
           return await this.packageRepository.GetLast10DeliveryPlan();
        }

        public async Task<MTripPlan> getPlan(string? id)
        {
            return await this.packageRepository.getPlan(id);
        }

        public async Task<MDrone> getDrone(string planid, int droneId)
        {
            return await this.packageRepository.getDrone(planid, droneId);
        }

        
        #endregion
    }
}
