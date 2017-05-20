using System;

namespace Assets.Scripts.Data
{
    [Serializable]
    public struct ChunkData
    {
        public RVector3 MapPosition;
        public BlockData[,,] Blocks;
    }
}