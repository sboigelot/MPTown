using Assets.Scripts.Data;
using Assets.Scripts.Helpers;
using Assets.Scripts.Network;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class MapController : NetworkBusUser
    {
        private MapData mapData;
        public MapData MapData
        {
            get
            {
                return mapData;
            }

            set
            {
                if (mapData == value)
                {
                    return;
                }

                mapData = value;
                OnMapDataChanged();
            }
        }
        
        private void OnMapDataChanged()
        {        
            InvalidateVisual();
            SendIfServer(mapData);
        }

        private void InvalidateVisual()
        {
            transform.ClearChildren();

            var chunk = MapData.Chunks[0, 0];
            var blocks = chunk.Blocks;

            for (int x = 0; x < blocks.GetLength(0); x++)
            {
                for (int y = 0; y < blocks.GetLength(1); y++)
                {
                    for (int z = 0; z < blocks.GetLength(2); z++)
                    {
                        if (blocks[x, y, z].BlockIndex != 0)
                        {
                            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            cube.transform.SetParent(transform);
                            cube.transform.position = new Vector3(x, y, z);
                            cube.transform.localScale = Vector3.one;
                        }
                    }
                }
            }
        }
        
        public void Initialize()
        {
            if(NetworkBus.IsServer)
            {
                InitMapData();
            }
        }

        private void InitMapData()
        {
            var map = new MapData
            {
                Chunks = new ChunkData[1, 1]
            };

            map.Chunks[0, 0] = new ChunkData
            {
                Blocks = new BlockData[10, 5, 10]
            };

            var chunk = map.Chunks[0, 0];
            var blocks = chunk.Blocks;

            for (int x = 0; x < blocks.GetLength(0); x++)
            {
                for (int y = 0; y < blocks.GetLength(1); y++)
                {
                    for (int z = 0; z < blocks.GetLength(2); z++)
                    {
                        blocks[x, y, z] = new BlockData
                        {
                            BlockIndex = Random.Range(0, 2),
                        };
                    }
                }
            }

            MapData = map;
        }

        protected override void RegisterMessageHandlers()
        {
            RegisterMessageHandler<MapData>(OnMapDataReceived);
        }

        private void OnMapDataReceived(NetworkBusEnvelope envelope)
        {
            if (!NetworkBus.IsServer)
            {
                MapData = envelope.Open<MapData>();
            }
        }
    }
}