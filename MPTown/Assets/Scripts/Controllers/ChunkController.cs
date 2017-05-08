using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.McChunk;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class ChunkController : MonoBehaviour
    {
        public ChunkData ChunkData;
        public Vector2 TextureBlockSize;

        public Mesh chunkMesh;
        public RVector3 Position;
        public RVector3 Size;

        private List<int> triangles = new List<int>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<Vector3> verticies = new List<Vector3>();

        private int verticiesIndex;
        private Texture textureAtlas;
        private Vector3 atlasSize;
        
        public void Start()
        {
            textureAtlas = transform.GetComponent<MeshRenderer>().material.mainTexture;
            atlasSize = new Vector2(textureAtlas.width / TextureBlockSize.x, textureAtlas.height / TextureBlockSize.y);
            chunkMesh = GetComponent<MeshFilter>().mesh;
            UpdateChunk();
        }

        public void UpdateChunk()
        {
            verticies = new List<Vector3>();
            uvs = new List<Vector2>();
            triangles = new List<int>();

            chunkMesh.Clear();

            float blockSize = 1;

            var chunkBlocks = ChunkData.Blocks;

            for (var y = 0; y < Size.y; y++)
            {
                for (var x = 0; x < Size.x; x++)
                {
                    for (var z = 0; z < Size.z; z++)
                    {
                        if (chunkBlocks[x, y, z] != 0)
                        {
                            if (CheckSides(new RVector3(x, y, z), BlockFace.Top))
                            {
                                verticiesIndex = verticies.Count;

                                verticies.Add(new Vector3(x, y + blockSize, z));
                                verticies.Add(new Vector3(x, y + blockSize, z + blockSize));
                                verticies.Add(new Vector3(x + blockSize, y + blockSize, z + blockSize));
                                verticies.Add(new Vector3(x + blockSize, y + blockSize, z));
                                UpdateChunkUV(chunkBlocks[x, y, z]);
                            }

                            if (CheckSides(new RVector3(x, y, z), BlockFace.Bottom))
                            {
                                verticiesIndex = verticies.Count;

                                verticies.Add(new Vector3(x, y, z));
                                verticies.Add(new Vector3(x + blockSize, y, z));
                                verticies.Add(new Vector3(x + blockSize, y, z + blockSize));
                                verticies.Add(new Vector3(x, y, z + blockSize));
                                UpdateChunkUV(chunkBlocks[x, y, z]);
                            }

                            if (CheckSides(new RVector3(x, y, z), BlockFace.Right))
                            {
                                verticiesIndex = verticies.Count;


                                verticies.Add(new Vector3(x + blockSize, y, z));
                                verticies.Add(new Vector3(x + blockSize, y + blockSize, z));
                                verticies.Add(new Vector3(x + blockSize, y + blockSize, z + blockSize));
                                verticies.Add(new Vector3(x + blockSize, y, z + blockSize));
                                UpdateChunkUV(chunkBlocks[x, y, z]);
                            }

                            if (CheckSides(new RVector3(x, y, z), BlockFace.Left))
                            {
                                verticiesIndex = verticies.Count;

                                verticies.Add(new Vector3(x, y, z + blockSize));
                                verticies.Add(new Vector3(x, y + blockSize, z + blockSize));
                                verticies.Add(new Vector3(x, y + blockSize, z));
                                verticies.Add(new Vector3(x, y, z));
                                UpdateChunkUV(chunkBlocks[x, y, z]);
                            }

                            if (CheckSides(new RVector3(x, y, z), BlockFace.Far))
                            {
                                verticiesIndex = verticies.Count;

                                verticies.Add(new Vector3(x, y, z + blockSize));
                                verticies.Add(new Vector3(x + blockSize, y, z + blockSize));
                                verticies.Add(new Vector3(x + blockSize, y + blockSize, z + blockSize));
                                verticies.Add(new Vector3(x, y + blockSize, z + blockSize));
                                UpdateChunkUV(chunkBlocks[x, y, z]);
                            }

                            if (CheckSides(new RVector3(x, y, z), BlockFace.Near))
                            {
                                verticiesIndex = verticies.Count;

                                verticies.Add(new Vector3(x, y, z));
                                verticies.Add(new Vector3(x, y + blockSize, z));
                                verticies.Add(new Vector3(x + blockSize, y + blockSize, z));
                                verticies.Add(new Vector3(x + blockSize, y, z));
                                UpdateChunkUV(chunkBlocks[x, y, z]);
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

            var textureInterval = new Vector2(1 / atlasSize.x, 1 / atlasSize.y);

            var textureID = new Vector2(textureInterval.x * (blockID % atlasSize.x),
                textureInterval.y * Mathf.FloorToInt(blockID / atlasSize.y));

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
        }
    }
}