using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Utils
{
    public class Aimer : MonoBehaviour
    {
        [SerializeField] Transform transformAimTo;

        Vector3 vOldPos;

        public void SetAimTarget(Transform transformTarget)
        {
            transformAimTo = transformTarget;
        }

        // Start is called before the first frame update
        void Start() { }

        private void OnEnable()
        {
            transform.rotation = Quaternion.identity;
        }

        // Update is called once per frame
        void Update()
        {
            if (transformAimTo != null)
            {
                Vector3 vNewDir = transform.localPosition - vOldPos;
                Vector2 vDir = new Vector2(vNewDir.x, vNewDir.y);
                vDir.Normalize();
                transform.rotation = Quaternion.FromToRotation(Vector3.up, vDir);
            }

            vOldPos = transform.localPosition;
        }
    }
}