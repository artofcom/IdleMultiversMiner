using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using GD = IGCore.Simulator.GameData;

namespace IGCore.Simulator
{
    public class GameSimulation : MonoBehaviour
    {
        [Header("Simulation Settings")]
        [SerializeField] bool autoRun = false;
        [SerializeField] string gameDataPath = "Data/Simulator/gameData";
    
        [Header("Events")]
        public System.Action<string> OnSimulationComplete;
        public System.Action<string> OnSkillComplete;
    
        private GD.GameData gameData;
        private HashSet<int> unlockedPlanets = new HashSet<int>();
    
        void Start()
        {
            LoadGameData();
            InitializeUnlockedPlanets();
            if (autoRun)
            {
                RunSimulation();
            }
        }
    
        void LoadGameData()
        {
            string jsonString = Resources.Load<TextAsset>(gameDataPath).text;
            gameData = JsonUtility.FromJson<GD.GameData>(jsonString);
            Debug.Log("Game data loaded successfully");
        }
    
        void InitializeUnlockedPlanets()
        {
            // Start with planet 1 unlocked
            unlockedPlanets.Add(1);
            Debug.Log("Initial unlocked planets: planet_1");
        }
    
        [ContextMenu("Run Simulation")]
        public void RunSimulation()
        {
            Debug.Log("=== SKILL TREE COMPLETION SIMULATION (NO SKILL EFFECTS) ===");
        
            BigInteger totalTimeSeconds = 0;
        
            // Process each skill sequentially
            for (int i = 0; i < gameData.skillTree.Count; i++)
            {
                var skill = gameData.skillTree[i];
                var requirement = skill.sources[0];
                var amount = BigInteger.Parse(requirement.amount);
            
                Debug.Log($"\n--- Skill {i+1}: {skill.id} ({skill.feature}) ---");
                Debug.Log($"Required: {amount:N0} {requirement.resourceId}");
            
                // Unlock planets for this skill
                foreach (var planetId in skill.unlockPlanets)
                {
                    unlockedPlanets.Add(planetId);
                    Debug.Log($"🔓 Unlocked: planet_{planetId}");
                }
            
                // Calculate materials needed (item → comp → mat conversion)
                BigInteger materialsNeeded = amount * 10; // Simplified: all items need mat_9
            
                // Calculate time WITHOUT skill multiplier
                BigInteger skillTime = CalculateTimeWithPlanets(materialsNeeded, 1.0); // No skill effects
                totalTimeSeconds += skillTime;
            
                Debug.Log($"Materials: {materialsNeeded:N0} mat_9");
                Debug.Log($"Unlocked planets: {string.Join(", ", unlockedPlanets)}");
                Debug.Log($"Time: {skillTime:N0}s ({(double)skillTime / 3600.0:F1}h)");
                Debug.Log($"Cumulative: {totalTimeSeconds:N0}s ({(double)totalTimeSeconds / 3600.0:F1}h)");
            }
        
            double totalDays = (double)totalTimeSeconds / 86400.0;
        
            Debug.Log("\n=== FINAL RESULTS ===");
            Debug.Log($"🎯 TOTAL COMPLETION TIME: {totalDays:F1} DAYS");
            Debug.Log($"🔓 Final unlocked planets: {string.Join(", ", unlockedPlanets)}");
            Debug.Log($"⚡ Skill effects: DISABLED");
        
            // Trigger completion event
            string resultMessage = $"🎯 TOTAL COMPLETION TIME: {totalDays:F1} DAYS";
            OnSimulationComplete?.Invoke(resultMessage);
        
            // Save to file
            SaveResults(totalDays, 1.0);
        }
    
        BigInteger CalculateTimeWithPlanets(BigInteger materialsNeeded, double multiplier)
        {
            // Calculate total production from all unlocked planets
            double totalProduction = 0.0;
        
            foreach (var planetId in unlockedPlanets)
            {
                var planet = gameData.planets.Find(p => p.id == $"planet_{planetId}");
                if (planet != null)
                {
                    // Base production from this planet (level 1)
                    double planetProduction = CalculatePlanetProduction(planet);
                    totalProduction += planetProduction;
                }
            }
        
            // Apply skill multiplier
            double effectiveProduction = totalProduction * multiplier;
        
            if (effectiveProduction <= 0)
            {
                Debug.LogWarning("No production available!");
                return BigInteger.Zero;
            }
        
            double timeSeconds = (double)materialsNeeded / effectiveProduction;
            return new BigInteger(Math.Ceiling(timeSeconds));
        }
    
        double CalculatePlanetProduction(GD.Planet planet)
        {
            // Calculate production for mat_9 from this planet
            float performance = planet.performances.GetValue(1); // Level 1
            float rate = 0;
        
            foreach (var obtainable in planet.obtainables)
            {
                if (obtainable.resourceId == "mat_9")
                {
                    rate = obtainable.rate;
                    break;
                }
            }
        
            return rate + (performance * 0.001f);
        }
    
        void SaveResults(double totalDays, double finalMultiplier)
        {
            string result = $"Skill Tree Completion Time: {totalDays:F1} days\nFinal Multiplier: {finalMultiplier:F2}x\nUnlocked Planets: {string.Join(", ", unlockedPlanets)}";
            string path = System.IO.Path.Combine(Application.dataPath, "..", "simulation_result.txt");
        
            try
            {
                System.IO.File.WriteAllText(path, result);
                Debug.Log($"Results saved to: {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save results: {e.Message}");
            }
        }
    }
}