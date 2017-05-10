using UnityEngine;

namespace Assets.Scripts.Helpers
{
    public struct Noise
    {
        public static float[,] Generate(int xSize, int ySize, int seed, float intensity)
        {
            float[,] noise = new float[xSize, ySize];

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    float xNoise = (float)x / xSize;
                    float yNoise = (float)y / ySize;

                    noise[x, y] = Mathf.PerlinNoise(seed + xNoise, seed + yNoise) * intensity;
                }
            }
            return noise;
        }
    }
}