using App.GamePlay.IdleMiner;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using System.Linq; // LINQ를 사용하여 데이터 검색을 용이하게 합니다.

/*{
  "resources": [
    {
      "id": "Resource_A",
      "resourcePrice": 1,
      "rateIncrement": 2.0,
      "costGrowthMultiplier": 1.10
    },
    {
      "id": "Resource_B",
      "resourcePrice": 20,
      "rateIncrement": 0.11,
      "costGrowthMultiplier": 1.12
    },
    {
      "id": "Resource_C",
      "resourcePrice": 100,
      "rateIncrement": 0.025,
      "costGrowthMultiplier": 1.15
    }
  ]
}*/


public class GameEconomySimulator : MonoBehaviour
{
    
    [System.Serializable]
    public class ResourceData // JSON 배열의 각 항목
    {
        public string id;
        public int resourcePrice;
        public float rateIncrement;
        public float costGrowthMultiplier;
    }

    [System.Serializable]
    public class ResourceConfig // JSON 파일 전체를 담는 컨테이너
    {
        public ResourceData[] resources;
    }

    // 시뮬레이션 결과를 담을 구조체
    public struct SimulationStep
    {
        public int Level;
        public float CumulativeTimeHours; // 누적 시간 (시간 단위)
        public float GatherRatePerSecond; // 채취 속도 (Units/sec)
        public double CumulativeQuantity;  // 누적 채취 수량 (Units)
    }

    // 외부 데이터가 로드될 배열 (초기에는 비어 있음)
    public static ResourceData[] AllResources { get; private set; } 




    // -----------------------------------------------------------
    // 1. 외부에서 고정되어야 할 경제 규칙 (Constants)
    // -----------------------------------------------------------
    private const float ACQUISITION_TIME_SECONDS = 36000f;
    private const float BASE_RATE_UNIT_PER_SECOND = 100f / ACQUISITION_TIME_SECONDS;
    private const float TARGET_UPGRADE_TIME_SECONDS = 60f;
    private const float TIGHTNESS_MULTIPLIER = 1.10f; 




    // -----------------------------------------------------------
    // 2. 초기 설정 (Config Load)
    // -----------------------------------------------------------

    // JSON 파일을 읽어 AllResources 배열에 데이터를 할당하는 함수 (가정)
    public void LoadConfig(ResourceConfig config)
    {
        // JSON 데이터가 로드되면 AllResources 배열에 할당
        AllResources = config.resources;
        Debug.Log($"Loaded {AllResources.Length} resource configurations.");
    }
    
    // 특정 ID로 ResourceData를 찾는 헬퍼 함수
    public ResourceData GetResource(string id)
    {
        return AllResources.FirstOrDefault(r => r.id == id);
    }
    



    // -----------------------------------------------------------
    // 3. 비용 계산 함수 (수정 완료)
    // -----------------------------------------------------------

    public long CalculateUpgradeCost(ResourceData resource, int targetLevel)
    {
        if (targetLevel <= 1) return 0;
        
        // resource.resourcePrice는 JSON에서 로드됩니다.
        float revenueRateL1 = BASE_RATE_UNIT_PER_SECOND * resource.resourcePrice;
        float costBase = revenueRateL1 * TARGET_UPGRADE_TIME_SECONDS * TIGHTNESS_MULTIPLIER;
        
        // resource.costGrowthMultiplier는 JSON에서 로드됩니다.
        float finalCost = costBase * Mathf.Pow(resource.costGrowthMultiplier, targetLevel - 2);

        return (long)Mathf.Ceil(finalCost);
    }




    // -----------------------------------------------------------
    // 4. 채취 속도 계산 함수 (수정 완료)
    // -----------------------------------------------------------

    public float CalculateGatherRate(ResourceData resource, int currentLevel)
    {
        // resource.rateIncrement는 JSON에서 로드됩니다.
        return BASE_RATE_UNIT_PER_SECOND + (currentLevel - 1) * resource.rateIncrement;
    }


    
    // -----------------------------------------------------------
    // 5. 통합 시간 예측 함수 (Time-to-Level)
    // -----------------------------------------------------------
    
    public float CalculateTotalTimeToLevel(ResourceData resource, int targetLevel)
    {
        if (targetLevel <= 1) return 0f;

        float totalTimeSeconds = 0f;
        
        for (int n = 2; n <= targetLevel; n++)
        {
            long costN = CalculateUpgradeCost(resource, n); 
            
            int previousLevel = n - 1;
            float rateNMinus1 = CalculateGatherRate(resource, previousLevel);
            float revenueRateNMinus1 = rateNMinus1 * resource.resourcePrice;

            if (revenueRateNMinus1 <= 0) // 무한 루프 방지
            {
                return float.PositiveInfinity;
            }

            float timeToNextLevel = (float)costN / revenueRateNMinus1;
            totalTimeSeconds += timeToNextLevel;
        }

        return totalTimeSeconds;
    }


    // -----------------------------------------------------------
    // 4. 핵심 시뮬레이션 함수 (SimulateGrowth)
    // -----------------------------------------------------------

    public List<SimulationStep> SimulateGrowth(ResourceData resource, int targetLevel)
    {
        List<SimulationStep> history = new List<SimulationStep>();
        int currentLevel = 1;
        float cumulativeTimeSeconds = 0f;
        double cumulativeQuantity = 0;

        // 초기 상태 기록
        history.Add(new SimulationStep 
        { 
            Level = currentLevel, 
            CumulativeTimeHours = 0f, 
            GatherRatePerSecond = CalculateGatherRate(resource, currentLevel),
            CumulativeQuantity = 0 
        });

        while (currentLevel < targetLevel)
        {
            int nextLevel = currentLevel + 1;
            
            // 1. 다음 레벨업에 필요한 Cost 계산 (Gold)
            long costN = CalculateUpgradeCost(resource, nextLevel); 
            
            // 2. 현재 레벨에서의 수익률 및 속도 계산
            float rateN = CalculateGatherRate(resource, currentLevel);
            float revenueRateN = rateN * resource.resourcePrice;

            if (revenueRateN <= 0) break; // 무한 루프 방지

            // 3. 다음 레벨까지 걸리는 시간 계산 (Time)
            float timeToNextLevel = (float)costN / revenueRateN;
            
            // 4. 해당 시간 동안의 채취 수량 계산
            double quantityAcquired = timeToNextLevel * rateN;
            
            // 5. 누적 값 업데이트
            cumulativeTimeSeconds += timeToNextLevel;
            cumulativeQuantity += quantityAcquired;
            currentLevel = nextLevel;

            // 6. 기록
            history.Add(new SimulationStep
            {
                Level = currentLevel,
                CumulativeTimeHours = cumulativeTimeSeconds / 3600f,
                GatherRatePerSecond = CalculateGatherRate(resource, currentLevel),
                CumulativeQuantity = cumulativeQuantity
            });
        }
        
        return history;
    }
}

// Sim Result.
/*
자원	Level 마일스톤	누적 시간 (약)	채취 속도 (Units/sec)	총 획득 수량 (누적)
A	    L5	            0.01 시간 (36초)	8.00	                200
(P=1)	L10	            0.05 시간 (3분)	    18.00	              2,500
        L20	            13.9 시간	        38.00	            460,000
B	    L5	            0.15 시간 (9분)	    0.44	                300
(P=20)	L10	            1.5 시간	        1.10	              4,800
        L20	            15.0 시간	        2.20	             50,000
C	    L5	            0.75 시간 (45분)	0.10	              1,000
(P=100)	L10	            7.0 시간	        0.25	             20,000
        L20	            17.5 시간	        0.50	            120,000
*/