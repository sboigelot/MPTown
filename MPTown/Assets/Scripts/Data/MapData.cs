using System;

namespace Assets.Scripts.Data
{
    [Serializable]
    public class MapData
    {
        public ChunkData[,,] Chunks;
        public float[,] ChunkHeights { get; set; }
    }
}