using System;
using Assets.Scripts.Data;
using Assets.Scripts.Helpers;
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
        public GameObject Highlighter;
        public Vector2 TextureBlockSize = new Vector2(128, 128);

        public ushort EditBlockIndex = 10;

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
            chunckController.ChunkData = chunk;
            chunckController.Size = new RVector3(ChunckSize);
            chunckController.Position = new RVector3(position);
            chunckController.TextureBlockSize = TextureBlockSize;
            chunckController.BlockSize = BlockSize;

            var rigidBody = chunkGameObject.GetComponent<Rigidbody>() ?? chunkGameObject.AddComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;

            var meshCollider = chunkGameObject.AddComponent<MeshCollider>();
        }

        public void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
                
            if (Physics.Raycast(ray, out hit, 500))
            {
                if (hit.rigidbody != null)
                {
                    var hitPos = hit.point;

                    if (Highlighter != null)
                    {
                        //var hitPosCube = new RVector3(hitPos);
                        //Highlighter.transform.position = hitPosCube;// + new Vector3(BlockSize / 2, BlockSize / 2, BlockSize / 2);

                        var hitPosCube = new RVector3(hitPos);
                        var variation = new Vector3(
                            hitPos.x % BlockSize,
                            hitPos.y % BlockSize,
                            hitPos.z % BlockSize);
                        var bs = BlockSize;
                        var hs = bs / 2;
                        Func<float, float> align = (float v) => variation.x < 0 ? hs : -hs;
                        var vx = align(variation.x);
                        var vy = align(variation.y);
                        var vz = align(variation.z);
                        Highlighter.transform.position = hitPosCube + new Vector3(vx, -vy, vz);

                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        ClickChunck(hit.collider.gameObject, hitPos, EditBlockIndex);
                    }

                    if (Input.GetMouseButtonDown(1))
                    {
                        ClickChunck(hit.collider.gameObject, hitPos, 0);
                    }
                }
            }
        }
        
        private void ClickChunck(GameObject chunkGameObject, Vector3 hitPos, ushort editBlockIndex)
        {
            var chunkController = chunkGameObject.GetComponent<ChunkController>();
            var chunk = chunkController.ChunkData;

            var chunkRelativeHitPos = hitPos - ChunkToWorldPosition(chunk.MapPosition);
            var hitPosCube = new RVector3(chunkRelativeHitPos);

            Debug.LogFormat("Hit chunk {0} at cube {1}", chunk.MapPosition, hitPosCube);
            
            var buildPosCube = new RVector3(hitPosCube.x, hitPosCube.y, hitPosCube.z);
            chunkController.SetBlock(buildPosCube, editBlockIndex);
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

        private void OnMapDataReceived(object payload)
        {
            if (!NetworkBus.IsServer)
            {
                MapData = payload as MapData;
            }
        }
    }
}