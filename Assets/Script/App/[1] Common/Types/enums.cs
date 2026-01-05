
namespace App.GamePlay.IdleMiner.Common.Types
{
    public enum eABILITY { MINING_RATE = 0, DELIVERY_SPEED, CARGO_SIZE, SHOT_ACCURACY, SHOT_INTERVAL, MAX };
    
    public enum AccountStatus
    {
        UNKNOWN,
        NULL_2_ID_A, 
        NULL_2_NULL, 
        DEVICE_ID_2_NULL, 
        DEVICE_ID_2_ID_A,
        ID_A_2_NULL, 
        ID_A_2_ID_A, 
        ID_A_2_ID_B,
    }
}
