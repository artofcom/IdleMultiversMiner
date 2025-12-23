using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using GD = IGCore.Simulator.GameData;

namespace IGCore.Simulator.Editor
{
    public class GameDataAnalyzer : EditorWindow
    {
        private GD.GameData gameData;
        private AnalysisResult analysisResult;
        private UnityEngine.Vector2 scrollPosition;
        private bool showDetailedAnalysis = false;
        
        [Serializable]
        public class AnalysisResult
        {
            public double estimatedCompletionDays;
            public BigInteger totalMaterialsNeeded;
            public double totalProductionRate;
            public List<SkillAnalysis> skillAnalyses = new List<SkillAnalysis>();
            public List<PlanetAnalysis> planetAnalyses = new List<PlanetAnalysis>();
            public BalanceRecommendations recommendations = new BalanceRecommendations();
        }
        
        [Serializable]
        public class SkillAnalysis
        {
            public string skillId;
            public BigInteger cost;
            public double estimatedTimeHours;
            public double cumulativeTimeHours;
            public List<int> unlockedPlanets;
        }
        
        [Serializable]
        public class PlanetAnalysis
        {
            public string planetId;
            public double productionRate;
            public double contributionPercentage;
            public bool isUnlocked;
        }
        
        [Serializable]
        public class BalanceRecommendations
        {
            public string overallAssessment;
            public List<string> recommendations = new List<string>();
            public float suggestedProductionMultiplier;
            public float suggestedCostMultiplier;
        }
        
