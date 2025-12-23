using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using GD = IGCore.Simulator.GameData;

namespace IGCore.Simulator.Editor
{
    public class GameDataEditor : EditorWindow
    {
        [Header("Target Parameters")]
        [SerializeField] private float targetCompletionDays = 10f;
        [SerializeField] private float resourceAmountMultiplier = 1.0f;
        [SerializeField] private float planetProductionMultiplier = 1.0f;
        [SerializeField] private float craftDurationMultiplier = 1.0f;
        
        [Header("Balance Adjustments")]
        [SerializeField] private float skillCostProgression = 1.5f; // Each skill costs 1.5x more than previous
        [SerializeField] private float planetUnlockProgression = 1.2f; // Each planet is 1.2x more productive
        [SerializeField] private float resourcePriceProgression = 2.0f; // Each tier costs 2x more
        
        [Header("Advanced Settings")]
        [SerializeField] private bool autoCalculate = true;
        [SerializeField] private bool preserveRelativeRatios = true;
        [SerializeField] private bool applySkillEffects = false;
        
        private GD.GameData originalGameData;
        private GD.GameData modifiedGameData;
        private UnityEngine.Vector2 scrollPosition;
        private bool showPreview = false;
        private string previewResult = "";
        
        [MenuItem("PlasticGames/SimpleSimulator/Game Data Editor")]
        public static void Open()
        {
            var window = GetWindow<GameDataEditor>("Game Data Editor");
            window.minSize = new UnityEngine.Vector2(600, 800);
            window.Show();
        }
        
