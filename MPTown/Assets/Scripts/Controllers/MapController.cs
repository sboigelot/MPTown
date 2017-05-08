using System;
using Assets.Scripts.Data;
using Assets.Scripts.Helpers;
using Assets.Scripts.McChunk;
using Assets.Scripts.Network;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Controllers
{
    public class MapController : NetworkBusUser
    {
        public Vector3 ChunckSize = new Vector3(16,16,16);
        public Vector3 MapSize = new Vector3(2, 1, 2);
        public float BlockSize = 1f;
        public Material ChunckMaterial;

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

            this.ForXyz(
                mapData.Chunks.GetLength(0), 
                mapData.Chunks.GetLength(1), 
                mapData.Chunks.GetLength(2),
                (x, y, z) =>
                {
                    var chunk = MapData.Chunks[x, y, z];
                    RenderChunk(chunk, new Vector3(x, y, z));
                });
        }

        private void RenderChunk(ChunkData chunk, Vector3 position)
        {
            var chunckWorldPosition = ChunkToWorldPosition(position);

            if (chunk.Blocks == null)
            {
                return;
            }

            var chunkGameObject = new GameObject("Chunk "+ position);
            chunkGameObject.transform.SetParent(transform);
            chunkGameObject.transform.position = chunckWorldPosition;

            var meshFilter = chunkGameObject.AddComponent<MeshFilter>();
            var meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = ChunckMaterial;
            var chunckController = chunkGameObject.AddComponent<ChunkController>();
            chunckController.TextureBlockSize = new Vector2(128, 128);
            chunckController.ChunkData = chunk;
            chunckController.Size = new RVector3(ChunckSize);
            chunckController.Position = new RVector3(position);
        }

        private Vector3 ChunkToWorldPosition(Vector3 position)
        {
            return new Vector3(
                position.x * BlockSize * ChunckSize.x,
                position.y * BlockSize * ChunckSize.z,
                position.z * BlockSize * ChunckSize.z);
        }

        private Vector3 BlockToWorldPosition(Vector3 position)
        {
            return new Vector3(
                position.x * BlockSize,
                position.y * BlockSize,
                position.z * BlockSize);
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
            MapData = new MapGenerator().GenerateMap(MapSize, ChunckSize, Random.Range(100,10000), 16f);
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