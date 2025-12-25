using System;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "SpriteConfig", menuName = "ScriptableObjects/SpriteConfig")]
public class SpriteConfig : ScriptableObject
{
    [Serializable]
    public class SpriteInfo
    {
        [SerializeField] string key;
        [SerializeField] Sprite sprite;

        public string Key => key;
        public Sprite Sprite => sprite;
    }

    [SerializeField] List<SpriteInfo> sprites;
    
    void OnEnable() {}


    public Sprite GetSprite(string key)
    {
        for(int q = 0; q < sprites.Count; q++) 
        {
            if(string.Compare(key, sprites[q].Key, ignoreCase: true) == 0)
                return sprites[q].Sprite;
        }
        return null;
    }
}
