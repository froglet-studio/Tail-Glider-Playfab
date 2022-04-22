﻿using UnityEngine;

namespace Amoebius.Core.Input
{
    public class Gyro : MonoBehaviour
    {
        //[System.Serializable]
        public Transform gyroTransform;

        private bool useGryo = true;

        public SpaceCraftControl2Axis control2Axis;
        

        // Start is called before the first frame update
        void Start()
        {
            
            if (SystemInfo.supportsGyroscope)
            {
                
                UnityEngine.Input.gyro.enabled = true;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (SystemInfo.supportsGyroscope && useGryo)
            {
                //updates GameObjects rotation from input devices gyroscope
                gyroTransform.rotation = GyroToUnity(UnityEngine.Input.gyro.attitude);
            }
        }

        //Coverts Android and Mobile Device Quaterion into Unity Quaterion  TODO: Test
        private Quaternion GyroToUnity(Quaternion q)
        {
            return new Quaternion(q.x, q.y, -q.z, -q.w);
        }

        public void FlipUseGyro()
        {
            //TODO check if isLocalPlayer in multiplayer
            useGryo = Utils.Flip(useGryo);
        }
    }
}

