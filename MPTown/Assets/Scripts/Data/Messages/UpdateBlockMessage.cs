using System;

namespace Assets.Scripts.Data.Messages
{
    [Serializable]
    public class UpdateBlockMessage
    {
        public RVector3 ChunkPosition;
        public RVector3 BlockPosition;
        public ushort BlockType;
    }
}
