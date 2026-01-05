using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace App.GamePlay.IdleMiner
{
    public class DragEventReceiver : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("> Pointer Drag. ");// + _word);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("> Pointer Down.");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("> Pointer Up. ");// + _word);
        }
    }
}
