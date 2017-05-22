using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.Data.Messages;
using Assets.Scripts.Helpers;
using Assets.Scripts.Navigation;
using Assets.Scripts.Network;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Controllers
{
    [RequireComponent(typeof(NavWeb))]
    public class MapController : NetworkBusUser
    {
        public float BlockSize = 1f;
        public Material ChunckMaterial;
        public Vector3 ChunkSize = new Vector3(16, 16, 16);

        public ushort EditBlockIndex = 10;
        public GameObject Highlighter;

        private MapData mapData;
        public Vector3 MapSize = new Vector3(2, 1, 2);
        public Vector2 TextureBlockSize = new Vector2(128, 128);

        public GameObject[] TreePrefabs;

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
                InvalidateVisual();
            }
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
            chunckController.Size = new RVector3(ChunkSize);
            chunckController.TextureBlockSize = TextureBlockSize;
            chunckController.BlockSize = BlockSize;

            var rigidBody = chunkGameObject.GetComponent<Rigidbody>() ?? chunkGameObject.AddComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
        }

        private RVector3 debugOriginForPath;
        private RVector3 debugDestinationForPath;
        private List<RVector3> debugPath;

        public void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

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
                        Highlighter.transform.position = cubeWorldPos + new Vector3(hs, hs, hs);
                    }

                    if (Input.GetKeyUp(KeyCode.G))
                    {
                        debugPath = new List<RVector3>();
                        debugOriginForPath = new RVector3(cubeWorldPos);
                    }

                    if (Input.GetKeyDown(KeyCode.H))
                    {
                        debugDestinationForPath = new RVector3(cubeWorldPos);
                        debugPath = GetComponent<NavWeb>().FindPath(debugOriginForPath, debugDestinationForPath);
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

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            
            if(debugOriginForPath!=null)
            Gizmos.DrawCube(new Vector3(debugOriginForPath.x + .5f, debugOriginForPath.y, debugOriginForPath.z + .5f), Vector3.one * .2f);
            if(debugDestinationForPath!=null)
            Gizmos.DrawCube(new Vector3(debugDestinationForPath.x + .5f, debugDestinationForPath.y, debugDestinationForPath.z + .5f), Vector3.one * .2f);

            if (debugPath == null || !debugPath.Any())
                return;
            
            var o = debugPath[0];
            var previous = new Vector3(o.x + .5f, o.y, o.z + .5f);
            foreach (RVector3 v in debugPath.Skip(1))
            {
                var current = new Vector3(v.x + .5f, v.y, v.z + .5f);
                Gizmos.DrawLine(previous, current);
                previous = current;
            }
        }

        private Vector3 GetCubeWorldPosFromHit(Vector3 hitPos)
        {
            var hitPosCube = new RVector3(
                Mathf.FloorToInt(hitPos.x),
                Mathf.FloorToInt(hitPos.y),
                Mathf.FloorToInt(hitPos.z));

            //TODO if face x-1 or face z-1 change vx & vy

            var vx = 0;
            if (Input.GetKey(KeyCode.LeftControl))
                vx -= (int) BlockSize;

            var vy = 0;
            if (Input.GetKey(KeyCode.LeftShift))
                vy -= (int) BlockSize;

            var vz = 0;
            if (Input.GetKey(KeyCode.LeftAlt))
                vz -= (int) BlockSize;

            return new RVector3(hitPosCube.x + vx, hitPosCube.y + vy, hitPosCube.z + vz);
        }

        private void ClickChunck(GameObject chunkGameObject, Vector3 cubeWorldPos, ushort editBlockIndex)
        {
            var chunkController = chunkGameObject.GetComponent<ChunkController>();
            var chunk = chunkController.ChunkData;
            var chunkIndex = chunk.MapPosition;

            var chunkRelativeWorldPos = cubeWorldPos - chunkController.gameObject.transform.position;
            var cubeChunkIndex = new RVector3(chunkRelativeWorldPos);
            
            if (cubeChunkIndex.x < 0)
            {
                cubeChunkIndex.x += (int)ChunkSize.x;
                chunkIndex.x -= 1;
                chunkController = GetChunkAtPosition(chunkIndex);
                chunk = chunkController.ChunkData;
            }
            if (cubeChunkIndex.z < 0)
            {
                cubeChunkIndex.z += (int)ChunkSize.x;
                chunkIndex.z -= 1;
                chunkController = GetChunkAtPosition(chunkIndex);
                chunk = chunkController.ChunkData;
            }
            if (cubeChunkIndex.x >= ChunkSize.x)
            {
                cubeChunkIndex.x -= (int)ChunkSize.x;
                chunkIndex.x += 1;
                chunkController = GetChunkAtPosition(chunkIndex);
                chunk = chunkController.ChunkData;
            }
            if (cubeChunkIndex.z >= ChunkSize.z)
            {
                cubeChunkIndex.z -= (int)ChunkSize.x;
                chunkIndex.z += 1;
                chunkController = GetChunkAtPosition(chunkIndex);
                chunk = chunkController.ChunkData;
            }
            
            //Debug.LogFormat("Hit chunk {2} {0} at cube {1}", chunk.MapPosition, cubeChunkIndex, chunkRelativeWorldPos);
            if (chunkController != null)
            {
                chunkController.SetBlock(cubeChunkIndex, editBlockIndex);
                GetComponent<NavWeb>()
                    .RecalculateBlockNavigation(
                        cubeChunkIndex.x,
                        cubeChunkIndex.y,
                        cubeChunkIndex.z,
                        chunk.Blocks,
                        new RVector3(chunkIndex.x * ChunkSize.x,
                            chunkIndex.y * ChunkSize.y,
                            chunkIndex.z * ChunkSize.z));
            }
        }

        private ChunkController GetChunkAtPosition(RVector3 chunkIndex)
        {
            return GetComponentsInChildren<ChunkController>().FirstOrDefault(
                c => c.ChunkData.MapPosition == chunkIndex
            );
        }

        private Vector3 ChunkToWorldPosition(Vector3 position)
        {
            return new Vector3(
                position.x * BlockSize * ChunkSize.x,
                position.y * BlockSize * ChunkSize.y,
                position.z * BlockSize * ChunkSize.z);
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
                SendIfServer(new MapInitializationMessage
                {
                    ChunckSize = new RVector3(ChunkSize),
                    MapSize = new RVector3(MapSize),
                    Seed = Random.Range(100, 10000),
                    Intensity = (int)ChunkSize.y * 3 / 4,
                    TreeDensityPerChunk = Random.Range(2, 7)
                });
            }
        }

        protected override void RegisterMessageHandlers()
        {
            RegisterMessageHandler<MapInitializationMessage>(OnMapInitializationMessageReceived);
            RegisterMessageHandler<GrowTreeMessage>(OnGrowTreeMessageReceived);
        }

        public void OnMapInitializationMessageReceived(object payload)
        {
            var init = (MapInitializationMessage)payload;
            MapData = new MapGenerator().GenerateMap(init.MapSize, init.ChunckSize, init.Seed, init.Intensity);
            GenerateTreePosition(init);
        }

        private void GenerateTreePosition(MapInitializationMessage init)
        {
            if (IsSinglePlayer || NetworkBus.IsServer)
            {
                int numberOfTrees = init.TreeDensityPerChunk * init.MapSize.x * init.MapSize.z;

                var growTreeMessage = new GrowTreeMessage
                {
                    GrowTreeInfos = new List<GrowTreeInfo>()
                };

                for (int i = 0; i <= numberOfTrees; i++)
                {
                    var info = new GrowTreeInfo
                    {
                        ChunkX = (ushort)Random.Range(0, init.MapSize.x),
                        ChunkZ = (ushort)Random.Range(0, init.MapSize.z),
                        InChunkX = (ushort)Random.Range(0, init.ChunckSize.x),
                        InChunkZ = (ushort)Random.Range(0, init.ChunckSize.z),
                        Scale = Random.Range(0.6f, 1.4f),
                        TreeType = (ushort)Random.Range(0, TreePrefabs.Length)
                    };
                    info.InChunkY = GetTopBlockHeight(info.ChunkX, info.ChunkZ, info.InChunkX, info.InChunkZ);
                    growTreeMessage.GrowTreeInfos.Add(info);
                }

                SendIfServer(growTreeMessage);
            }
        }

        private ushort GetTopBlockHeight(int chunkX, int chunkZ, int inChunkX, int inChunkZ)
        {
            var h = (ushort)(MapData.ChunkHeights[
                                                  chunkX * (int)ChunkSize.x +
                                                  inChunkX,
                                                  chunkZ * (int)ChunkSize.z +
                                                  inChunkZ] + 1);
            return h;
        }

        private void OnGrowTreeMessageReceived(object payload)
        {
            var message = (GrowTreeMessage)payload;

            foreach (var info in message.GrowTreeInfos)
            {
                var treeData = new BlockObjectData
                {
                    ObjectType = "tree",
                    SubType = info.TreeType
                };
                var chunk = MapData.Chunks[info.ChunkX, 0, info.ChunkZ];
                var block = chunk.Blocks[info.InChunkX, info.InChunkY, info.InChunkZ];
                block.ObjectDataData = treeData;

                var tree = Instantiate(TreePrefabs[info.TreeType]);
                tree.transform.SetParent(this.transform);

                var blockObjectController = tree.GetComponent<BlockObjectController>() ??
                                            tree.AddComponent<BlockObjectController>();
                blockObjectController.Data = treeData;

                var chunkPosition =
                    ChunkToWorldPosition(
                        new Vector3(
                            info.ChunkX,
                            0,
                            info.ChunkZ));

                var inChunkPosition =
                    BlockToChunkRelativeWorldPosition(
                        new Vector3(
                            info.InChunkX,
                            info.InChunkY,
                            info.InChunkZ));


                var anchor = chunkPosition + inChunkPosition;
                tree.transform.localScale = new Vector3(info.Scale, info.Scale, info.Scale);
                tree.transform.position = anchor;
            }

            GetComponent<NavWeb>().InitializeFromMapData(MapData);
        }
    }
}