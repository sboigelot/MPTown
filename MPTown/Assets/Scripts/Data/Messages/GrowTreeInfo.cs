using System;

namespace Assets.Scripts.Data.Messages
{
    [Serializable]
    public struct GrowTreeInfo
    {
        public ushort ChunkX;
        public ushort ChunkZ;
        public ushort InChunkX;
        public ushort InChunkZ;
        public ushort InChunkY;
        public float Scale;
        public ushort TreeType;
    }
}