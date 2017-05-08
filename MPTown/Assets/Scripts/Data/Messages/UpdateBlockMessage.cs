using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.McChunk;

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
