using System;
using System.Collections.Generic;
using UnityEngine;


public class ResourceSimulator
{
    public class ResourceConfig
    {
        public string ResourceId { get; set; }
        public double TargetAmount { get; set; } = 1000;
        public double TargetTime { get; set; } = 36000; // 10 hours in seconds
        public double rarityFactor { get; set; } = 1.0; // A:1, B:3, C:10
        public double Price { get; set; } = 1.0; // A:1, B:5, C:20
        public double P0 { get; set; } = 0.0278; // Initial production rate
        public double m { get; set; } = 0.1; // Upgrade multiplier
        public double k { get; set; } = 1.5; // Cost growth rate
        public double CostBase { get; set; } = 100; // Base upgrade cost
        public int MaxLevel { get; set; } = 10; // Max upgrades
    }

    public void Simulate(List<ResourceConfig> resources)
    {
        foreach (var res in resources)
        {
            res.P0 = res.TargetAmount / res.TargetTime / res.rarityFactor;
            res.CostBase *= res.rarityFactor;
            SimulateResource(res);
        }
    }

    private void SimulateResource(ResourceConfig res)
    {
        double currentTime = 0;
        double accumulatedA = 0;
        double currentP = res.P0;
        int level = 0;
        double currentGold = 0;

        List<(double Time, double A, double P)> data = new List<(double, double, double)>();
        data.Add((currentTime, accumulatedA, currentP));

        while (accumulatedA < res.TargetAmount && level < res.MaxLevel)
        {
            double cost = res.CostBase * Math.Pow(res.k, level);
            double timeToNextUpgrade = cost / (currentP * res.Price); // Time to produce enough A for Gold
            accumulatedA += currentP * timeToNextUpgrade;
            currentGold += currentP * timeToNextUpgrade * res.Price; // A converted to Gold
            currentTime += timeToNextUpgrade;

            if (currentGold >= cost)
            {
                currentGold -= cost;
                accumulatedA -= cost / res.Price; // A sold for Gold
                level++;
                currentP = res.P0 * (1 + level * res.m);
            }

            data.Add((currentTime, accumulatedA, currentP));
            if (currentTime > res.TargetTime) break;
        }

        // Output for this resource
        Debug.Log($"\nResource: {res.ResourceId}");
        Debug.Log($"Time to reach {res.TargetAmount} A: {currentTime:F2} seconds");
        Debug.Log($"Number of upgrades: {level}");
        Debug.Log($"Final production rate: {currentP:F2}/sec");
        Debug.Log($"Final Gold: {currentGold:F2}");

        Debug.Log("\nASCII Graph: Time vs Accumulated A (with sales for upgrades)\n");

        foreach (var point in data)
        {
            Debug.Log($"t={point.Time:F2}".PadRight(10) + " | " + $"{point.A:F0}".PadRight(4) + $" (P={point.P:F3}/sec)");
        }

        Debug.Log("\nUpgrade Production Rate Changes:");
        for (int l = 0; l <= level; l++)
        {
            double p = res.P0 * (1 + l * res.m);
            Debug.Log($"Upgrade {l}: {p:F3}/sec");
        }
    }
}



public class GameEconomyGrokSimulator
{
    public void Run()
    {
        var resources = new List<ResourceSimulator.ResourceConfig>
        {
            new ResourceSimulator.ResourceConfig { ResourceId = "A", TargetAmount = 1000, rarityFactor = 1, Price = 1, CostBase = 100, P0 = 0.0278 },
            new ResourceSimulator.ResourceConfig { ResourceId = "B", TargetAmount = 600, rarityFactor = 3, Price = 5, CostBase = 150, P0 = 0.0167 },
            new ResourceSimulator.ResourceConfig { ResourceId = "C", TargetAmount = 200, rarityFactor = 10, Price = 20, CostBase = 200, P0 = 0.0056 }
        };
        ResourceSimulator simulator = new ResourceSimulator();
        simulator.Simulate(resources);
    }
}

// Sim Result.
/*
자원 ID	최종 Level	누적 시간 (시간)	최종 속도 (Units/sec)	총 누적 수량 (Units)
Resource_A	20	        ≈13.9	            ≈38.00	            ≈460,000
Resource_B	20	        ≈15.0	            ≈2.20	            ≈50,000
Resource_C	20	        ≈17.5	            ≈0.50	            ≈120,000
*/