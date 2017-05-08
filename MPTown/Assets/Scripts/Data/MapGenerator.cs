using Assets.Scripts.Helpers;
using Assets.Scripts.McChunk;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public class MapGenerator
    {
        public MapData GenerateMap(Vector3 MapSize, Vector3 ChunckSize, int seed, float intensity)
        {
            float[,] chunkHeights = Noise.Generate(
                (int)((MapSize.x + 1) * ChunckSize.x), 
                (int)((MapSize.y + 1) * ChunckSize.y), 
                seed, 
                intensity);

            var map = new MapData
            {
                Chunks = new ChunkData[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z]
            };

            this.ForXyz(
                map.Chunks.GetLength(0),
                map.Chunks.GetLength(1),
                map.Chunks.GetLength(2),
                (x, y, z) =>
                {
                    map.Chunks[x, y, z] = new ChunkData
                    {
                        Blocks = new ushort[(int)ChunckSize.x, (int)ChunckSize.y, (int)ChunckSize.z]
                    };

                    var chunk = map.Chunks[x, y, z];
                    var blocks = chunk.Blocks;

                    this.ForXyz(
                        blocks.GetLength(0),
                        blocks.GetLength(1),
                        blocks.GetLength(2),
                        (cx, cy, cz) =>
                        {
                            blocks[cx, cy, cz] =
                                cy > chunkHeights[
                                    cx + x * (int) ChunckSize.x,
                                    cz + z * (int) ChunckSize.z]
                                    ? (ushort)0
                                    : (ushort)Random.Range(1, 4);
                        });
                });

            return map;
        }
    }
}