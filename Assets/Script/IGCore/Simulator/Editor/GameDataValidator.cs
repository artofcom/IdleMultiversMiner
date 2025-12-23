using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using GD = IGCore.Simulator.GameData;

namespace IGCore.Simulator.Editor
{
    public class GameDataValidator : EditorWindow
    {
        private GD.GameData gameData;
        private List<string> validationResults = new List<string>();
        private UnityEngine.Vector2 scrollPosition;
        
        [MenuItem("PlasticGames/SimpleSimulator/Game Data Validator")]
        public static void Open()
        {
            var window = GetWindow<GameDataValidator>("Game Data Validator");
            window.minSize = new UnityEngine.Vector2(500, 600);
            window.Show();
        }
        
        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.LabelField("Game Data Validator", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Load and Validate Data"))
            {
                LoadAndValidateData();
            }
            
            EditorGUILayout.Space();
            
            if (validationResults.Count > 0)
            {
                EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);
                
                foreach (var result in validationResults)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField(result, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.EndVertical();
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        void LoadAndValidateData()
        {
            validationResults.Clear();
            
            try
            {
                string jsonString = Resources.Load<TextAsset>("Data/Simulator/gameData").text;
                gameData = JsonUtility.FromJson<GD.GameData>(jsonString);
                
                ValidateData();
                
                Debug.Log("Data validation completed");
            }
            catch (Exception e)
            {
                validationResults.Add($"❌ Failed to load data: {e.Message}");
                Debug.LogError($"Failed to load game data: {e.Message}");
            }
        }
        
        void ValidateData()
        {
            if (gameData == null)
            {
                validationResults.Add("❌ No game data loaded");
                return;
            }
            
            ValidateResources();
            ValidatePlanets();
            ValidateRecipes();
            ValidateSkillTree();
            ValidateBalance();
        }
        
        void ValidateResources()
        {
            validationResults.Add("🔍 Validating Resources...");
            
            // Check resource count
            if (gameData.resources.Count != 30)
            {
                validationResults.Add($"⚠️ Expected 30 resources, found {gameData.resources.Count}");
            }
            
            // Check material prices progression
            var materials = gameData.resources.FindAll(r => r.id.StartsWith("mat_"));
            for (int i = 1; i < materials.Count; i++)
            {
                var prevPrice = materials[i-1].GetSellPrice();
                var currPrice = materials[i].GetSellPrice();
                
                if (currPrice <= prevPrice)
                {
                    validationResults.Add($"⚠️ Material price not increasing: {materials[i-1].id} ({prevPrice}) -> {materials[i].id} ({currPrice})");
                }
            }
            
            validationResults.Add($"✅ Resources validated: {materials.Count} materials, {gameData.resources.Count - materials.Count} other resources");
        }
        
        void ValidatePlanets()
        {
            validationResults.Add("🔍 Validating Planets...");
            
            if (gameData.planets.Count != 9)
            {
                validationResults.Add($"⚠️ Expected 9 planets, found {gameData.planets.Count}");
            }
            
            // Check planet production rates
            foreach (var planet in gameData.planets)
            {
                if (planet.obtainables.Count == 0)
                {
                    validationResults.Add($"⚠️ Planet {planet.id} has no obtainables");
                }
                
                if (planet.performances.baseValue <= 0)
                {
                    validationResults.Add($"⚠️ Planet {planet.id} has invalid performance value");
                }
            }
            
            validationResults.Add($"✅ Planets validated: {gameData.planets.Count} planets");
        }
        
        void ValidateRecipes()
        {
            validationResults.Add("🔍 Validating Recipes...");
            
            // Check component recipes
            if (gameData.compRecipes.Count != 10)
            {
                validationResults.Add($"⚠️ Expected 10 component recipes, found {gameData.compRecipes.Count}");
            }
            
            // Check item recipes
            if (gameData.itemRecipes.Count != 10)
            {
                validationResults.Add($"⚠️ Expected 10 item recipes, found {gameData.itemRecipes.Count}");
            }
            
            // Check recipe durations progression
            for (int i = 1; i < gameData.compRecipes.Count; i++)
            {
                if (gameData.compRecipes[i].duration <= gameData.compRecipes[i-1].duration)
                {
                    validationResults.Add($"⚠️ Component recipe duration not increasing: {gameData.compRecipes[i-1].id} -> {gameData.compRecipes[i].id}");
                }
            }
            
            validationResults.Add($"✅ Recipes validated: {gameData.compRecipes.Count} component, {gameData.itemRecipes.Count} item recipes");
        }
        
        void ValidateSkillTree()
        {
            validationResults.Add("🔍 Validating Skill Tree...");
            
            if (gameData.skillTree.Count != 10)
            {
                validationResults.Add($"⚠️ Expected 10 skills, found {gameData.skillTree.Count}");
            }
            
            // Check skill cost progression
            for (int i = 1; i < gameData.skillTree.Count; i++)
            {
                var prevCost = BigInteger.Parse(gameData.skillTree[i-1].sources[0].amount);
                var currCost = BigInteger.Parse(gameData.skillTree[i].sources[0].amount);
                
                if (currCost <= prevCost)
                {
                    validationResults.Add($"⚠️ Skill cost not increasing: {gameData.skillTree[i-1].id} ({prevCost}) -> {gameData.skillTree[i].id} ({currCost})");
                }
            }
            
            // Check planet unlock progression
            var unlockedPlanets = new HashSet<int>();
            for (int i = 0; i < gameData.skillTree.Count; i++)
            {
                var skill = gameData.skillTree[i];
                foreach (var planetId in skill.unlockPlanets)
                {
                    if (unlockedPlanets.Contains(planetId))
                    {
                        validationResults.Add($"⚠️ Planet {planetId} unlocked multiple times");
                    }
                    unlockedPlanets.Add(planetId);
                }
            }
            
            validationResults.Add($"✅ Skill tree validated: {gameData.skillTree.Count} skills, {unlockedPlanets.Count} planets unlocked");
        }
        
        void ValidateBalance()
        {
            validationResults.Add("🔍 Validating Game Balance...");
            
            // Calculate total materials needed
            BigInteger totalMaterialsNeeded = 0;
            foreach (var skill in gameData.skillTree)
            {
                var requirement = skill.sources[0];
                BigInteger amount = BigInteger.Parse(requirement.amount);
                totalMaterialsNeeded += amount * 10; // Simplified conversion
            }
            
            // Calculate total production rate
            double totalProductionRate = 0;
            foreach (var planet in gameData.planets)
            {
                foreach (var obtainable in planet.obtainables)
                {
                    if (obtainable.resourceId == "mat_9")
                    {
                        totalProductionRate += obtainable.rate + (planet.performances.GetValue(1) * 0.001f);
                    }
                }
            }
            
            // Calculate estimated completion time
            double estimatedTimeSeconds = (double)totalMaterialsNeeded / totalProductionRate;
            double estimatedDays = estimatedTimeSeconds / (24 * 3600);
            
            validationResults.Add($"📊 Balance Analysis:");
            validationResults.Add($"   Total materials needed: {totalMaterialsNeeded:N0}");
            validationResults.Add($"   Total production rate: {totalProductionRate:F4}/s");
            validationResults.Add($"   Estimated completion time: {estimatedDays:F1} days");
            
            if (estimatedDays < 5)
            {
                validationResults.Add($"⚠️ Game might be too easy (completion in {estimatedDays:F1} days)");
            }
            else if (estimatedDays > 30)
            {
                validationResults.Add($"⚠️ Game might be too hard (completion in {estimatedDays:F1} days)");
            }
            else
            {
                validationResults.Add($"✅ Game balance looks reasonable");
            }
        }
    }
}
