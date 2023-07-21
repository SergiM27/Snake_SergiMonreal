using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitFPS : MonoBehaviour
{
        public int target = 60;

        void Awake()
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = target;
        }

        void LateUpdate()
        {
            if (Application.targetFrameRate != target)
                Application.targetFrameRate = target;
        }
}
