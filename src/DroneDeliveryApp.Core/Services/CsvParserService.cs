using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DroneDeliveryApp.Core.Interfaces;
using DroneDeliveryApp.Core.Models;

namespace DroneDeliveryApp.Core.Services;

public class CsvParserService : ICsvParserService
{
    public (List<Drone> Drones, List<Package> Packages) ParseCsv(Stream csvStream)
    {
        using var reader = new StreamReader(csvStream, Encoding.UTF8, leaveOpen: true);
        var content = reader.ReadToEnd();
        return ParseCsv(content);
    }

    public (List<Drone> Drones, List<Package> Packages) ParseCsv(string csvContent)
    {
        var drones = new List<Drone>();
        var packages = new List<Package>();

        if (string.IsNullOrWhiteSpace(csvContent))
        {
            return (drones, packages);
        }

        var rawLines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var cleanLines = rawLines
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#"))
            .ToList();

        if (cleanLines.Count == 0)
        {
            return (drones, packages);
        }

        // Check if the CSV is in standard tabular header format: "Type, Name, Weight, Location"
        var firstLineTokens = TokenizeLine(cleanLines[0]);
        bool isTabularWithHeader = firstLineTokens.Count >= 3 &&
            firstLineTokens[0].Equals("Type", StringComparison.OrdinalIgnoreCase) &&
            firstLineTokens.Any(t => t.Equals("Weight", StringComparison.OrdinalIgnoreCase));

        if (isTabularWithHeader)
        {
            ParseTabularFormat(cleanLines.Skip(1), drones, packages);
        }
        else
        {
            ParseLineByLineFormat(cleanLines, drones, packages);
        }

        return (drones, packages);
    }

    private static void ParseLineByLineFormat(List<string> lines, List<Drone> drones, List<Package> packages)
    {
        int packageCounter = 1;

        // Line 1 contains Drones: [DroneA], [200], [DroneB], [250]...
        var droneTokens = TokenizeLine(lines[0]);
        for (int i = 0; i < droneTokens.Count - 1; i += 2)
        {
            string name = CleanToken(droneTokens[i]);
            string capacityStr = CleanToken(droneTokens[i + 1]);

            if (double.TryParse(capacityStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double capacity) && capacity > 0)
            {
                drones.Add(new Drone(name, capacity));
            }
        }

        // Subsequent lines (Line 2 to end) contain Packages/Locations: [LocationA], [200]
        for (int lineIdx = 1; lineIdx < lines.Count; lineIdx++)
        {
            var tokens = TokenizeLine(lines[lineIdx]);
            if (tokens.Count < 2) continue;

            // Check if this line is in format "Drone, Name, Capacity" or "Package, Location, Weight"
            if (tokens[0].Equals("Drone", StringComparison.OrdinalIgnoreCase) && tokens.Count >= 3)
            {
                string name = CleanToken(tokens[1]);
                if (double.TryParse(CleanToken(tokens[2]), NumberStyles.Any, CultureInfo.InvariantCulture, out double cap))
                {
                    drones.Add(new Drone(name, cap));
                }
                continue;
            }

            for (int i = 0; i < tokens.Count - 1; i += 2)
            {
                string loc = CleanToken(tokens[i]);
                string weightStr = CleanToken(tokens[i + 1]);

                if (double.TryParse(weightStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double weight) && weight > 0)
                {
                    packages.Add(new Package(loc, weight, $"Pkg-{packageCounter++}"));
                }
            }
        }
    }

    private static void ParseTabularFormat(IEnumerable<string> lines, List<Drone> drones, List<Package> packages)
    {
        int packageCounter = 1;
        foreach (var line in lines)
        {
            var tokens = TokenizeLine(line);
            if (tokens.Count < 3) continue;

            string type = CleanToken(tokens[0]);
            string nameOrLoc = CleanToken(tokens[1]);
            string weightOrCapStr = CleanToken(tokens[2]);

            if (type.Equals("Drone", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(weightOrCapStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double cap) && cap > 0)
                {
                    drones.Add(new Drone(nameOrLoc, cap));
                }
            }
            else
            {
                string location = tokens.Count >= 4 && !string.IsNullOrWhiteSpace(tokens[3]) ? CleanToken(tokens[3]) : nameOrLoc;
                if (double.TryParse(weightOrCapStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double weight) && weight > 0)
                {
                    packages.Add(new Package(location, weight, $"Pkg-{packageCounter++}"));
                }
            }
        }
    }

    private static List<string> TokenizeLine(string line)
    {
        // Split by comma while preserving tokens inside brackets or quotes
        var tokens = line.Split(',')
            .Select(CleanToken)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();

        return tokens;
    }

    private static string CleanToken(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken)) return string.Empty;
        var token = rawToken.Trim();
        // Remove surrounding square brackets '[' ']' or quotes '"'
        token = token.Trim('[', ']', '"', '\'');
        return token.Trim();
    }
}
