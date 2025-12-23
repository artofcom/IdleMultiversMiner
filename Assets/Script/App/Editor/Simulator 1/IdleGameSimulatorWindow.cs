using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace App.Tools.Simulator
{
    /*
    public class IdleGameSimulatorWindow : EditorWindow
    {
        [Serializable]
        public class SimulationResult
        {
            public double totalHours;
            public BigInteger totalCoins;
            public int miningLevel;
            public int deliveryLevel;
            public int cargoLevel;
            public string log;
            public bool goalAchieved;
            public double completionTime;
            public int unlockedSkills;
        }

        // Game Data Structures
        public class GameData
        {
            public PlanetData planetData;
            public ResourceData materialPrices;
            public ResourceData componentPrices;
            public ResourceData itemPrices;
            public CraftData compCraftData;
            public CraftData itemCraftData;
            public SkillTreeData miningSkills;
            public SkillTreeData compCraftSkills;
            public SkillTreeData itemCraftSkills;
            public SkillTreeData goalSkills;
        }

        public class PlanetData
        {
            public List<PlanetZoneGroup> data;
        }

        public class PlanetZoneGroup
        {
            public int zoneId;
            public List<PlanetZoneData> planets;
        }

        public class PlanetZoneData
        {
            public int id;
            public string name;
            public string openCost;
            public List<Obtainable> obtainables;
            public string miningRate;
            public string shipSpeed;
            public string cargoSize;
            public string shotAccuracy;
            public string shotInterval;
            public string miningCost;
            public string shipCost;
            public string cargoCost;
            public string shotAccuracyCost;
            public string shotIntervalCost;
        }

        public class Obtainable
        {
            public string resourceId;
            public float yield;
        }

        public class ResourceData
        {
            public List<ResourceInfo> data;
        }

        public class ResourceInfo
        {
            public string id;
            public string price;
        }

        public class CraftData
        {
            public List<CraftRecipe> recipes;
            public List<string> slotCosts;
        }

        public class CraftRecipe
        {
            public string id;
            public List<ResourceRequirement> sources;
            public string outcomeId;
            public int duration;
            public string cost;
        }

        public class ResourceRequirement
        {
            public string resourceId;
            public int count;
        }

        public class SkillTreeData
        {
            public string id;
            public string rootId;
            public List<SkillInfo> skillInfoPool;
        }

        public class SkillInfo
        {
            public string name;
            public string id;
            public string abilityId;
            public string abilityParam;
            public string description;
            public List<ResourceRequirement> unlockCost;
            public List<string> children;
        }

        public class SimulationState
        {
            public double currentTime;
            public BigInteger totalCoins;
            public Dictionary<string, BigInteger> resources = new Dictionary<string, BigInteger>();
            public HashSet<string> unlockedSkills = new HashSet<string>();
            public Dictionary<string, int> planetUpgradeLevels = new Dictionary<string, int>();
            public Dictionary<string, List<string>> skillPaths = new Dictionary<string, List<string>>();
            public int currentZone = 0;
            public int currentPlanet = 0;
            public Dictionary<string, double> skillCompletionTimes = new Dictionary<string, double>();
            public Dictionary<string, double> zoneBuffCooldowns = new Dictionary<string, double>();
            public Dictionary<string, float> craftingInProgress = new Dictionary<string, float>();
            public float totalMiningEfficiency = 1.0f;
            public float totalCraftEfficiency = 1.0f;
        }

        UnityEngine.Vector2 scroll;

        [MenuItem("PlasticGames/Simulator/Idle Game Simulator")] 
        public static void Open()
        {
            var win = GetWindow<IdleGameSimulatorWindow>(utility: false, title: "Idle Game Simulator");
            win.minSize = new UnityEngine.Vector2(460, 520);
            win.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Idle Game Simulator", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("This simulator uses real game data from Resources/Bundles/G010_Graves/Data/", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Run Simulation"))
            {
                var res = RunSimulation();
                ShowResultDialog(res);
            }
        }

        static SimulationResult RunSimulation()
        {
            var sb = new StringBuilder(1024);
            
            try
            {
                Debug.Log("[Simulator] Starting simulation with real game data...");
                
                // Load real game data
                var gameData = LoadGameData();
                if (gameData == null)
                {
                    return new SimulationResult
                    {
                        totalHours = 0.0,
                        totalCoins = BigInteger.Zero,
                        miningLevel = 1,
                        deliveryLevel = 1,
                        cargoLevel = 1,
                        log = "Failed to load game data",
                        goalAchieved = false,
                        completionTime = 0.0,
                        unlockedSkills = 0
                    };
                }
                
                // Run simulation
                var result = RunFullSimulation(gameData, sb);
                
                Debug.Log($"[Simulator] Simulation completed: {result.totalHours:F1} hours, Goal achieved: {result.goalAchieved}");
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Simulator] Simulation failed: {e.Message}");
                return new SimulationResult
                {
                    totalHours = 0.0,
                    totalCoins = BigInteger.Zero,
                    miningLevel = 1,
                    deliveryLevel = 1,
                    cargoLevel = 1,
                    log = $"Simulation failed: {e.Message}",
                    goalAchieved = false,
                    completionTime = 0.0,
                    unlockedSkills = 0
                };
            }
        }
        
        static GameData LoadGameData()
        {
            try
            {
                var gameData = new GameData();
                
                // Load planet data
                string planetJson = Resources.Load<TextAsset>("Bundles/G010_Graves/Data/PlanetData").text;
                gameData.planetData = JsonConvert.DeserializeObject<PlanetData>(planetJson);
                
                // Load resource prices
                string materialJson = Resources.Load<TextAsset>("Bundles/G010_Graves/Data/Resource_Mat").text;
                gameData.materialPrices = JsonConvert.DeserializeObject<ResourceData>(materialJson);
                
                string componentJson = Resources.Load<TextAsset>("Bundles/G010_Graves/Data/Resource_Comp").text;
                gameData.componentPrices = JsonConvert.DeserializeObject<ResourceData>(componentJson);
                
                string itemJson = Resources.Load<TextAsset>("Bundles/G010_Graves/Data/Resource_Item").text;
                gameData.itemPrices = JsonConvert.DeserializeObject<ResourceData>(itemJson);
                
                // Load craft data
                string compCraftJson = Resources.Load<TextAsset>("Bundles/G010_Graves/Data/Craft_Comp").text;
                gameData.compCraftData = JsonConvert.DeserializeObject<CraftData>(compCraftJson);
                
                string itemCraftJson = Resources.Load<TextAsset>("Bundles/G010_Graves/Data/Craft_Item").text;
                gameData.itemCraftData = JsonConvert.DeserializeObject<CraftData>(itemCraftJson);
                
                // Load skill trees
                string miningSkillsJson = Resources.Load<TextAsset>("Bundles/G010_Graves/Data/Skill_Mining").text;
                gameData.miningSkills = JsonConvert.DeserializeObject<SkillTreeData>(miningSkillsJson);
                
                string compCraftSkillsJson = Resources.Load<TextAsset>("Bundles/G010_Graves/Data/Skill_CompCraft").text;
                gameData.compCraftSkills = JsonConvert.DeserializeObject<SkillTreeData>(compCraftSkillsJson);
                
                string itemCraftSkillsJson = Resources.Load<TextAsset>("Bundles/G010_Graves/Data/Skill_ItemCraft").text;
                gameData.itemCraftSkills = JsonConvert.DeserializeObject<SkillTreeData>(itemCraftSkillsJson);
                
                string goalSkillsJson = Resources.Load<TextAsset>("Bundles/G010_Graves/Data/Skill_Goal").text;
                gameData.goalSkills = JsonConvert.DeserializeObject<SkillTreeData>(goalSkillsJson);
                
                Debug.Log("[Simulator] Game data loaded successfully");
                return gameData;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Simulator] Failed to load game data: {e.Message}");
                return null;
            }
        }
        
        static SimulationResult RunFullSimulation(GameData data, StringBuilder sb)
        {
            var state = new SimulationState();
            
            // Initialize simulation
            InitializeSkillPaths(data, state);
            InitializePlanetUpgradeLevels(data, state);
            
            // Start with first planet unlocked (Zone 100, Planet 1)
            state.currentZone = 0; // Zone 100 is at index 0
            state.currentPlanet = 0; // Planet 1 is at index 0
            
            sb.AppendLine("=== Starting Full Game Simulation ===");
            sb.AppendLine($"Loaded data: {data.planetData.data.Count} zones");
            sb.AppendLine($"Goal: Complete {data.goalSkills.skillInfoPool[0].id}");
            
            // Debug: Check skill tree root IDs
            sb.AppendLine($"Mining rootId: {data.miningSkills.rootId}");
            sb.AppendLine($"CompCraft rootId: {data.compCraftSkills.rootId}");
            sb.AppendLine($"ItemCraft rootId: {data.itemCraftSkills.rootId}");
            sb.AppendLine($"Goal rootId: {data.goalSkills.rootId}");
            
            // Run simulation for 10 days maximum
            double targetTime = 10.0 * 24.0; // 10 days in hours
            double timeStep = 1.0; // 1 hour (increased for better efficiency)
            
            sb.AppendLine($"Running simulation for {targetTime} hours with {timeStep:F2} hour steps");
            
            // Create log file
            string logPath = System.IO.Path.Combine(Application.dataPath, "..", "SimulationLog.txt");
            using (var logWriter = new System.IO.StreamWriter(logPath))
            {
                logWriter.WriteLine($"=== Simulation Log - {System.DateTime.Now} ===");
                logWriter.WriteLine($"Target Time: {targetTime} hours");
                logWriter.WriteLine($"Time Step: {timeStep} hours");
                logWriter.WriteLine();
            
                for (double time = 0; time < targetTime; time += timeStep)
                {
                    state.currentTime = time;
                    
                    // Process mining, crafting, skills, upgrades
                    ProcessMining(data, state, timeStep, logWriter);
                    
                    // Process ongoing crafts first (reduce time)
                    ProcessOngoingCrafts(state, timeStep, logWriter);
                    
                    // Process crafting (only if we have available slots)
                    if (state.craftingInProgress.Count < 3) // Max 3 crafting slots
                    {
                        ProcessCrafting(data, state, timeStep, logWriter);
                    }
                    
                    ProcessSkillUnlocking(data, state, logWriter);
                    ProcessUpgrades(data, state);
                    
                    // Check if goal is achieved
                    if (state.unlockedSkills.Contains("GoalSection_0_Final"))
                    {
                        string goalMessage = $"Goal achieved at {time:F1} hours!";
                        sb.AppendLine(goalMessage);
                        logWriter.WriteLine(goalMessage);
                        break;
                    }
                    
                    // Debug: Log first few hours
                    if (time < 5.0 && Math.Floor(time) % 1 == 0)
                    {
                        BigInteger totalResources = BigInteger.Zero;
                        foreach (var resource in state.resources.Values)
                        {
                            totalResources += resource;
                        }
                        string hourLog = $"Hour {time:F1}: Coins={state.totalCoins}, Skills={state.unlockedSkills.Count}, Resources={totalResources}";
                        sb.AppendLine(hourLog);
                        logWriter.WriteLine(hourLog);
                        
                        // Show first few resources
                        int count = 0;
                        foreach (var resource in state.resources)
                        {
                            if (count < 3)
                            {
                                string resourceLog = $"  {resource.Key}: {resource.Value}";
                                sb.AppendLine(resourceLog);
                                logWriter.WriteLine(resourceLog);
                                count++;
                            }
                        }
                    }
                    
                    // Log progress every 24 hours
                    if (Math.Floor(time) % 24 == 0 && time > 0)
                    {
                        BigInteger totalResources = BigInteger.Zero;
                        foreach (var resource in state.resources.Values)
                        {
                            totalResources += resource;
                        }
                        string dayLog = $"Day {Math.Floor(time / 24) + 1}: Coins={state.totalCoins}, Skills={state.unlockedSkills.Count}, Resources={totalResources}";
                        sb.AppendLine(dayLog);
                        logWriter.WriteLine(dayLog);
                    }
                }
                
                // Final log
                logWriter.WriteLine();
                logWriter.WriteLine($"=== Simulation Complete ===");
                logWriter.WriteLine($"Total Time: {state.currentTime:F1} hours");
                logWriter.WriteLine($"Goal Achieved: {state.unlockedSkills.Contains("GoalSection_0_Final")}");
                logWriter.WriteLine($"Total Coins: {state.totalCoins}");
                logWriter.WriteLine($"Unlocked Skills: {state.unlockedSkills.Count}");
                
                BigInteger finalResources = BigInteger.Zero;
                foreach (var resource in state.resources.Values)
                {
                    finalResources += resource;
                }
                logWriter.WriteLine($"Total Resources: {finalResources}");
                logWriter.WriteLine();
                logWriter.WriteLine("=== Resource Details ===");
                foreach (var resource in state.resources)
                {
                    logWriter.WriteLine($"{resource.Key}: {resource.Value}");
                }
            }
            
            // Calculate results
            bool goalAchieved = state.unlockedSkills.Contains("GoalSection_0_Final");
            double completionTime = goalAchieved ? state.currentTime : targetTime;
            
            return new SimulationResult
            {
                totalHours = state.currentTime,
                totalCoins = state.totalCoins,
                miningLevel = 1, // Simplified for now
                deliveryLevel = 1,
                cargoLevel = 1,
                log = sb.ToString(),
                goalAchieved = goalAchieved,
                completionTime = completionTime,
                unlockedSkills = state.unlockedSkills.Count
            };
        }

        // Core simulation methods
        static void InitializeSkillPaths(GameData data, SimulationState state)
        {
            // Build skill paths for each tree
            BuildSkillPath(data.miningSkills, state);
            BuildSkillPath(data.compCraftSkills, state);
            BuildSkillPath(data.itemCraftSkills, state);
            BuildSkillPath(data.goalSkills, state);
        }
        
        static void BuildSkillPath(SkillTreeData skillTree, SimulationState state)
        {
            if (skillTree?.skillInfoPool == null) return;
            
            var path = new List<string>();
            BuildPathFromRoot(skillTree.rootId, skillTree.skillInfoPool, path);
            state.skillPaths[skillTree.id] = path;
            
            Debug.Log($"[Simulator] Built {skillTree.id} skill path with {path.Count} skills: {string.Join(" -> ", path)}");
        }
        
        static void BuildPathFromRoot(string rootId, List<SkillInfo> skillPool, List<string> path)
        {
            if (string.IsNullOrEmpty(rootId)) return;
            
            var skill = skillPool.FirstOrDefault(s => s.id == rootId);
            if (skill == null) 
            {
                Debug.LogWarning($"[Simulator] Skill not found: {rootId}");
                return;
            }
            
            path.Add(rootId);
            
            // Follow ALL children (not just first) to build complete paths
            if (skill.children != null && skill.children.Count > 0)
            {
                foreach (var childId in skill.children)
                {
                    BuildPathFromRoot(childId, skillPool, path);
                }
            }
        }
        
        static void InitializePlanetUpgradeLevels(GameData data, SimulationState state)
        {
            foreach (var zone in data.planetData.data)
            {
                foreach (var planet in zone.planets)
                {
                    string planetKey = $"{zone.zoneId}_{planet.id}";
                    state.planetUpgradeLevels[planetKey] = 1;
                }
            }
        }
        
        static int FindOptimalPlanet(PlanetZoneGroup zone, SimulationState state, GameData data)
        {
            int bestPlanet = 0;
            float bestEfficiency = 0f;
            
            for (int i = 0; i < zone.planets.Count; i++)
            {
                var planet = zone.planets[i];
                string planetKey = $"{zone.zoneId}_{planet.id}";
                int upgradeLevel = state.planetUpgradeLevels.ContainsKey(planetKey) ? state.planetUpgradeLevels[planetKey] : 1;
                
                // Calculate mining efficiency
                var miningRate = ParseLevelBasedFloat(planet.miningRate);
                var shipSpeed = ParseLevelBasedFloat(planet.shipSpeed);
                var cargoSize = ParseLevelBasedFloat(planet.cargoSize);
                
                float efficiency = miningRate.Value(upgradeLevel) * shipSpeed.Value(upgradeLevel) * cargoSize.Value(upgradeLevel);
                
                // Prioritize planets with needed resources
                float priority = efficiency;
                foreach (var obtainable in planet.obtainables)
                {
                    // Check if this resource is needed for current skills
                    if (IsResourceNeededForSkills(obtainable.resourceId, state, data))
                    {
                        priority *= 2.0f; // Double priority for needed resources
                    }
                }
                
                if (priority > bestEfficiency)
                {
                    bestEfficiency = priority;
                    bestPlanet = i;
                }
            }
            
            return bestPlanet;
        }
        
        static bool IsResourceNeededForSkills(string resourceId, SimulationState state, GameData data)
        {
            // Check if this resource is needed for any unlocked skills
            foreach (var skillPath in state.skillPaths.Values)
            {
                foreach (var skillId in skillPath)
                {
                    if (!state.unlockedSkills.Contains(skillId))
                    {
                        // This skill is not unlocked yet, check if it needs this resource
                        var skillInfo = GetSkillInfo(skillId, data);
                        if (skillInfo != null && skillInfo.unlockCost != null)
                        {
                            foreach (var cost in skillInfo.unlockCost)
                            {
                                if (cost.resourceId == resourceId)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        
        static SkillInfo GetSkillInfo(string skillId, GameData data)
        {
            // Search through all skill trees to find the skill
            var allSkills = new List<SkillInfo>();
            allSkills.AddRange(data.miningSkills.skillInfoPool);
            allSkills.AddRange(data.compCraftSkills.skillInfoPool);
            allSkills.AddRange(data.itemCraftSkills.skillInfoPool);
            allSkills.AddRange(data.goalSkills.skillInfoPool);
            
            return allSkills.FirstOrDefault(s => s.id == skillId);
        }
        
        static void ProcessMining(GameData data, SimulationState state, double timeStep, System.IO.StreamWriter logWriter)
        {
            if (state.currentZone >= data.planetData.data.Count) 
            {
                Debug.LogWarning($"[Simulator] Current zone {state.currentZone} >= total zones {data.planetData.data.Count}");
                return;
            }
            
            var currentZone = data.planetData.data[state.currentZone];
            
            // Find optimal planet in current zone
            int optimalPlanet = FindOptimalPlanet(currentZone, state, data);
            state.currentPlanet = optimalPlanet;
            
            if (state.currentPlanet >= currentZone.planets.Count) 
            {
                Debug.LogWarning($"[Simulator] Current planet {state.currentPlanet} >= total planets {currentZone.planets.Count} in zone {currentZone.zoneId}");
                return;
            }
            
            var currentPlanet = currentZone.planets[state.currentPlanet];
            string planetKey = $"{currentZone.zoneId}_{currentPlanet.id}";
            
            int upgradeLevel = state.planetUpgradeLevels.ContainsKey(planetKey) ? state.planetUpgradeLevels[planetKey] : 1;
            
            // Parse LevelBasedFloat strings
            var miningRate = ParseLevelBasedFloat(currentPlanet.miningRate);
            var shipSpeed = ParseLevelBasedFloat(currentPlanet.shipSpeed);
            var cargoSize = ParseLevelBasedFloat(currentPlanet.cargoSize);
            
            float miningEfficiency = miningRate.Value(upgradeLevel) * shipSpeed.Value(upgradeLevel) * cargoSize.Value(upgradeLevel);
            
            // Debug first mining calculation
            if (state.currentTime < 5.0) // Log first few hours
            {
                Debug.Log($"[Simulator] Mining Debug - Zone: {state.currentZone}, Planet: {state.currentPlanet}");
                Debug.Log($"[Simulator] Current Planet: {currentPlanet.name} (ID: {currentPlanet.id})");
                Debug.Log($"[Simulator] Mining Rate: {miningRate.Value(upgradeLevel):F2}, Ship Speed: {shipSpeed.Value(upgradeLevel):F2}, Cargo Size: {cargoSize.Value(upgradeLevel):F2}");
                Debug.Log($"[Simulator] Mining Efficiency: {miningEfficiency:F2}, Time Step: {timeStep:F2}");
                Debug.Log($"[Simulator] Available Resources: {currentPlanet.obtainables.Count}");
                foreach (var obtainable in currentPlanet.obtainables)
                {
                    Debug.Log($"[Simulator]   - {obtainable.resourceId}: yield={obtainable.yield:F2}");
                }
            }
            
            // Apply zone buff if available
            string zoneKey = currentZone.zoneId.ToString();
            if (state.zoneBuffCooldowns.ContainsKey(zoneKey) && state.zoneBuffCooldowns[zoneKey] > 0)
            {
                miningEfficiency *= 10.0f; // 10x buff
                state.zoneBuffCooldowns[zoneKey] -= timeStep;
            }
            
        foreach (var obtainable in currentPlanet.obtainables)
        {
            float yield = obtainable.yield * miningEfficiency * (float)timeStep;
            int collected = Mathf.FloorToInt(yield);
            
            // Ensure minimum collection even with low efficiency
            if (collected == 0 && yield > 0.01f)
            {
                collected = 1; // At least collect 1 if there's any yield
            }
            
            // Debug resource collection calculation
            if (state.currentTime < 5.0)
            {
                Debug.Log($"[Simulator] Resource Calculation: {obtainable.resourceId}");
                Debug.Log($"[Simulator]   Base Yield: {obtainable.yield:F2}");
                Debug.Log($"[Simulator]   Mining Efficiency: {miningEfficiency:F2}");
                Debug.Log($"[Simulator]   Time Step: {timeStep:F2}");
                Debug.Log($"[Simulator]   Calculated Yield: {yield:F2}");
                Debug.Log($"[Simulator]   Collected: {collected}");
            }
            
            if (collected > 0)
            {
                if (!state.resources.ContainsKey(obtainable.resourceId))
                    state.resources[obtainable.resourceId] = 0;
                
                state.resources[obtainable.resourceId] += collected;
                
                // Convert to coins
                var resourcePrice = data.materialPrices.data.FirstOrDefault(r => r.id == obtainable.resourceId);
                if (resourcePrice != null)
                {
                    int price = int.Parse(resourcePrice.price);
                    state.totalCoins += collected * price;
                    
                    string collectLog = $"[Simulator] Collected {collected} {obtainable.resourceId} (yield: {obtainable.yield:F2}) worth {collected * price} coins";
                    Debug.Log(collectLog);
                    logWriter.WriteLine(collectLog);
                }
                else
                {
                    Debug.LogWarning($"[Simulator] No price found for resource: {obtainable.resourceId}");
                }
            }
            else
            {
                if (state.currentTime < 5.0)
                {
                    Debug.Log($"[Simulator] No resources collected for {obtainable.resourceId} (yield too low: {yield:F2})");
                }
            }
        }
            
            state.totalMiningEfficiency = miningEfficiency;
        }
        
        static void ProcessCrafting(GameData data, SimulationState state, double timeStep, System.IO.StreamWriter logWriter)
        {
            // Process component crafting (3 slots)
            ProcessCraftingType(data.compCraftData, state, timeStep, 3, data, logWriter);
            
            // Process item crafting (3 slots)
            ProcessCraftingType(data.itemCraftData, state, timeStep, 3, data, logWriter);
        }
        
        static void ProcessOngoingCrafts(SimulationState state, double timeStep, System.IO.StreamWriter logWriter)
        {
            var completedCrafts = new List<string>();
            
            // Debug: Log ongoing crafts
            if (state.currentTime < 15.0 && state.craftingInProgress.Count > 0)
            {
                string ongoingLog = $"[Simulator] Ongoing crafts at {state.currentTime:F1} hours: {state.craftingInProgress.Count}";
                Debug.Log(ongoingLog);
                logWriter.WriteLine(ongoingLog);
                
                foreach (var craft in state.craftingInProgress)
                {
                    string craftLog = $"[Simulator]   - {craft.Key}: {craft.Value:F1} hours remaining";
                    Debug.Log(craftLog);
                    logWriter.WriteLine(craftLog);
                }
            }
            
            // Create a copy of keys to avoid modification during enumeration
            var craftKeys = new List<string>(state.craftingInProgress.Keys);
            
            foreach (var craftKey in craftKeys)
            {
                if (state.craftingInProgress.ContainsKey(craftKey))
                {
                    float oldTime = state.craftingInProgress[craftKey];
                    state.craftingInProgress[craftKey] -= (float)timeStep;
                    float newTime = state.craftingInProgress[craftKey];
                    
                    // Debug: Log time reduction
                    if (state.currentTime < 15.0)
                    {
                        string timeLog = $"[Simulator] Craft {craftKey}: {oldTime:F1} -> {newTime:F1} hours";
                        Debug.Log(timeLog);
                        logWriter.WriteLine(timeLog);
                    }
                    
                    if (state.craftingInProgress[craftKey] <= 0)
                    {
                        // Craft completed
                        if (!state.resources.ContainsKey(craftKey))
                            state.resources[craftKey] = 0;
                        
                        state.resources[craftKey]++;
                        completedCrafts.Add(craftKey);
                        
                        // Debug: Log completed crafts
                        string completeLog = $"[Simulator] Completed crafting {craftKey} at {state.currentTime:F1} hours";
                        Debug.Log(completeLog);
                        logWriter.WriteLine(completeLog);
                    }
                }
            }
            
            // Remove completed crafts
            foreach (var completed in completedCrafts)
            {
                state.craftingInProgress.Remove(completed);
            }
        }
        
        static void ProcessCraftingType(CraftData craftData, SimulationState state, double timeStep, int maxSlots, GameData data, System.IO.StreamWriter logWriter)
        {
            // Debug: Log current skill progress
            var unlockableSkills = GetUnlockableSkills(state, data, logWriter);
            var nextLevelSkills = GetNextLevelUnlockableSkills(state, data, logWriter);
            
            logWriter.WriteLine($"[Debug] Unlockable skills: {string.Join(", ", unlockableSkills)}");
            logWriter.WriteLine($"[Debug] Next level skills: {string.Join(", ", nextLevelSkills)}");
            
            // Check if current ongoing crafts are still needed
            var craftsToStop = new List<string>();
            foreach (var ongoingCraft in state.craftingInProgress.Keys.ToList())
            {
                if (!IsCraftStillNeeded(ongoingCraft, state, data))
                {
                    craftsToStop.Add(ongoingCraft);
                    logWriter.WriteLine($"[Debug] Stopping unnecessary craft: {ongoingCraft}");
                }
            }
            
            // Stop unnecessary crafts
            foreach (var craftId in craftsToStop)
            {
                state.craftingInProgress.Remove(craftId);
            }
            
            // Analyze current skill tree progress and prioritize accordingly
            var prioritizedRecipes = PrioritizeRecipesBySkillProgress(craftData.recipes, state, data, logWriter);
            
            logWriter.WriteLine($"[Debug] Prioritized recipes: {string.Join(", ", prioritizedRecipes.Select(r => r.outcomeId))}");
            
            int activeSlots = state.craftingInProgress.Count;
            
            // Start crafting recipes based on skill tree priority
            foreach (var recipe in prioritizedRecipes)
            {
                if (activeSlots >= maxSlots) break;
                
                if (!state.craftingInProgress.ContainsKey(recipe.outcomeId) && CanCraft(recipe, state))
                {
                    StartCrafting(recipe, state, data, logWriter);
                    activeSlots++;
                }
            }
        }
        
        static void StartCrafting(CraftRecipe recipe, SimulationState state, GameData data, System.IO.StreamWriter logWriter)
        {
            // Apply skill buffs
            float timeMultiplier = GetCraftTimeMultiplier(recipe, state);
            float resourceMultiplier = GetCraftResourceMultiplier(recipe, state);
            
            // Calculate craft time
            float craftTime = recipe.duration * timeMultiplier;
            
            // Consume resources
            foreach (var source in recipe.sources)
            {
                int actualCount = Mathf.RoundToInt(source.count * resourceMultiplier);
                if (state.resources.ContainsKey(source.resourceId))
                {
                    BigInteger newValue = state.resources[source.resourceId] - actualCount;
                    state.resources[source.resourceId] = newValue < 0 ? BigInteger.Zero : newValue;
                }
            }
            
            // Start crafting
            state.craftingInProgress[recipe.outcomeId] = craftTime;
            
            // Debug: Log first few crafts
            if (state.currentTime < 5.0)
            {
                string craftLog = $"[Simulator] Started crafting {recipe.outcomeId} at {state.currentTime:F1} hours (duration: {craftTime:F1})";
                Debug.Log(craftLog);
                logWriter.WriteLine(craftLog);
            }
        }
        
        static bool IsCraftStillNeeded(string craftId, SimulationState state, GameData data)
        {
            // Check if this craft is needed for any unlockable skills
            var skillTrees = new[] { data.miningSkills, data.compCraftSkills, data.itemCraftSkills, data.goalSkills };
            
            foreach (var skillTree in skillTrees)
            {
                if (skillTree?.skillInfoPool == null) continue;
                
                foreach (var skillInfo in skillTree.skillInfoPool)
                {
                    if (!state.unlockedSkills.Contains(skillInfo.id) && skillInfo.unlockCost != null)
                    {
                        // Check if prerequisites are met
                        bool prerequisitesMet = true;
                        if (skillInfo.id != skillTree.rootId)
                        {
                            var parentSkill = skillTree.skillInfoPool.FirstOrDefault(s => 
                                s.children != null && s.children.Contains(skillInfo.id));
                            if (parentSkill != null && !state.unlockedSkills.Contains(parentSkill.id))
                            {
                                prerequisitesMet = false;
                            }
                        }
                        
                        if (prerequisitesMet)
                        {
                            foreach (var cost in skillInfo.unlockCost)
                            {
                                if (cost.resourceId == craftId)
                                {
                                    int currentCount = state.resources.ContainsKey(cost.resourceId) ? (int)state.resources[cost.resourceId] : 0;
                                    if (currentCount < cost.count)
                                    {
                                        return true; // Still needed
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return false; // Not needed
        }
        
        static List<CraftRecipe> PrioritizeRecipesBySkillProgress(List<CraftRecipe> recipes, SimulationState state, GameData data, System.IO.StreamWriter logWriter)
        {
            var prioritized = new List<CraftRecipe>();
            
            // Find currently unlockable skills (skills that can be unlocked with current resources)
            var unlockableSkills = GetUnlockableSkills(state, data, logWriter);
            
            // First priority: recipes needed for currently unlockable skills (only if we don't have enough)
            foreach (var skill in unlockableSkills)
            {
                var skillInfo = GetSkillInfo(skill, data);
                if (skillInfo != null && skillInfo.unlockCost != null)
                {
                    foreach (var cost in skillInfo.unlockCost)
                    {
                        int currentCount = state.resources.ContainsKey(cost.resourceId) ? (int)state.resources[cost.resourceId] : 0;
                        if (currentCount < cost.count) // Only craft if we don't have enough
                        {
                            var recipe = recipes.FirstOrDefault(r => r.outcomeId == cost.resourceId);
                            if (recipe != null && !prioritized.Contains(recipe) && CanCraft(recipe, state))
                            {
                                prioritized.Add(recipe);
                                logWriter.WriteLine($"[Debug] Prioritizing {recipe.outcomeId} for skill {skill} (need {cost.count}, have {currentCount})");
                            }
                        }
                    }
                }
            }
            
            // Second priority: recipes needed for next-level unlockable skills (skills that will be unlockable soon)
            var nextLevelSkills = GetNextLevelUnlockableSkills(state, data, logWriter);
            foreach (var skill in nextLevelSkills)
            {
                var skillInfo = GetSkillInfo(skill, data);
                if (skillInfo != null && skillInfo.unlockCost != null)
                {
                    foreach (var cost in skillInfo.unlockCost)
                    {
                        int currentCount = state.resources.ContainsKey(cost.resourceId) ? (int)state.resources[cost.resourceId] : 0;
                        if (currentCount < cost.count) // Only craft if we don't have enough
                        {
                            var recipe = recipes.FirstOrDefault(r => r.outcomeId == cost.resourceId);
                            if (recipe != null && !prioritized.Contains(recipe) && CanCraft(recipe, state))
                            {
                                prioritized.Add(recipe);
                                logWriter.WriteLine($"[Debug] Prioritizing {recipe.outcomeId} for next-level skill {skill} (need {cost.count}, have {currentCount})");
                            }
                        }
                    }
                }
            }
            
            // Third priority: recipes that can be crafted immediately (have resources) - only if not already prioritized
            foreach (var recipe in recipes)
            {
                if (!prioritized.Contains(recipe) && CanCraft(recipe, state))
                {
                    prioritized.Add(recipe);
                }
            }
            
            // Fourth priority: all other recipes
            foreach (var recipe in recipes)
            {
                if (!prioritized.Contains(recipe))
                {
                    prioritized.Add(recipe);
                }
            }
            
            return prioritized;
        }
        
        static List<string> GetUnlockableSkills(SimulationState state, GameData data, System.IO.StreamWriter logWriter)
        {
            var unlockable = new List<string>();
            
            // Debug: Log skill tree data
            logWriter.WriteLine($"[Debug] Mining skills count: {data.miningSkills?.skillInfoPool?.Count ?? 0}");
            logWriter.WriteLine($"[Debug] CompCraft skills count: {data.compCraftSkills?.skillInfoPool?.Count ?? 0}");
            logWriter.WriteLine($"[Debug] ItemCraft skills count: {data.itemCraftSkills?.skillInfoPool?.Count ?? 0}");
            logWriter.WriteLine($"[Debug] Goal skills count: {data.goalSkills?.skillInfoPool?.Count ?? 0}");
            logWriter.WriteLine($"[Debug] Unlocked skills: {string.Join(", ", state.unlockedSkills)}");
            
            // Check all skills in all skill trees
            var skillTrees = new[] { data.miningSkills, data.compCraftSkills, data.itemCraftSkills, data.goalSkills };
            
            foreach (var skillTree in skillTrees)
            {
                if (skillTree?.skillInfoPool == null) continue;
                
                logWriter.WriteLine($"[Debug] Checking skill tree: {skillTree.id}, rootId: {skillTree.rootId}");
                
                foreach (var skillInfo in skillTree.skillInfoPool)
                {
                    if (!state.unlockedSkills.Contains(skillInfo.id))
                    {
                        logWriter.WriteLine($"[Debug] Checking skill: {skillInfo.id}");
                        
                        // Check if prerequisites are met (parent skills unlocked)
                        bool prerequisitesMet = true;
                        if (skillInfo.id != skillTree.rootId)
                        {
                            // Find parent skill
                            var parentSkill = skillTree.skillInfoPool.FirstOrDefault(s => 
                                s.children != null && s.children.Contains(skillInfo.id));
                            if (parentSkill != null && !state.unlockedSkills.Contains(parentSkill.id))
                            {
                                prerequisitesMet = false;
                                logWriter.WriteLine($"[Debug] Prerequisites not met for {skillInfo.id}: parent {parentSkill.id} not unlocked");
                            }
                        }
                        else
                        {
                            logWriter.WriteLine($"[Debug] {skillInfo.id} is root skill");
                        }
                        
                        if (prerequisitesMet && skillInfo.unlockCost != null)
                        {
                            logWriter.WriteLine($"[Debug] Checking unlock cost for {skillInfo.id}");
                            bool canUnlock = true;
                            foreach (var cost in skillInfo.unlockCost)
                            {
                                int resourceCount = state.resources.ContainsKey(cost.resourceId) ? (int)state.resources[cost.resourceId] : 0;
                                logWriter.WriteLine($"[Debug] {skillInfo.id} needs {cost.resourceId}: {cost.count}, have: {resourceCount}");
                                if (resourceCount < cost.count)
                                {
                                    canUnlock = false;
                                    break;
                                }
                            }
                            
                            if (canUnlock)
                            {
                                unlockable.Add(skillInfo.id);
                                logWriter.WriteLine($"[Debug] Found unlockable skill: {skillInfo.id}");
                            }
                        }
                    }
                }
            }
            
            return unlockable;
        }
        
        static List<string> GetNextLevelUnlockableSkills(SimulationState state, GameData data, System.IO.StreamWriter logWriter)
        {
            var nextLevel = new List<string>();
            
            // Check all skills in all skill trees
            var skillTrees = new[] { data.miningSkills, data.compCraftSkills, data.itemCraftSkills, data.goalSkills };
            
            foreach (var skillTree in skillTrees)
            {
                if (skillTree?.skillInfoPool == null) continue;
                
                foreach (var skillInfo in skillTree.skillInfoPool)
                {
                    if (!state.unlockedSkills.Contains(skillInfo.id))
                    {
                        // Check if prerequisites are met (parent skills unlocked)
                        bool prerequisitesMet = true;
                        if (skillInfo.id != skillTree.rootId)
                        {
                            // Find parent skill
                            var parentSkill = skillTree.skillInfoPool.FirstOrDefault(s => 
                                s.children != null && s.children.Contains(skillInfo.id));
                            if (parentSkill != null && !state.unlockedSkills.Contains(parentSkill.id))
                            {
                                prerequisitesMet = false;
                            }
                        }
                        
                        if (prerequisitesMet && skillInfo.unlockCost != null)
                        {
                            bool closeToUnlock = true;
                            foreach (var cost in skillInfo.unlockCost)
                            {
                                int resourceCount = state.resources.ContainsKey(cost.resourceId) ? (int)state.resources[cost.resourceId] : 0;
                                if (resourceCount < cost.count * 0.5f) // Need at least 50% of required resources
                                {
                                    closeToUnlock = false;
                                    break;
                                }
                            }
                            
                            if (closeToUnlock)
                            {
                                nextLevel.Add(skillInfo.id);
                            }
                        }
                    }
                }
            }
            
            return nextLevel;
        }
        
        static List<CraftRecipe> PrioritizeRecipes(List<CraftRecipe> recipes, SimulationState state, GameData data)
        {
            var prioritized = new List<CraftRecipe>();
            
            // First priority: recipes needed for GoalSection_0_Final (infernalcodex series)
            foreach (var recipe in recipes)
            {
                if (IsRecipeNeededForGoal(recipe.outcomeId, state, data))
                {
                    prioritized.Add(recipe);
                }
            }
            
            // Second priority: recipes needed for zone unlocking (bindingsigil, etc.)
            foreach (var recipe in recipes)
            {
                if (!prioritized.Contains(recipe) && IsRecipeNeededForZoneUnlock(recipe.outcomeId, state, data))
                {
                    prioritized.Add(recipe);
                }
            }
            
            // Third priority: recipes needed for immediate skill unlocking
            foreach (var recipe in recipes)
            {
                if (!prioritized.Contains(recipe) && IsRecipeNeededForSkills(recipe.outcomeId, state, data))
                {
                    prioritized.Add(recipe);
                }
            }
            
            // Fourth priority: recipes that can be crafted (have resources)
            foreach (var recipe in recipes)
            {
                if (!prioritized.Contains(recipe) && CanCraft(recipe, state))
                {
                    prioritized.Add(recipe);
                }
            }
            
            // Fifth priority: recipes needed for future skills (even if not craftable yet)
            foreach (var recipe in recipes)
            {
                if (!prioritized.Contains(recipe) && IsRecipeNeededForFutureSkills(recipe.outcomeId, state, data))
                {
                    prioritized.Add(recipe);
                }
            }
            
            // Sixth priority: all other recipes
            foreach (var recipe in recipes)
            {
                if (!prioritized.Contains(recipe))
                {
                    prioritized.Add(recipe);
                }
            }
            
            return prioritized;
        }
        
        static bool IsRecipeNeededForGoal(string outcomeId, SimulationState state, GameData data)
        {
            // Check if this recipe outcome is needed for GoalSection_0_Final
            var goalSkill = GetSkillInfo("GoalSection_0_Final", data);
            if (goalSkill != null && goalSkill.unlockCost != null)
            {
                foreach (var cost in goalSkill.unlockCost)
                {
                    if (cost.resourceId == outcomeId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        static bool IsRecipeNeededForZoneUnlock(string outcomeId, SimulationState state, GameData data)
        {
            // Check if this recipe outcome is needed for zone unlocking skills
            string[] zoneUnlockSkills = {
                "Mining_1_ZoneDiscovery2", "Mining_2_ZoneDiscovery3", "Mining_3_ZoneDiscovery4",
                "CompCraft_5_ZoneDiscovery5", "ItemCraft_5_ZoneDiscovery6"
            };
            
            foreach (var skillId in zoneUnlockSkills)
            {
                if (!state.unlockedSkills.Contains(skillId))
                {
                    var skillInfo = GetSkillInfo(skillId, data);
                    if (skillInfo != null && skillInfo.unlockCost != null)
                    {
                        foreach (var cost in skillInfo.unlockCost)
                        {
                            if (cost.resourceId == outcomeId)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            
            // Also prioritize recipes that produce resources needed for zone unlock
            if (outcomeId == "nightwing" || outcomeId == "bindingsigil")
            {
                return true; // Always prioritize these for zone unlocking
            }
            
            return false;
        }
        
        static bool IsRecipeNeededForFutureSkills(string outcomeId, SimulationState state, GameData data)
        {
            // Check if this recipe outcome is needed for any future skills
            foreach (var skillPath in state.skillPaths.Values)
            {
                foreach (var skillId in skillPath)
                {
                    if (!state.unlockedSkills.Contains(skillId))
                    {
                        // This skill is not unlocked yet, check if it needs this resource
                        var skillInfo = GetSkillInfo(skillId, data);
                        if (skillInfo != null && skillInfo.unlockCost != null)
                        {
                            foreach (var cost in skillInfo.unlockCost)
                            {
                                if (cost.resourceId == outcomeId)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        
        static bool IsRecipeNeededForSkills(string outcomeId, SimulationState state, GameData data)
        {
            // Check if this recipe outcome is needed for any unlocked skills
            foreach (var skillPath in state.skillPaths.Values)
            {
                foreach (var skillId in skillPath)
                {
                    if (!state.unlockedSkills.Contains(skillId))
                    {
                        // This skill is not unlocked yet, check if it needs this resource
                        var skillInfo = GetSkillInfo(skillId, data);
                        if (skillInfo != null && skillInfo.unlockCost != null)
                        {
                            foreach (var cost in skillInfo.unlockCost)
                            {
                                if (cost.resourceId == outcomeId)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        
        static bool CanCraft(CraftRecipe recipe, SimulationState state)
        {
            // If no sources required, can always craft
            if (recipe.sources == null || recipe.sources.Count == 0)
            {
                return true;
            }
            
            foreach (var source in recipe.sources)
            {
                if (!state.resources.ContainsKey(source.resourceId) || 
                    state.resources[source.resourceId] < source.count)
                {
                    return false;
                }
            }
            return true;
        }
        
        static float GetCraftTimeMultiplier(CraftRecipe recipe, SimulationState state)
        {
            float multiplier = 1.0f;
            
            // Apply craft time buffs from unlocked skills
            foreach (var skillId in state.unlockedSkills)
            {
                // This would need to check actual skill effects
                // For now, simplified implementation
            }
            
            return multiplier;
        }
        
        static float GetCraftResourceMultiplier(CraftRecipe recipe, SimulationState state)
        {
            float multiplier = 1.0f;
            
            // Apply resource reduction buffs from unlocked skills
            foreach (var skillId in state.unlockedSkills)
            {
                // This would need to check actual skill effects
                // For now, simplified implementation
            }
            
            return multiplier;
        }
        
        static void ProcessSkillUnlocking(GameData data, SimulationState state, System.IO.StreamWriter logWriter)
        {
            ProcessSkillTree(data.miningSkills, state, logWriter);
            ProcessSkillTree(data.compCraftSkills, state, logWriter);
            ProcessSkillTree(data.itemCraftSkills, state, logWriter);
            ProcessSkillTree(data.goalSkills, state, logWriter);
        }
        
        static void ProcessSkillTree(SkillTreeData skillTree, SimulationState state, System.IO.StreamWriter logWriter)
        {
            if (!state.skillPaths.ContainsKey(skillTree.id)) return;
            
            var path = state.skillPaths[skillTree.id];
            if (path.Count == 0) return;
            
            // Find the next skill to unlock in this path
            string nextSkillId = null;
            for (int i = 0; i < path.Count; i++)
            {
                if (!state.unlockedSkills.Contains(path[i]))
                {
                    nextSkillId = path[i];
                    break;
                }
            }
            
            if (nextSkillId == null) return; // All skills in this path are unlocked
            
            var skillInfo = skillTree.skillInfoPool.FirstOrDefault(s => s.id == nextSkillId);
            if (skillInfo == null) return;
            
            // Check if we can unlock this skill
            if (CanUnlockSkill(skillInfo, state))
            {
                state.unlockedSkills.Add(nextSkillId);
                ConsumeSkillCosts(skillInfo, state);
                state.skillCompletionTimes[nextSkillId] = state.currentTime;
                
                string skillLog = $"Skill unlocked: {nextSkillId} at {state.currentTime:F1} hours";
                Debug.Log(skillLog);
                logWriter.WriteLine(skillLog);
                
                // Apply skill effects
                ApplySkillEffect(skillInfo, state, logWriter);
            }
        }
        
        static bool CanUnlockSkill(SkillInfo skillInfo, SimulationState state)
        {
            foreach (var cost in skillInfo.unlockCost)
            {
                if (!state.resources.ContainsKey(cost.resourceId) || 
                    state.resources[cost.resourceId] < cost.count)
                {
                    return false;
                }
            }
            return true;
        }
        
        static void ConsumeSkillCosts(SkillInfo skillInfo, SimulationState state)
        {
            foreach (var cost in skillInfo.unlockCost)
            {
                if (state.resources.ContainsKey(cost.resourceId))
                {
                    state.resources[cost.resourceId] -= cost.count;
                }
            }
        }
        
        static void ApplySkillEffect(SkillInfo skillInfo, SimulationState state, System.IO.StreamWriter logWriter)
        {
            switch (skillInfo.abilityId)
            {
                case "unlock_zone":
                    if (int.TryParse(skillInfo.abilityParam, out int zoneId))
                    {
                        state.currentZone = Math.Max(state.currentZone, zoneId / 100 - 1);
                        string zoneLog = $"Zone {zoneId} unlocked at {state.currentTime:F1} hours";
                        Debug.Log(zoneLog);
                        logWriter.WriteLine(zoneLog);
                    }
                    break;
                    
                case "unlock_feature":
                    string featureLog = $"Feature {skillInfo.abilityParam} unlocked at {state.currentTime:F1} hours";
                    Debug.Log(featureLog);
                    logWriter.WriteLine(featureLog);
                    break;
                    
                case "mining_zone_buff":
                    // Parse buff parameters: "zoneId:duration:buffRate:coolTimeDuration"
                    string[] params2 = skillInfo.abilityParam.Split(':');
                    if (params2.Length >= 4)
                    {
                        if (int.TryParse(params2[0], out int buffZoneId))
                        {
                            state.zoneBuffCooldowns[buffZoneId.ToString()] = 240.0; // 240 hours duration (entire simulation)
                            string buffLog = $"Zone buff activated for Zone {buffZoneId} at {state.currentTime:F1} hours";
                            Debug.Log(buffLog);
                            logWriter.WriteLine(buffLog);
                        }
                    }
                    break;
            }
        }
        
        static void ProcessUpgrades(GameData data, SimulationState state)
        {
            // Immediate upgrades with gold for optimal mining efficiency
            foreach (var zoneGroup in data.planetData.data)
            {
                foreach (var planet in zoneGroup.planets)
                {
                    string planetKey = $"{zoneGroup.zoneId}_{planet.id}";
                    
                    // Calculate optimal upgrade level based on available coins
                    int currentLevel = state.planetUpgradeLevels.ContainsKey(planetKey) ? state.planetUpgradeLevels[planetKey] : 1;
                    int maxAffordableLevel = CalculateMaxAffordableLevel(planet, state.totalCoins);
                    
                    if (maxAffordableLevel > currentLevel)
                    {
                        // Upgrade to optimal level
                        state.planetUpgradeLevels[planetKey] = maxAffordableLevel;
                        
                        // Deduct upgrade cost (simplified - assume linear cost)
                        BigInteger upgradeCost = CalculateUpgradeCost(planet, currentLevel, maxAffordableLevel);
                        state.totalCoins -= upgradeCost;
                        
                        if (state.currentTime < 5.0)
                        {
                            string upgradeLog = $"[Simulator] Upgraded Planet {planetKey} to level {maxAffordableLevel} at {state.currentTime:F1} hours";
                            Debug.Log(upgradeLog);
                        }
                    }
                }
            }
        }
        
        static int CalculateMaxAffordableLevel(PlanetZoneData planet, BigInteger availableCoins)
        {
            // Simplified calculation - assume each level costs 1000 coins
            int maxLevel = (int)(availableCoins / 1000);
            return Math.Max(1, Math.Min(maxLevel, 50)); // Cap at level 50
        }
        
        static BigInteger CalculateUpgradeCost(PlanetZoneData planet, int fromLevel, int toLevel)
        {
            // Simplified linear cost calculation
            return (toLevel - fromLevel) * 1000;
        }
        
        static LevelBasedFloat ParseLevelBasedFloat(string value)
        {
            // Parse strings like "base=1.00:incPercent=100.00:incBase=1.00"
            var parts = value.Split(':');
            float baseValue = 1.0f;
            float incPercent = 100.0f;
            float incBase = 1.0f;
            
            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2)
                {
                    switch (keyValue[0])
                    {
                        case "base":
                            float.TryParse(keyValue[1], out baseValue);
                            break;
                        case "incPercent":
                            float.TryParse(keyValue[1], out incPercent);
                            break;
                        case "incBase":
                            float.TryParse(keyValue[1], out incBase);
                            break;
                    }
                }
            }
            
            return new LevelBasedFloat(baseValue, incPercent, incBase);
        }
        
        // Helper class for LevelBasedFloat
        public class LevelBasedFloat
        {
            public float DefaultValue;
            public float IncreasePercent;
            public float IncreaseBase;
            
            public LevelBasedFloat(float defaultValue, float increasePercent, float increaseBase)
            {
                DefaultValue = defaultValue;
                IncreasePercent = increasePercent;
                IncreaseBase = increaseBase;
            }
            
            public float Value(int level)
            {
                level--;
                float increasedAmount = IncreaseBase * IncreasePercent * ((float)level);
                increasedAmount *= 0.01f;
                return DefaultValue + increasedAmount;
            }
        }

        void ShowResultDialog(SimulationResult res)
        {
            var msg = new StringBuilder(256);
            msg.AppendLine($"Total Hours: {res.totalHours:F1}");
            msg.AppendLine($"Coins: {res.totalCoins}");
            msg.AppendLine($"Goal Achieved: {res.goalAchieved}");
            msg.AppendLine($"Completion Time: {res.completionTime:F1} hours");
            msg.AppendLine($"Unlocked Skills: {res.unlockedSkills}");
            msg.AppendLine($"Levels  M:{res.miningLevel}  S:{res.deliveryLevel}  C:{res.cargoLevel}");
            EditorUtility.DisplayDialog("Idle Game Simulator", msg.ToString(), "OK");
        }
    }*/
}


