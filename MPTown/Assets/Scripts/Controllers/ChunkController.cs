using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.Data.Messages;
using Assets.Scripts.Network;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class ChunkController : NetworkBusUser
    {
        public ChunkData ChunkData;
        public Vector2 TextureBlockSize;

        public Mesh chunkMesh;
        public RVector3 Position;
        public RVector3 Size;

        public float BlockSize;

        private List<int> triangles = new List<int>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<Vector3> verticies = new List<Vector3>();

        private int verticiesIndex;
        private Texture textureAtlas;
        private Vector3 atlasSize;
        private Vector2 textureInterval;

        public override void Start()
        {
            base.Start();
            textureAtlas = transform.GetComponent<MeshRenderer>().material.mainTexture;
            atlasSize = new Vector2(textureAtlas.width / TextureBlockSize.x, textureAtlas.height / TextureBlockSize.y);
            textureInterval = new Vector2(1 / atlasSize.x, 1 / atlasSize.y);
            chunkMesh = GetComponent<MeshFilter>().mesh;
            UpdateChunk();
        }

        public void UpdateChunk()
        {
            verticies = new List<Vector3>();
            uvs = new List<Vector2>();
            triangles = new List<int>();

            chunkMesh.Clear();

            var chunkBlocks = ChunkData.Blocks;

            for (var yi = 0; yi < Size.y; yi++)
            {
                for (var xi = 0; xi < Size.x; xi++)
                {
                    for (var zi = 0; zi < Size.z; zi++)
                    {
                        int blockType = chunkBlocks[xi, yi, zi];
                        if (blockType != 0)
                        {
                            var blockIndex = new RVector3(xi, yi, zi);

                            var x = (float)xi * BlockSize;
                            var y = (float)yi * BlockSize;
                            var z = (float)zi * BlockSize;

                            if (CheckSides(blockIndex, BlockFace.Top))
                            {
                                verticiesIndex = verticies.Count;
                                verticies.Add(new Vector3(x, y + BlockSize, z));
                                verticies.Add(new Vector3(x, y + BlockSize, z + BlockSize));
                                verticies.Add(new Vector3(x + BlockSize, y + BlockSize, z + BlockSize));
                                verticies.Add(new Vector3(x + BlockSize, y + BlockSize, z));
                                UpdateChunkUV(blockType);
                            }

                            if (CheckSides(blockIndex, BlockFace.Bottom))
                            {
                                verticiesIndex = verticies.Count;
                                verticies.Add(new Vector3(x, y, z));
                                verticies.Add(new Vector3(x + BlockSize, y, z));
                                verticies.Add(new Vector3(x + BlockSize, y, z + BlockSize));
                                verticies.Add(new Vector3(x, y, z + BlockSize));
                                UpdateChunkUV(blockType);
                            }

                            if (CheckSides(blockIndex, BlockFace.Right))
                            {
                                verticiesIndex = verticies.Count;
                                verticies.Add(new Vector3(x + BlockSize, y, z));
                                verticies.Add(new Vector3(x + BlockSize, y + BlockSize, z));
                                verticies.Add(new Vector3(x + BlockSize, y + BlockSize, z + BlockSize));
                                verticies.Add(new Vector3(x + BlockSize, y, z + BlockSize));
                                UpdateChunkUV(blockType);
                            }

                            if (CheckSides(blockIndex, BlockFace.Left))
                            {
                                verticiesIndex = verticies.Count;
                                verticies.Add(new Vector3(x, y, z + BlockSize));
                                verticies.Add(new Vector3(x, y + BlockSize, z + BlockSize));
                                verticies.Add(new Vector3(x, y + BlockSize, z));
                                verticies.Add(new Vector3(x, y, z));
                                UpdateChunkUV(blockType);
                            }

                            if (CheckSides(blockIndex, BlockFace.Far))
                            {
                                verticiesIndex = verticies.Count;
                                verticies.Add(new Vector3(x, y, z + BlockSize));
                                verticies.Add(new Vector3(x + BlockSize, y, z + BlockSize));
                                verticies.Add(new Vector3(x + BlockSize, y + BlockSize, z + BlockSize));
                                verticies.Add(new Vector3(x, y + BlockSize, z + BlockSize));
                                UpdateChunkUV(blockType);
                            }

                            if (CheckSides(blockIndex, BlockFace.Near))
                            {
                                verticiesIndex = verticies.Count;
                                verticies.Add(new Vector3(x, y, z));
                                verticies.Add(new Vector3(x, y + BlockSize, z));
                                verticies.Add(new Vector3(x + BlockSize, y + BlockSize, z));
                                verticies.Add(new Vector3(x + BlockSize, y, z));
                                UpdateChunkUV(blockType);
                            }
                        }
                    }
                }
            }
            FinalizeChunk();
        }

        public bool CheckSides(RVector3 blockPosition, BlockFace blockFace)
        {
            int x, y, z;
            x = blockPosition.x;
            y = blockPosition.y;
            z = blockPosition.z;

            var chunkBlocks = ChunkData.Blocks;

            switch (blockFace)
            {
                case BlockFace.Top: //Checks top face

                    if (y + 1 < Size.y)
                    {
                        if (chunkBlocks[x, y + 1, z] != 0)
                        {
                            return false;
                        }
                    }
                    break;

                case BlockFace.Bottom: //Checks bottom face

                    if (y - 1 >= 0 && chunkBlocks[x, y - 1, z] != 0)
                    {
                        return false;
                    }
                    break;

                case BlockFace.Right: //Checks right face

                    if (x + 1 < Size.x)
                    {
                        if (chunkBlocks[x + 1, y, z] != 0)
                        {
                            return false;
                        }
                    }
                    break;

                case BlockFace.Left: //Checks Left face

                    if (x - 1 >= 0)
                    {
                        if (chunkBlocks[x - 1, y, z] != 0)
                        {
                            return false;
                        }
                    }
                    break;

                case BlockFace.Far: //Checks Far face

                    if (z + 1 < Size.z)
                    {
                        if (chunkBlocks[x, y, z + 1] != 0)
                        {
                            return false;
                        }
                    }
                    break;

                case BlockFace.Near: //Checks Near face

                    if (z - 1 >= 0)
                    {
                        if (chunkBlocks[x, y, z - 1] != 0)
                        {
                            return false;
                        }
                    }
                    break;
            }
            return true;
        }

        void UpdateChunkUV(int blockID)
        {
            //because 0 is empty
            blockID = blockID - 1;

            triangles.Add(verticiesIndex);
            triangles.Add(verticiesIndex + 1);
            triangles.Add(verticiesIndex + 2);

            triangles.Add(verticiesIndex + 2);
            triangles.Add(verticiesIndex + 3);
            triangles.Add(verticiesIndex);


            var textureID = new Vector2(
                textureInterval.x * (blockID % atlasSize.x),
                -textureInterval.y * Mathf.FloorToInt(blockID / atlasSize.y));

            uvs.Add(new Vector2(textureID.x, textureID.y - textureInterval.y));
            uvs.Add(new Vector2(textureID.x + textureInterval.x, textureID.y - textureInterval.y));
            uvs.Add(new Vector2(textureID.x + textureInterval.x, textureID.y));
            uvs.Add(new Vector2(textureID.x, textureID.y));
        }

        void FinalizeChunk()
        {
            chunkMesh.vertices = verticies.ToArray();
            chunkMesh.triangles = triangles.ToArray();
            chunkMesh.uv = uvs.ToArray();
            chunkMesh.RecalculateNormals();

            GetComponent<MeshCollider>().sharedMesh = chunkMesh;
        }

        public void SetBlock(RVector3 buildPosCube, ushort blocktype)
        {
            if (buildPosCube.x < 0 || buildPosCube.y < 0 || buildPosCube.z < 0 || buildPosCube.x >= Size.x ||
                buildPosCube.y >= Size.y || buildPosCube.z >= Size.z)
            {
                return;
            }

            Send(new UpdateBlockMessage
            {
                ChunkPosition = Position,
                BlockPosition = buildPosCube,
                BlockType = blocktype
            },
            true);
        }

        protected override void RegisterMessageHandlers()
        {
            RegisterMessageHandler<UpdateBlockMessage>(OnUpdateBlock);
        }

        private void OnUpdateBlock(object payload)
        {
            var updateBlockMessage = (UpdateBlockMessage) payload;

            if (updateBlockMessage.ChunkPosition.x != Position.x ||
                updateBlockMessage.ChunkPosition.y != Position.y ||
                updateBlockMessage.ChunkPosition.z != Position.z)
                return;

            var buildPosCube = updateBlockMessage.BlockPosition;
            ChunkData.Blocks[buildPosCube.x, buildPosCube.y, buildPosCube.z] = updateBlockMessage.BlockType;
            UpdateChunk();
        }
    }
}