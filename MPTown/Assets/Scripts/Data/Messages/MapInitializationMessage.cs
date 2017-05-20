using System;

namespace Assets.Scripts.Data.Messages
{
    [Serializable]
    public class MapInitializationMessage
    {
        public RVector3 ChunckSize;
        public RVector3 MapSize;
        public int Seed;
        public int Intensity;
        public int TreeDensityPerChunk;
    }
}