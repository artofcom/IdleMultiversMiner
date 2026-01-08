
namespace App.GamePlay.IdleMiner.Common.Types
{
    public class EventID 
    {
        public const string APPLICATION_FOCUSED = "ApplicationFocused";
        public const string APPLICATION_PAUSED = "ApplicationPaused";

        public const string PLAYER_HAS_SIGNEDIN_OR_TIMED_OUT = "OnPlayerHasSignedIn_Or_TimedOut";

        public const string IAP_MONEY_CHANGED = "IAP_MoneyAmountChanged";
        public const string STAR_AMOUNT_CHANGED = "StarAmountChanged";
        public const string SETTING_BGM_CHANGED = "SettingBGMChanged";
        public const string SETTING_SOUND_FX_CHANGED = "SettingSoundFXChanged";
        
        public const string GAME_LEVEL_START = "GameLevelStart";

        public const string CRAFT_SUCCESSED = "CraftSuccessed";
        public const string CRAFT_SLOT_EXTENDED = "CraftSlotExtended";
        public const string CRAFT_RECIPE_PURCHASED = "CraftRecipePurchased";
        public const string CRAFT_RECIPE_ASSIGNED = "CraftRecipeAssigned";

        public const string RESOURCE_UPDATED = "ResourceUpdated";
        public const string SKILL_LEARNED = "SkillLearned";
        
        public const string SKILL_RESET_GAME_INIT = "SkillResetGameInit";
        public const string GAME_RESET_REFRESH = "GameResetRefreshAllView";
        
        public const string GAME_CURRENCY_UPDATED = "GameCurrencyUpdated";
        public const string MINING_STAT_RESET = "MinigStatReset";
        public const string MINING_STAT_UPGRADED = "MiningStatUpgraded";

        public const string ZONE_UNLOCKED = "ZoneUnlocked";
        public const string PLANET_UNLOCKED = "PlanetUnlocked";
        public const string PLANET_DAMAGED = "PlanetDamaged";
        public const string PLANET_CLOSED = "PlanetClosed";
        public const string PLANET_BATTLE_CLEARED = "PlanetBattleCleared";

        public const string PLANET_BOOSTER_TRIGGERED = "PlanetBoosterTriggered";
        public const string PLANET_BOOSTER_FINISHED = "PlanetBoosterFinished";

        public const string ADS_WATCHED = "AdsWatched";
        public const string DAILY_MISSION_GOAL_ACHIEVED = "DailyMissionGoalAchieved";
        public const string DAILY_MISSION_RESET = "DailyMissionReset";

        // Main Notification.
        // public const string LEARNABL_SKILL_FOUND = "LearnableSkillFound";
    }
}
