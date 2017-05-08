using System;

namespace Assets.Scripts.Data
{
    [Serializable]
    public struct ChunkData
    {
        public ushort[,,] Blocks;
    }
}