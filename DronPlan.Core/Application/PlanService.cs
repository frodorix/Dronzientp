using CORE.Application.Interfaces;
using CORE.Domain.Exception;
using CORE.Domain.Model;
using CORE.Domain.PlanningAlgorithm;
using CORE.DTO;
using CORE.Interfaces;
using System.Diagnostics;

namespace DronPlan.Core.Application
{
    /// <summary>
    /// Service for managing delivery plans, including parsing input data, creating plans, and converting to output format.
    /// </summary>
    public class PlanService : IPlanService
    {
        private readonly IPackageRepository packageRepository;
        private readonly IPlanningAlgorithm planAlgorithm;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanService"/> class.
        /// </summary>
        /// <param name="plan">Repository for persisting trip plans</param>
        /// <param name="planAlgorithm">Algorithm for creating delivery plans</param>
        public PlanService(IPackageRepository plan, IPlanningAlgorithm planAlgorithm)
        {
            this.packageRepository = plan;
            this.planAlgorithm = planAlgorithm;
        }

        /// <summary>
        /// Converts a trip plan to text format, ordered by drone ID and location ID.
        /// </summary>
        /// <param name="plan">The trip plan to convert</param>
        /// <returns>Delivery plan in text format</returns>
        private static string ConvertPlanToText(MTripPlan plan)
        {
            try
            {
                var response = new System.Text.StringBuilder();
                
                foreach (var drone in plan.drones.OrderBy(x => x.Id))
                {
                    response.AppendLine($"[Drone #{drone.Id} {drone.Name}]");
                    int tripNumber = 1;
                    
                    foreach (var trip in drone.Trips)
                    {
                        response.AppendLine($"Trip #{tripNumber++}");
                        var locations = trip.OrderBy(x => x.Id)
                                           .Select(x => $"[Location #{x.Id} {x.Location}]")
                                           .ToArray();
                        response.AppendLine(string.Join(", ", locations));
                        response.AppendLine();
                    }
                }

                return response.ToString();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ERROR: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Parses input data from a CSV file containing drone and package information.
        /// </summary>
        /// <param name="csvFilename">Path to the CSV file</param>
        /// <returns>Tuple containing lists of drones and packages</returns>
        /// <exception cref="MaximumNumberOfDronesExcededExceptio">Thrown when the file contains more than 100 drones</exception>
        /// <exception cref="Exception">Thrown when the file format is invalid or contains invalid data</exception>
        /// <remarks>
        /// Expected format:
        /// - First line: Drone NAME/WEIGHT pairs (e.g., "DroneA,100,DroneB,150")
        /// - Subsequent lines: Location,Weight pairs (e.g., "LocationA,50")
        /// </remarks>
        public Tuple<List<MDrone>, List<MPackage>> ParseInputData(string csvFilename)
        {
            string[] parts;
            double weight;
            int droneId = 1;
            int packageId = 1;
            double maxWeight = 0;
            var drones = new List<MDrone>();
            var packages = new List<MPackage>();

            using (var reader = new StreamReader(csvFilename))
            {
                #region Loading Drones
                if (!reader.EndOfStream)
                {
                    var line = reader.ReadLine() ?? string.Empty;
                    parts = line.Replace("[", "").Replace("]", "").Split(',');
                    
                    Trace.WriteLine($"Number of drones: {parts.Length / 2}");
                    
                    if (parts.Length / 2 > 100)
                    {
                        throw new MaximumNumberOfDronesExcededExceptio(
                            $"Exceeded maximum number of drones: {parts.Length / 2} > 100");
                    }

                    for (int i = 0; i < parts.Length; i += 2)
                    {
                        if (double.TryParse(parts[i + 1], out weight))
                        {
                            drones.Add(new MDrone
                            {
                                Id = droneId++,
                                Name = parts[i],
                                MaxWeight = weight,
                                Trips = new List<List<MPackage>>()
                            });
                            maxWeight = Math.Max(maxWeight, weight);
                        }
                        else
                        {
                            throw new Exception($"Invalid weight at drone {i}: {parts[i + 1]}");
                        }
                    }
                }
                #endregion

                #region Loading Packages
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine() ?? string.Empty;
                    parts = line.Replace("[", "").Replace("]", "").Split(',');
                    
                    if (double.TryParse(parts[1], out weight))
                    {
                        if (weight > maxWeight)
                        {
                            throw new Exception(
                                $"Max Weight ({maxWeight}) exceeded for package {parts[0]} = {parts[1]}");
                        }
                        
                        packages.Add(new MPackage
                        {
                            Id = packageId++,
                            Location = parts[0],
                            Weight = weight
                        });
                    }
                    else
                    {
                        throw new Exception($"Invalid weight at Package {parts[0]} = {parts[1]}");
                    }
                }
                #endregion
            }

            return Tuple.Create(drones, packages);
        }

        /// <summary>
        /// Imports a CSV file, creates a delivery plan, and returns the plan as formatted text.
        /// </summary>
        /// <param name="filePath">Path to the CSV file to import</param>
        /// <returns>Formatted text representation of the delivery plan</returns>
        public async Task<string> ImportFile(string filePath)
        {
            var inputData = this.ParseInputData(filePath);
            var plan = await this.PrepareDeliveryPlan(inputData.Item1, inputData.Item2);
            var textOutput = ConvertPlanToText(plan);
            return textOutput;
        }

        /// <summary>
        /// Prepares a delivery plan for the given drones and packages, then persists it to the database.
        /// </summary>
        /// <param name="drones">List of available drones</param>
        /// <param name="packages">List of packages to deliver</param>
        /// <returns>The created trip plan</returns>
        public async Task<MTripPlan> PrepareDeliveryPlan(List<MDrone> drones, List<MPackage> packages)
        {
            var plan = this.planAlgorithm.PrepareDeliveryPlan(drones, packages);
            await this.packageRepository.createPlan(plan);
            return plan;
        }

        #region History Helpers
        
        /// <summary>
        /// Retrieves the last 10 delivery plans from the database.
        /// </summary>
        /// <returns>List of trip plan summaries</returns>
        public async Task<List<MTripPlanDTO>> GetLast10DeliveryPlan()
        {
            return await this.packageRepository.GetLast10DeliveryPlan();
        }

        /// <summary>
        /// Retrieves a specific delivery plan by ID.
        /// </summary>
        /// <param name="id">The plan ID</param>
        /// <returns>The trip plan, or null if not found</returns>
        public async Task<MTripPlan?> getPlan(string? id)
        {
            return await this.packageRepository.getPlan(id);
        }

        /// <summary>
        /// Retrieves a specific drone from a delivery plan.
        /// </summary>
        /// <param name="planId">The plan ID</param>
        /// <param name="droneId">The drone ID</param>
        /// <returns>The drone with its assigned trips, or null if not found</returns>
        public async Task<MDrone?> getDrone(string planId, int droneId)
        {
            return await this.packageRepository.getDrone(planId, droneId);
        }

        #endregion
    }
}
