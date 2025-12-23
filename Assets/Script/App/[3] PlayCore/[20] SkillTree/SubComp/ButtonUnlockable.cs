using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ButtonUnlockable : AUnlockable
{
    [SerializeField] Button btnMain;
    [SerializeField] Image icon;

    bool bInteractable;
    public bool Interactable
    {
        get => bInteractable;
        set
        {
            bInteractable = value;
            btnMain.interactable = bInteractable;
            icon.color = bInteractable ? Color.white : Color.gray;
        }
    }

    private void Awake()
    {
        Assert.IsNotNull(btnMain);
        Assert.IsNotNull(icon);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
