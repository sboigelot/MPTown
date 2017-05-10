using System;

namespace Assets.Scripts.Helpers
{
    public static class ObjectExtensions
    {
        public static void ForXyz(this object o, int xmax, int ymax, int zmax, Action<int, int, int> doAction)
        {
            for (int x = 0; x < xmax; x++)
            {
                for (int y = 0; y < ymax; y++)
                {
                    for (int z = 0; z < zmax; z++)
                    {
                        doAction(x, y, z);
                    }
                }
            }
        }
    }
}