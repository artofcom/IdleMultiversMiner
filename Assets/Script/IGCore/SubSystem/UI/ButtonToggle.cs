using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ButtonToggle : MonoBehaviour
{
    [SerializeField] Button btnOn;
    [SerializeField] Button btnOff;
    
    public bool IsOn
    {
        get => btnOn.gameObject.activeSelf && !btnOff.gameObject.activeSelf;
        set
        {
            btnOn.gameObject.SetActive(value);
            btnOff.gameObject.SetActive(!value);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assert.IsNotNull(btnOn);
        Assert.IsNotNull(btnOff);
    }
}
