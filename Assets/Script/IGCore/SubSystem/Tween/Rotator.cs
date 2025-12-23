using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core.Tween
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] float Power = 10.0f;

        float fZAngle = .0f;

        void Start()
        {

        }

        private void Update()
        {
            fZAngle += (Time.deltaTime * Power);

            Quaternion rot = Quaternion.Euler(.0f, .0f, fZAngle);
            transform.rotation = rot;
        }
    }

}