using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public class MapGenerator
    {
        public MapData GenerateMap(Vector3 MapSize, Vector3 ChunckSize, int seed, float intensity)
        {
            var map = new MapData
            {
                Chunks = new ChunkData[(int) MapSize.x, (int) MapSize.y, (int) MapSize.z],
                ChunkHeights = Noise.Generate(
                    (int) ((MapSize.x + 1) * ChunckSize.x),
                    (int) ((MapSize.z + 1) * ChunckSize.z),
                    seed,
                    intensity)
            };
            
            this.ForXyz(
                map.Chunks.GetLength(0),
                map.Chunks.GetLength(1),
                map.Chunks.GetLength(2),
                (x, y, z) =>
                {
                    map.Chunks[x, y, z] = new ChunkData
                    {
                        Blocks = new BlockData[(int) ChunckSize.x, (int) ChunckSize.y, (int) ChunckSize.z],
                        MapPosition = new RVector3(x, y, z)
                    };

                    var chunk = map.Chunks[x, y, z];
                    var blocks = chunk.Blocks;

                    this.ForXyz(
                        blocks.GetLength(0),
                        blocks.GetLength(1),
                        blocks.GetLength(2),
                        (cx, cy, cz) =>
                        {
                            var h = map.ChunkHeights[cx + x * (int) ChunckSize.x, cz + z * (int) ChunckSize.z];
                            blocks[cx, cy, cz] = new BlockData
                            {
                                BlockType = GetBlock(h, cx, cy, cz)
                            };
                        });
                });

            return map;
        }

        private ushort GetBlock(float h, int cx, int cy, int cz)
        {
            if (cy > h)
                return 0;

            if (cy <= 2)
            {
                return 1; //stone
            }

            if (cy <= 4)
            {
                return (ushort)Random.Range(5, 7); // dirt
            }

            if (cy <= 5)
            {
                return (ushort)Random.Range(7, 9); // sand
            }

            return (ushort)Random.Range(3,5); // grass
        }
    }
}