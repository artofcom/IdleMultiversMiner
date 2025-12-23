using UnityEngine;
using System.Collections;
using IGCore.MVCS;

public interface IContext 
{
    IEnumerator Init(MonoBehaviour monoObject, AView popupDialogView);
}