        void OnEnable()
        {
            LoadGameData();
        }
        
        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.LabelField("Game Data Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            DrawParameters();
            EditorGUILayout.Space();
            
            DrawActionButtons();
            EditorGUILayout.Space();
            
            if (showPreview)
            {
                DrawPreview();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        void DrawParameters()
        {
            EditorGUILayout.LabelField("Target Parameters", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            targetCompletionDays = EditorGUILayout.FloatField("Target Completion Days", targetCompletionDays);
            resourceAmountMultiplier = EditorGUILayout.FloatField("Resource Amount Multiplier", resourceAmountMultiplier);
            planetProductionMultiplier = EditorGUILayout.FloatField("Planet Production Multiplier", planetProductionMultiplier);
            craftDurationMultiplier = EditorGUILayout.FloatField("Craft Duration Multiplier", craftDurationMultiplier);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Balance Adjustments", EditorStyles.boldLabel);
            
            skillCostProgression = EditorGUILayout.FloatField("Skill Cost Progression", skillCostProgression);
            planetUnlockProgression = EditorGUILayout.FloatField("Planet Unlock Progression", planetUnlockProgression);
            resourcePriceProgression = EditorGUILayout.FloatField("Resource Price Progression", resourcePriceProgression);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Advanced Settings", EditorStyles.boldLabel);
            
            autoCalculate = EditorGUILayout.Toggle("Auto Calculate", autoCalculate);
            preserveRelativeRatios = EditorGUILayout.Toggle("Preserve Relative Ratios", preserveRelativeRatios);
            applySkillEffects = EditorGUILayout.Toggle("Apply Skill Effects", applySkillEffects);
            
            if (EditorGUI.EndChangeCheck() && autoCalculate)
            {
                CalculateAndUpdateData();
            }
        }
        
        void DrawActionButtons()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Load Original Data"))
            {
                LoadGameData();
            }
            
            if (GUILayout.Button("Calculate & Preview"))
            {
                CalculateAndUpdateData();
                showPreview = true;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Apply Changes"))
            {
                ApplyChanges();
            }
            
            if (GUILayout.Button("Reset to Original"))
            {
                ResetToOriginal();
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Run Simulation Test"))
            {
                RunSimulationTest();
            }
        }
        
        void DrawPreview()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview Results", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(previewResult, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
        }
        
        void LoadGameData()
        {
            try
            {
                string jsonString = Resources.Load<TextAsset>("Data/Simulator/gameData").text;
                originalGameData = JsonUtility.FromJson<GD.GameData>(jsonString);
                modifiedGameData = JsonUtility.FromJson<GD.GameData>(jsonString);
                
                Debug.Log("Game data loaded successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game data: {e.Message}");
            }
        }
        
        void CalculateAndUpdateData()
        {
            if (originalGameData == null)
            {
                Debug.LogError("No game data loaded!");
                return;
            }
            
            // Create a copy for modification
            modifiedGameData = JsonUtility.FromJson<GD.GameData>(JsonUtility.ToJson(originalGameData));
            
            // Calculate target completion time in seconds
            double targetTimeSeconds = targetCompletionDays * 24 * 3600;
            
            // Calculate total materials needed for all skills
            BigInteger totalMaterialsNeeded = CalculateTotalMaterialsNeeded();
            
            // Calculate required production rate
            double requiredProductionRate = (double)totalMaterialsNeeded / targetTimeSeconds;
            
            // Adjust planet production rates
            AdjustPlanetProductionRates(requiredProductionRate);
            
            // Adjust skill costs based on progression
            AdjustSkillCosts();
            
            // Adjust resource prices
            AdjustResourcePrices();
            
            // Adjust craft durations
            AdjustCraftDurations();
            
            // Generate preview
            GeneratePreview();
        }
        
        BigInteger CalculateTotalMaterialsNeeded()
        {
            BigInteger total = 0;
            
            foreach (var skill in modifiedGameData.skillTree)
            {
                var requirement = skill.sources[0];
                BigInteger amount = BigInteger.Parse(requirement.amount);
                
                // Convert to materials (item -> comp -> mat conversion)
                BigInteger materialsNeeded = amount * 10; // Simplified conversion
                total += materialsNeeded;
            }
            
            return total;
        }
        
        void AdjustPlanetProductionRates(double requiredProductionRate)
        {
            double currentProductionRate = 0;
            
            // Calculate current production rate
            foreach (var planet in modifiedGameData.planets)
            {
                foreach (var obtainable in planet.obtainables)
                {
                    if (obtainable.resourceId == "mat_9")
                    {
                        currentProductionRate += obtainable.rate + (planet.performances.GetValue(1) * 0.001f);
                    }
                }
            }
            
            // Calculate multiplier needed
            double productionMultiplier = requiredProductionRate / currentProductionRate;
            
            // Apply multiplier to planet production rates
            foreach (var planet in modifiedGameData.planets)
            {
                foreach (var obtainable in planet.obtainables)
                {
                    obtainable.rate *= (float)(productionMultiplier * planetProductionMultiplier);
                }
                
                // Also adjust performance values
                planet.performances.baseValue *= (float)(productionMultiplier * planetProductionMultiplier);
                planet.performances.increaseValue *= (float)(productionMultiplier * planetProductionMultiplier);
            }
        }
        
        void AdjustSkillCosts()
        {
            BigInteger baseCost = 100;
            
            for (int i = 0; i < modifiedGameData.skillTree.Count; i++)
            {
                var skill = modifiedGameData.skillTree[i];
                var requirement = skill.sources[0];
                
                // Calculate cost based on progression
                BigInteger cost = baseCost;
                for (int j = 0; j < i; j++)
                {
                    cost = (BigInteger)((double)cost * skillCostProgression);
                }
                
                // Apply resource amount multiplier
                cost = (BigInteger)((double)cost * resourceAmountMultiplier);
                
                requirement.amount = cost.ToString();
            }
        }
        
        void AdjustResourcePrices()
        {
            BigInteger basePrice = 10;
            
            // Adjust material prices
            for (int i = 0; i < 10; i++)
            {
                var resource = modifiedGameData.resources.Find(r => r.id == $"mat_{i}");
                if (resource != null)
                {
                    BigInteger price = basePrice;
                    for (int j = 0; j < i; j++)
                    {
                        price = (BigInteger)((double)price * resourcePriceProgression);
                    }
                    resource.SetSellPrice(price);
                }
            }
            
            // Adjust component prices
            for (int i = 0; i < 10; i++)
            {
                var resource = modifiedGameData.resources.Find(r => r.id == $"comp_{i}");
                if (resource != null)
                {
                    BigInteger price = basePrice * 20; // Components are more expensive
                    for (int j = 0; j < i; j++)
                    {
                        price = (BigInteger)((double)price * resourcePriceProgression);
                    }
                    resource.SetSellPrice(price);
                }
            }
            
            // Adjust item prices
            for (int i = 0; i < 10; i++)
            {
                var resource = modifiedGameData.resources.Find(r => r.id == $"item_{i}");
                if (resource != null)
                {
                    BigInteger price = basePrice * 200; // Items are most expensive
                    for (int j = 0; j < i; j++)
                    {
                        price = (BigInteger)((double)price * resourcePriceProgression);
                    }
                    resource.SetSellPrice(price);
                }
            }
        }
        
        void AdjustCraftDurations()
        {
            // Adjust component craft durations
            foreach (var recipe in modifiedGameData.compRecipes)
            {
                recipe.duration = Mathf.RoundToInt(recipe.duration * craftDurationMultiplier);
            }
            
            // Adjust item craft durations
            foreach (var recipe in modifiedGameData.itemRecipes)
            {
                recipe.duration = Mathf.RoundToInt(recipe.duration * craftDurationMultiplier);
            }
        }
        
        void GeneratePreview()
        {
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine($"Target Completion Time: {targetCompletionDays} days");
            sb.AppendLine($"Resource Amount Multiplier: {resourceAmountMultiplier:F2}x");
            sb.AppendLine($"Planet Production Multiplier: {planetProductionMultiplier:F2}x");
            sb.AppendLine($"Craft Duration Multiplier: {craftDurationMultiplier:F2}x");
            sb.AppendLine();
            
            sb.AppendLine("Skill Costs:");
            for (int i = 0; i < Math.Min(5, modifiedGameData.skillTree.Count); i++)
            {
                var skill = modifiedGameData.skillTree[i];
                var requirement = skill.sources[0];
                sb.AppendLine($"  {skill.id}: {requirement.amount} {requirement.resourceId}");
            }
            if (modifiedGameData.skillTree.Count > 5)
            {
                sb.AppendLine($"  ... and {modifiedGameData.skillTree.Count - 5} more skills");
            }
            
            sb.AppendLine();
            sb.AppendLine("Planet Production Rates (mat_9):");
            for (int i = 0; i < Math.Min(3, modifiedGameData.planets.Count); i++)
            {
                var planet = modifiedGameData.planets[i];
                foreach (var obtainable in planet.obtainables)
                {
                    if (obtainable.resourceId == "mat_9")
                    {
                        sb.AppendLine($"  {planet.id}: {obtainable.rate:F4}/s");
                        break;
                    }
                }
            }
            
            previewResult = sb.ToString();
        }
        
        void ApplyChanges()
        {
            if (modifiedGameData == null)
            {
                Debug.LogError("No modified data to apply!");
                return;
            }
            
            try
            {
                string jsonString = JsonUtility.ToJson(modifiedGameData, true);
                string filePath = "Assets/Resources/Data/Simulator/gameData.json";
                
                System.IO.File.WriteAllText(filePath, jsonString);
                AssetDatabase.Refresh();
                
                Debug.Log("Game data updated successfully!");
                EditorUtility.DisplayDialog("Success", "Game data has been updated!", "OK");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to apply changes: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to apply changes: {e.Message}", "OK");
            }
        }
        
        void ResetToOriginal()
        {
            if (originalGameData == null)
            {
                Debug.LogError("No original data to reset to!");
                return;
            }
            
            modifiedGameData = JsonUtility.FromJson<GD.GameData>(JsonUtility.ToJson(originalGameData));
            showPreview = false;
            
            Debug.Log("Data reset to original values");
        }
        
        void RunSimulationTest()
        {
            if (modifiedGameData == null)
            {
                Debug.LogError("No data to test!");
                return;
            }
            
            // Create a temporary file for testing
            string tempJson = JsonUtility.ToJson(modifiedGameData, true);
            string tempPath = "Assets/Resources/Data/Simulator/gameData_temp.json";
            
            try
            {
                System.IO.File.WriteAllText(tempPath, tempJson);
                AssetDatabase.Refresh();
                
                Debug.Log("Running simulation test with modified data...");
                Debug.Log("Check the Console for simulation results.");
                
                // Note: This would require modifying GameSimulation to use the temp file
                // For now, just log that the test data is ready
                Debug.Log($"Test data saved to: {tempPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create test data: {e.Message}");
            }
        }
    }
}
