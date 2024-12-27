﻿using UnityEngine;

namespace CosmicShore
{
    public struct GyroidBondMateData
    {
        public CornerSiteType Substrate;
        public CornerSiteType Bondee;
        public Vector3 DeltaPosition;
        public Vector3 DeltaUp;
        public Vector3 DeltaForward;
        public GyroidBlockType BlockType;
        public bool isTail;
    }
}

