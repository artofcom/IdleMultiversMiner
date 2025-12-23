using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using UnityEngine.UI;

namespace App.GamePlay.IdleMiner
{
    public class StarGradeComp : MonoBehaviour
    {
        [SerializeField] Sprite On, Off;
        
        public class PresentInfo
        {
            // Assigned Slot.
            public PresentInfo(int grade)
            {
                Grade = grade;
            }

            public int Grade { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(On);
            Assert.IsNotNull(Off);
        }

        public void Refresh(PresentInfo presentor)
        {
            if (presentor == null)
                return;

            int idx = 0;
            for(int q = 0; q < transform.childCount; ++q)
            {
                Image imgGrade = transform.GetChild(q).GetComponent<Image>();
                if (imgGrade == null)
                    continue;

                imgGrade.sprite = idx < presentor.Grade ? On : Off;
                idx++;
            }
        }
    }
}
