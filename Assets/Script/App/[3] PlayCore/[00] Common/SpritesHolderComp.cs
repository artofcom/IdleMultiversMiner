using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;

public sealed class SpritesHolderComp : MonoBehaviour
{
    [SerializeField] List<Sprite> sprites;


    Dictionary<string, Sprite> spriteDict;

    // Start is called before the first frame update
    void Start()
    {
        Assert.IsTrue(sprites != null && sprites.Count > 0);

        spriteDict = new Dictionary<string, Sprite>();
        for(int q = 0; q < sprites.Count; ++q)
        {
            Assert.IsTrue(!spriteDict.ContainsKey(sprites[q].name), sprites[q].name);

            spriteDict.Add(sprites[q].name.ToLower(), sprites[q]);
            Debug.Log($"{q}:{sprites[q].name} sprite has been added to the dictionary.");
        }
    }


    public Sprite GetSprite(string key)
    {
        if (spriteDict.ContainsKey(key))
            return spriteDict[key];

        Debug.LogWarning("Couldn't find the sprite key : " + key);
        return null;
    }

}
