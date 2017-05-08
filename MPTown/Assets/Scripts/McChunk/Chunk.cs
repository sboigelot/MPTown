using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.McChunk
{
    [Serializable]
    public class Chunk : MonoBehaviour
    {
        public Block[,,] chunkBlocks;
        public Mesh chunkMesh;

        public RVector3 chunkPosition;

        public RVector3 chunkSize;
        List<int> chunkTriangles = new List<int>();
        List<Vector2> chunkUV = new List<Vector2>();

        List<Vector3> chunkVerticies = new List<Vector3>();

        public int seed;
        public int intensity;
        int VerticiesIndex;

        public Vector2 textureBlockSize;
        Texture textureAtlas;
        Vector2 atlasSize;

        public RVector3 Position
        {
            get { return chunkPosition; }
            set { chunkPosition = value; }
        }

        public RVector3 Size
        {
            get { return chunkSize; }
            set { chunkSize = value; }
        }

        public Block[,,] ReturnChunkBlocks
        {
            get { return chunkBlocks; }
        }

        public Chunk ThisChunk
        {
            get { return this; }
        }

        void Awake()
        {
            textureAtlas = transform.GetComponent<MeshRenderer>().material.mainTexture;
            atlasSize = new Vector2(textureAtlas.width / textureBlockSize.x, textureAtlas.height / textureBlockSize.y);

            chunkMesh = GetComponent<MeshFilter>().mesh;
            GenerateChunk();
        }


        public void GenerateChunk()
        {
            float[,] chunkHeights = Noise.Generate(chunkSize.x + 1, chunkSize.y + 1, seed, intensity);

            chunkBlocks = new Block[chunkSize.x + 1, chunkSize.y + 1, chunkSize.z + 1];

            for (var x = 0; x <= chunkSize.x; x++)
            {
                for (var z = 0; z <= chunkSize.z; z++)
                {
                    for (var y = 0; y <= chunkSize.y; y++)
                    {
                        chunkBlocks[x, y, z] = new Block(true);


                        if (y <= chunkHeights[x, z])
                        {
                            chunkBlocks[x, y, z] = new Block(false);
                            chunkBlocks[x, y, z].id = (byte)Random.Range(0, 3);
                        }
                    }
                }
            }
            UpdateChunk();
        }


        public void UpdateChunk()
        {
            chunkVerticies = new List<Vector3>();
            chunkUV = new List<Vector2>();
            chunkTriangles = new List<int>();

            chunkMesh.Clear();

            float blockSize = 1;

            for (var y = 0; y <= chunkSize.y; y++)
            {
                for (var x = 0; x <= chunkSize.x; x++)
                {
                    for (var z = 0; z <= chunkSize.z; z++)
                    {
                        if (!chunkBlocks[x, y, z].empty)
                        {
                            if (CheckSides(new RVector3(x, y, z), BlockFace.Top))
                            {
                                VerticiesIndex = chunkVerticies.Count;

                                chunkVerticies.Add(new Vector3(x, y + blockSize, z));
                                chunkVerticies.Add(new Vector3(x, y + blockSize, z + blockSize));
                                chunkVerticies.Add(new Vector3(x + blockSize, y + blockSize, z + blockSize));
                                chunkVerticies.Add(new Vector3(x + blockSize, y + blockSize, z));
                                UpdateChunkUV(chunkBlocks[x, y, z].id);
                            }

                            if (CheckSides(new RVector3(x, y, z), BlockFace.Bottom))
                            {
                                VerticiesIndex = chunkVerticies.Count;

                                chunkVerticies.Add(new Vector3(x, y, z));
                                chunkVerticies.Add(new Vector3(x + blockSize, y, z));
                                chunkVerticies.Add(new Vector3(x + blockSize, y, z + blockSize));
                                chunkVerticies.Add(new Vector3(x, y, z + blockSize));
                                UpdateChunkUV(chunkBlocks[x, y, z].id);
                            }

                            if (CheckSides(new RVector3(x, y, z), BlockFace.Right))
                            {
                                VerticiesIndex = chunkVerticies.Count;


                                chunkVerticies.Add(new Vector3(x + blockSize, y, z));
                                chunkVerticies.Add(new Vector3(x + blockSize, y + blockSize, z));
                                chunkVerticies.Add(new Vector3(x + blockSize, y + blockSize, z + blockSize));
                                chunkVerticies.Add(new Vector3(x + blockSize, y, z + blockSize));
                                UpdateChunkUV(chunkBlocks[x, y, z].id);
                            }

                            if (CheckSides(new RVector3(x, y, z), BlockFace.Left))
                            {
                                VerticiesIndex = chunkVerticies.Count;

                                chunkVerticies.Add(new Vector3(x, y, z + blockSize));
                                chunkVerticies.Add(new Vector3(x, y + blockSize, z + blockSize));
                                chunkVerticies.Add(new Vector3(x, y + blockSize, z));
                                chunkVerticies.Add(new Vector3(x, y, z));
                                UpdateChunkUV(chunkBlocks[x, y, z].id);
                            }

                            if (CheckSides(new RVector3(x, y, z), BlockFace.Far))
                            {
                                VerticiesIndex = chunkVerticies.Count;

                                chunkVerticies.Add(new Vector3(x, y, z + blockSize));
                                chunkVerticies.Add(new Vector3(x + blockSize, y, z + blockSize));
                                chunkVerticies.Add(new Vector3(x + blockSize, y + blockSize, z + blockSize));
                                chunkVerticies.Add(new Vector3(x, y + blockSize, z + blockSize));
                                UpdateChunkUV(chunkBlocks[x, y, z].id);
                            }

                            if (CheckSides(new RVector3(x, y, z), BlockFace.Near))
                            {
                                VerticiesIndex = chunkVerticies.Count;

                                chunkVerticies.Add(new Vector3(x, y, z));
                                chunkVerticies.Add(new Vector3(x, y + blockSize, z));
                                chunkVerticies.Add(new Vector3(x + blockSize, y + blockSize, z));
                                chunkVerticies.Add(new Vector3(x + blockSize, y, z));
                                UpdateChunkUV(chunkBlocks[x, y, z].id);
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

            switch (blockFace)
            {
                case BlockFace.Top: //Checks top face

                    if (y + 1 <= chunkSize.y)
                    {
                        if (!chunkBlocks[x, y + 1, z].empty)
                        {
                            return false;
                        }
                    }
                    break;

                case BlockFace.Bottom: //Checks bottom face

                    if (y - 1 >= 0 && !chunkBlocks[x, y - 1, z].empty)
                    {
                        return false;
                    }
                    break;

                case BlockFace.Right: //Checks right face

                    if (x + 1 <= chunkSize.x)
                    {
                        if (!chunkBlocks[x + 1, y, z].empty)
                        {
                            return false;
                        }
                    }
                    break;

                case BlockFace.Left: //Checks Left face

                    if (x - 1 >= 0)
                    {
                        if (!chunkBlocks[x - 1, y, z].empty)
                        {
                            return false;
                        }
                    }
                    break;

                case BlockFace.Far: //Checks Far face

                    if (z + 1 <= chunkSize.z)
                    {
                        if (!chunkBlocks[x, y, z + 1].empty)
                        {
                            return false;
                        }
                    }
                    break;

                case BlockFace.Near: //Checks Near face

                    if (z - 1 >= 0)
                    {
                        if (!chunkBlocks[x, y, z - 1].empty)
                        {
                            return false;
                        }
                    }
                    break;
            }
            return true;
        }

        void UpdateChunkUV(byte blockID)
        {
            chunkTriangles.Add(VerticiesIndex);
            chunkTriangles.Add(VerticiesIndex + 1);
            chunkTriangles.Add(VerticiesIndex + 2);

            chunkTriangles.Add(VerticiesIndex + 2);
            chunkTriangles.Add(VerticiesIndex + 3);
            chunkTriangles.Add(VerticiesIndex);
            Vector2 textureInterval = new Vector2(1 / atlasSize.x, 1 / atlasSize.y);


            Vector2 textureID = new Vector2(textureInterval.x * (blockID % atlasSize.x), textureInterval.y * Mathf.FloorToInt(blockID / atlasSize.y));


            chunkUV.Add(new Vector2(textureID.x, textureID.y - textureInterval.y));
            chunkUV.Add(new Vector2(textureID.x + textureInterval.x, textureID.y - textureInterval.y));
            chunkUV.Add(new Vector2(textureID.x + textureInterval.x, textureID.y));
            chunkUV.Add(new Vector2(textureID.x, textureID.y));
        }

        void FinalizeChunk()
        {
            chunkMesh.vertices = chunkVerticies.ToArray();
            chunkMesh.triangles = chunkTriangles.ToArray();
            chunkMesh.uv = chunkUV.ToArray();
            chunkMesh.RecalculateNormals();
        }
    }
}