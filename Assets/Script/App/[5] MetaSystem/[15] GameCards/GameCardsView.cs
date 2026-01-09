using IGCore.MVCS;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameCardsView : AView
{
    [SerializeField] Transform CardsRoot;


    public Action<string> EventGameCardClicked;

    public class Presentor : APresentor
    {
        public Presentor(List<Tuple<string, AView.APresentor>> gameCardsPresentor)
        {
            GameCardsPresentor = gameCardsPresentor;
        }   

        public List<Tuple<string, AView.APresentor>> GameCardsPresentor { get; private set; }
    }

    Dictionary<string, GameCardComp> dictGameCards = new Dictionary<string, GameCardComp>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assert.IsNotNull(CardsRoot);
        Init();
    }

    void Init()
    {
        dictGameCards.Clear();
        for(int q = 0; q < CardsRoot.childCount; ++q)
        {
            GameCardComp card = (CardsRoot.GetChild(q)).GetComponent<GameCardComp>();
            if(card == null)
                continue;

            dictGameCards.Add(card.GameKey.ToLower(), card);
        }
    }

    public override void Refresh(APresentor presentData)
    {
        Presentor presentor = presentData as Presentor;
        if(presentor == null)
            return;

        if(dictGameCards.Count == 0)
            Init();

        HashSet<string> processedCards = new HashSet<string>();
        for(int q = 0; q < presentor.GameCardsPresentor.Count; ++q) 
        {
            string gameKey = (presentor.GameCardsPresentor[q].Item1).ToLower();
            if(dictGameCards.ContainsKey( gameKey ))
            {
                dictGameCards[gameKey].Refresh(presentor.GameCardsPresentor[q].Item2);
                processedCards.Add(gameKey);
            }
            else 
                Debug.LogWarning($"Could not find the game key from the Lobby Cards ..[{gameKey}]");
        }

        // Now take care of the others that don't have data/presentor.
        foreach(var card in dictGameCards)
        {
            if(!processedCards.Contains(card.Key))
            {
                card.Value.Refresh(null);
                processedCards.Add(card.Key);
            }
        }
    }

    public AView GetGameCardView(string gameKey)
    {
        if(dictGameCards.ContainsKey(gameKey.ToLower()))
            return dictGameCards[gameKey.ToLower()];

        return null;
    }

    public void OnBtnGameCardClicked(string gameKey)
    {
        EventGameCardClicked?.Invoke(gameKey);
    }
}
