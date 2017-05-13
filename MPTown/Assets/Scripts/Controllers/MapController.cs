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
        public float BlockSize = 1f;
        public Material ChunckMaterial;
        public Vector3 ChunckSize = new Vector3(16, 16, 16);

        public ushort EditBlockIndex = 10;
        public GameObject Highlighter;

        private MapData mapData;
        public Vector3 MapSize = new Vector3(2, 1, 2);
        public Vector2 TextureBlockSize = new Vector2(128, 128);

        public MapData MapData
        {
            get { return mapData; }

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

            var chunkGameObject = new GameObject("Chunk " + position);
            chunkGameObject.transform.SetParent(transform);
            chunkGameObject.transform.position = chunckWorldPosition;

            var meshFilter = chunkGameObject.AddComponent<MeshFilter>();
            var meshCollider = chunkGameObject.AddComponent<MeshCollider>();
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
        }

        public void Update()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 500))
            {
                if (hit.rigidbody != null)
                {
                    var hitPos = hit.point;

                    var cubeWorldPos = GetCubeWorldPosFromHit(hitPos);
                    if (Highlighter != null)
                    {
                        var hs = BlockSize / 2;
                        Highlighter.transform.position = cubeWorldPos + new Vector3(hs,hs,hs);
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        ClickChunck(hit.collider.gameObject, cubeWorldPos, EditBlockIndex);
                    }

                    if (Input.GetMouseButtonDown(1))
                    {
                        ClickChunck(hit.collider.gameObject, cubeWorldPos, 0);
                    }
                }
            }
        }

        private Vector3 GetCubeWorldPosFromHit(Vector3 hitPos)
        {
            var hitPosCube = new RVector3(
                Mathf.FloorToInt(hitPos.x),
                Mathf.FloorToInt(hitPos.y),
                Mathf.FloorToInt(hitPos.z));

            //TODO if face x-1 or face z-1 change vx & vy
            
            int vx = 0;
            if (Input.GetKey(KeyCode.LeftControl))
                vx -= (int)BlockSize;

            int vy = 0;
            if (Input.GetKey(KeyCode.LeftShift))
                vy -= (int)BlockSize;

            int vz = 0;
            if (Input.GetKey(KeyCode.LeftAlt))
                vz -= (int)BlockSize;
            
            return new RVector3(hitPosCube.x + vx, hitPosCube.y + vy, hitPosCube.z +vz);
        }

        private void ClickChunck(GameObject chunkGameObject, Vector3 cubeWorldPos, ushort editBlockIndex)
        {
            //TODO we have to build in an adjacent chunk if we go outside of the chunk boudaries

            var chunkController = chunkGameObject.GetComponent<ChunkController>();
            var chunk = chunkController.ChunkData;

            var chunkRelativeWorldPos = cubeWorldPos - chunkController.gameObject.transform.position;
            var cubeChunkIndex = new RVector3(chunkRelativeWorldPos);

            //Debug.LogFormat("Hit chunk {2} {0} at cube {1}", chunk.MapPosition, cubeChunkIndex, chunkRelativeWorldPos);
            chunkController.SetBlock(cubeChunkIndex, editBlockIndex);
        }

        private Vector3 ChunkToWorldPosition(Vector3 position)
        {
            return new Vector3(
                position.x * BlockSize * ChunckSize.x,
                position.y * BlockSize * ChunckSize.y,
                position.z * BlockSize * ChunckSize.z);
        }

        private Vector3 BlockToChunkRelativeWorldPosition(Vector3 position)
        {
            return new Vector3(
                position.x * BlockSize,
                position.y * BlockSize,
                position.z * BlockSize);
        }

        public void Initialize()
        {
            if (IsSinglePlayer || NetworkBus.IsServer)
            {
                InitMapData();
            }
        }

        private void InitMapData()
        {
            MapData = new MapGenerator().GenerateMap(MapSize, ChunckSize, Random.Range(100, 10000), 16f);
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