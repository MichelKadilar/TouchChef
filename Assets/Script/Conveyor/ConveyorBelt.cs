using UnityEngine;
using System.Collections.Generic;
using NativeWebSocket;
using System;

namespace Script.Conveyor
{
    public class ConveyorBelt : MonoBehaviour
    {
        public float speed = 1f;
        private Material material;

        void Start()
        {
            material = GetComponent<Renderer>().material;
        }

        void Update()
        {
            // Animation de la texture uniquement
            Vector2 offset = material.mainTextureOffset;
            offset.x -= Time.deltaTime * speed;
            material.mainTextureOffset = offset;
        }
    }
}