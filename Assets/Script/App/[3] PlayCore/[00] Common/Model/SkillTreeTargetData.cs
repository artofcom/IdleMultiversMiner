using UnityEngine;



namespace App.GamePlay.IdleMiner.Common
{
    public interface ISkillRegistry
    {
    }
    
    public interface ISkillTarget
    {
        void Register(ISkillRegistry registry);
    }
    
    
    public class SkillTreeTargetData : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}