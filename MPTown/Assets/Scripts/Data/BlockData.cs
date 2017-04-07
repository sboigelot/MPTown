using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Data
{
    [Serializable]
    public class BlockData
    {
        public int BlockIndex;
        public int TextureIndex;
        public int MeshIndex;

        public InventoryData Inventory;
    }
}
