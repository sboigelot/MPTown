using System;
using Assets.Scripts.McChunk;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [Serializable]
    public struct ChunkData
    {
        public RVector3 MapPosition;
        public ushort[,,] Blocks;
    }
}