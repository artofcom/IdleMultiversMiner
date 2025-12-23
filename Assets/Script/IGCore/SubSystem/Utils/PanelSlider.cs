using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;

namespace Core.Utils
{
    public class PanelSlider : MonoBehaviour
    {
        [Serializable]
        public class SliderablePanel
        {
            [SerializeField] Transform highPanel;
            [SerializeField] Transform lowPanel;
            [SerializeField] RectTransform mainPanel;

            public Transform HighPanel => highPanel;
            public Transform LowPanel => lowPanel;
            public RectTransform MainPanel => mainPanel;
        }

        [SerializeField] List<SliderablePanel> Panels;

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsTrue(Panels != null && Panels.Count > 0);
        }

        public void SlidePanel(bool isUp)
        {
            for (int q = 0; q < Panels.Count; ++q)
            {
                Panels[q].HighPanel.gameObject.SetActive(false);
                Panels[q].LowPanel.gameObject.SetActive(false);

                Transform to = isUp ? Panels[q].HighPanel : Panels[q].LowPanel;
                to.gameObject.SetActive(true);

                Panels[q].MainPanel.SetParent(to);
                Panels[q].MainPanel.offsetMin = Vector2.zero;
                Panels[q].MainPanel.offsetMax = Vector2.zero;
            }
        }
    }
}