        [MenuItem("PlasticGames/SimpleSimulator/Game Data Analyzer")]
        public static void Open()
        {
            var window = GetWindow<GameDataAnalyzer>("Game Data Analyzer");
            window.minSize = new UnityEngine.Vector2(700, 800);
            window.Show();
        }
        
        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.LabelField("Game Data Analyzer", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Load and Analyze Data"))
            {
                LoadAndAnalyzeData();
            }
            
            EditorGUILayout.Space();
            
            if (analysisResult != null)
            {
                DrawAnalysisResults();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        void LoadAndAnalyzeData()
        {
            try
            {
                string jsonString = Resources.Load<TextAsset>("Data/Simulator/gameData").text;
                gameData = JsonUtility.FromJson<GD.GameData>(jsonString);
                
                AnalyzeData();
                
                Debug.Log("Data analysis completed");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game data: {e.Message}");
            }
        }
        
        void AnalyzeData()
        {
            analysisResult = new AnalysisResult();
            
            AnalyzeSkills();
            AnalyzePlanets();
            CalculateOverallBalance();
            GenerateRecommendations();
        }
        
        void AnalyzeSkills()
        {
            var unlockedPlanets = new HashSet<int>();
            double cumulativeTime = 0;
            
            foreach (var skill in gameData.skillTree)
            {
                var skillAnalysis = new SkillAnalysis();
                skillAnalysis.skillId = skill.id;
                skillAnalysis.cost = BigInteger.Parse(skill.sources[0].amount);
                skillAnalysis.unlockedPlanets = new List<int>(skill.unlockPlanets);
                
                // Calculate materials needed
                BigInteger materialsNeeded = skillAnalysis.cost * 10; // Simplified conversion
                
                // Calculate production rate from unlocked planets
                double productionRate = CalculateProductionRate(unlockedPlanets);
                
                // Calculate time needed
                if (productionRate > 0)
                {
                    skillAnalysis.estimatedTimeHours = (double)materialsNeeded / productionRate / 3600;
                }
                else
                {
                    skillAnalysis.estimatedTimeHours = 0;
                }
                
                cumulativeTime += skillAnalysis.estimatedTimeHours;
                skillAnalysis.cumulativeTimeHours = cumulativeTime;
                
                analysisResult.skillAnalyses.Add(skillAnalysis);
                
                // Add newly unlocked planets
                foreach (var planetId in skill.unlockPlanets)
                {
                    unlockedPlanets.Add(planetId);
                }
            }
            
            analysisResult.estimatedCompletionDays = cumulativeTime / 24;
        }
        
        void AnalyzePlanets()
        {
            var unlockedPlanets = new HashSet<int>();
            double totalProduction = 0;
            
            // First pass: calculate total production
            foreach (var skill in gameData.skillTree)
            {
                foreach (var planetId in skill.unlockPlanets)
                {
                    unlockedPlanets.Add(planetId);
                }
            }
            
            foreach (var planet in gameData.planets)
            {
                var planetAnalysis = new PlanetAnalysis();
                planetAnalysis.planetId = planet.id;
                planetAnalysis.isUnlocked = unlockedPlanets.Contains(int.Parse(planet.id.Replace("planet_", "")));
                
                // Calculate production rate for mat_9
                foreach (var obtainable in planet.obtainables)
                {
                    if (obtainable.resourceId == "mat_9")
                    {
                        planetAnalysis.productionRate = obtainable.rate + (planet.performances.GetValue(1) * 0.001f);
                        totalProduction += planetAnalysis.productionRate;
                        break;
                    }
                }
                
                analysisResult.planetAnalyses.Add(planetAnalysis);
            }
            
            // Second pass: calculate contribution percentages
            foreach (var planetAnalysis in analysisResult.planetAnalyses)
            {
                if (totalProduction > 0)
                {
                    planetAnalysis.contributionPercentage = (planetAnalysis.productionRate / totalProduction) * 100;
                }
            }
            
            analysisResult.totalProductionRate = totalProduction;
        }
        
        double CalculateProductionRate(HashSet<int> unlockedPlanets)
        {
            double totalRate = 0;
            
            foreach (var planetId in unlockedPlanets)
            {
                var planet = gameData.planets.Find(p => p.id == $"planet_{planetId}");
                if (planet != null)
                {
                    foreach (var obtainable in planet.obtainables)
                    {
                        if (obtainable.resourceId == "mat_9")
                        {
                            totalRate += obtainable.rate + (planet.performances.GetValue(1) * 0.001f);
                            break;
                        }
                    }
                }
            }
            
            return totalRate;
        }
        
        void CalculateOverallBalance()
        {
            // Calculate total materials needed
            BigInteger totalMaterials = 0;
            foreach (var skill in gameData.skillTree)
            {
                var requirement = skill.sources[0];
                BigInteger amount = BigInteger.Parse(requirement.amount);
                totalMaterials += amount * 10; // Simplified conversion
            }
            
            analysisResult.totalMaterialsNeeded = totalMaterials;
        }
        
        void GenerateRecommendations()
        {
            var recommendations = analysisResult.recommendations;
            
            // Overall assessment
            if (analysisResult.estimatedCompletionDays < 5)
            {
                recommendations.overallAssessment = "Game is too easy - players will complete too quickly";
                recommendations.suggestedProductionMultiplier = 0.5f;
                recommendations.suggestedCostMultiplier = 2.0f;
            }
            else if (analysisResult.estimatedCompletionDays > 30)
            {
                recommendations.overallAssessment = "Game is too hard - players will take too long to complete";
                recommendations.suggestedProductionMultiplier = 2.0f;
                recommendations.suggestedCostMultiplier = 0.5f;
            }
            else
            {
                recommendations.overallAssessment = "Game balance looks reasonable";
                recommendations.suggestedProductionMultiplier = 1.0f;
                recommendations.suggestedCostMultiplier = 1.0f;
            }
            
            // Specific recommendations
            recommendations.recommendations.Clear();
            
            if (analysisResult.estimatedCompletionDays < 10)
            {
                recommendations.recommendations.Add("Consider increasing skill costs by 1.5x");
                recommendations.recommendations.Add("Reduce planet production rates by 0.7x");
            }
            else if (analysisResult.estimatedCompletionDays > 20)
            {
                recommendations.recommendations.Add("Consider decreasing skill costs by 0.7x");
                recommendations.recommendations.Add("Increase planet production rates by 1.5x");
            }
            
            // Check for skill time distribution
            var maxSkillTime = 0.0;
            var minSkillTime = double.MaxValue;
            foreach (var skillAnalysis in analysisResult.skillAnalyses)
            {
                maxSkillTime = Math.Max(maxSkillTime, skillAnalysis.estimatedTimeHours);
                minSkillTime = Math.Min(minSkillTime, skillAnalysis.estimatedTimeHours);
            }
            
            if (maxSkillTime / minSkillTime > 10)
            {
                recommendations.recommendations.Add("Skill completion times are very uneven - consider rebalancing");
            }
        }
        
        void DrawAnalysisResults()
        {
            EditorGUILayout.LabelField("Analysis Results", EditorStyles.boldLabel);
            
            // Overall summary
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Overall Summary", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Estimated Completion Time: {analysisResult.estimatedCompletionDays:F1} days");
            EditorGUILayout.LabelField($"Total Materials Needed: {analysisResult.totalMaterialsNeeded:N0}");
            EditorGUILayout.LabelField($"Total Production Rate: {analysisResult.totalProductionRate:F4}/s");
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // Balance assessment
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Balance Assessment", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(analysisResult.recommendations.overallAssessment);
            
            if (analysisResult.recommendations.recommendations.Count > 0)
            {
                EditorGUILayout.LabelField("Recommendations:", EditorStyles.boldLabel);
                foreach (var recommendation in analysisResult.recommendations.recommendations)
                {
                    EditorGUILayout.LabelField($"• {recommendation}");
                }
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // Detailed analysis toggle
            showDetailedAnalysis = EditorGUILayout.Foldout(showDetailedAnalysis, "Detailed Analysis");
            
            if (showDetailedAnalysis)
            {
                DrawDetailedAnalysis();
            }
        }
        
        void DrawDetailedAnalysis()
        {
            // Skill analysis
            EditorGUILayout.LabelField("Skill Analysis", EditorStyles.boldLabel);
            foreach (var skillAnalysis in analysisResult.skillAnalyses)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"{skillAnalysis.skillId}: {skillAnalysis.cost:N0} items");
                EditorGUILayout.LabelField($"Time: {skillAnalysis.estimatedTimeHours:F1}h (Cumulative: {skillAnalysis.cumulativeTimeHours:F1}h)");
                EditorGUILayout.LabelField($"Unlocks: {string.Join(", ", skillAnalysis.unlockedPlanets)}");
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space();
            
            // Planet analysis
            EditorGUILayout.LabelField("Planet Analysis", EditorStyles.boldLabel);
            foreach (var planetAnalysis in analysisResult.planetAnalyses)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"{planetAnalysis.planetId}: {planetAnalysis.productionRate:F4}/s");
                EditorGUILayout.LabelField($"Contribution: {planetAnalysis.contributionPercentage:F1}%");
                EditorGUILayout.LabelField($"Status: {(planetAnalysis.isUnlocked ? "Unlocked" : "Locked")}");
                EditorGUILayout.EndVertical();
            }
        }
    }
}
