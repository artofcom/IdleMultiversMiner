using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Assertions;

namespace FrameCore.UI
{
    public class TabButtons : MonoBehaviour
    {
        [SerializeField] List<Button> ListNormalBtns = new List<Button>();
        [SerializeField] List<Button> ListActivatedBtns = new List<Button>();
     //   [SerializeField] List<GameObject> ListViews = new List<GameObject>();
        [SerializeField] bool AllowUnTabAll = false;

        public int SelectedIndex
        {
            get { return mSelectedIndex; }
            set
            {
                int idx = value;
                SelectButton(idx);
                if (mSelectedIndex != idx)
                {
                    mSelectedIndex = idx;
                    OnSelectionChanged?.Invoke(idx);
                    Debug.Log($"[TabButton] : Tab Selected : [{idx}].");
                }
            }
        }
        public int TabCount => ListNormalBtns.Count;

        [HideInInspector] public UnityEvent<int> OnSelectionChanged;

        int mSelectedIndex = -1;

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(ListNormalBtns);
            Assert.IsNotNull(ListActivatedBtns);
            Assert.IsTrue(ListNormalBtns.Count == ListActivatedBtns.Count);
        }

        public void CloseAll()
        {
            for (int q = 0; q < ListNormalBtns.Count; ++q)
            {
                ListNormalBtns[q].gameObject.SetActive(true);
                ListActivatedBtns[q].gameObject.SetActive(false);

               // if(q<ListViews.Count && ListViews[q]!=null)
               //     ListViews[q].gameObject.SetActive(false);
            }

            SelectedIndex = -1;
        }

        public void OnClicked(Button clickedTarget)
        {
            for(int q = 0; q < ListNormalBtns.Count; ++q)
            {
                if(clickedTarget == ListNormalBtns[q])
                {
                    SelectedIndex = q;
                    return;
                }
            }

            for (int q = 0; q < ListActivatedBtns.Count; ++q)
            {
                if (clickedTarget == ListActivatedBtns[q])
                {
                    if (AllowUnTabAll)
                        CloseAll();
                    break;
                }
            }
        }

        void SelectButton(int idx)
        {
            // if idx was set out of boundary, then all will go de-activated. 
            for (int q = 0; q < ListNormalBtns.Count; ++q)
            {
                ListNormalBtns[q].gameObject.SetActive(idx != q);
                ListActivatedBtns[q].gameObject.SetActive(idx == q);

              //  if (q < ListViews.Count && ListViews[q] != null)
               //     ListViews[q].gameObject.SetActive(idx == q);
            }
        }
    }
}
