using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Navigation
{
    public class NavWeb : MonoBehaviour
    {
        public int ConnexionDistance = 1;
        public RVector3Table NavPoints = new RVector3Table();

        public void InitializeFromMapData(MapData mapData)
        {
            //Set all air block without tree as navigable

            var chunks = mapData.Chunks;
            this.ForXyz(chunks.GetLength(0),
                chunks.GetLength(1),
                chunks.GetLength(2),
                (cx, cy, cz) =>
                {
                    var chunk = chunks[cx, cy, cz];
                    var blocks = chunk.Blocks;

                    var chunkOffset = new RVector3(
                        cx * (blocks.GetLength(0) - 1),
                        cy * (blocks.GetLength(1) - 1),
                        cz * (blocks.GetLength(2) - 1));

                    this.ForXyz(
                        blocks.GetLength(0),
                        blocks.GetLength(1),
                        blocks.GetLength(2),
                        (bx, by, bz) =>
                        {
                            RecalculateBlockNavigation(bx, by, bz, blocks, chunkOffset);
                        });
                });
        }

        public void RecalculateBlockNavigation(int bx, int by, int bz, BlockData[,,] blocks, RVector3 chunkOffset)
        {
            var block = blocks[bx, by, bz];
            if (block.BlockType == 0 &&
                block.ObjectDataData == null)
            {
                var position = new RVector3(bx + chunkOffset.x, by + chunkOffset.y, bz + chunkOffset.z);

                var blockOnBottomIsAir = by == 0 ||
                                         blocks[bx, by - 1, bz].BlockType == 0 &&
                                         blocks[bx, by - 1, bz].ObjectDataData == null;
                var blockOnTopIsAir = by == (blocks.GetLength(2) - 1) ||
                                      blocks[bx, by + 1, bz].BlockType == 0;

                if (!blockOnBottomIsAir && blockOnTopIsAir)
                {
                    SetNavigablePoint(position);
                }
            }
        }

        public void SetNavigablePoint(RVector3 position)
        {
            if (NavPoints.Contains(position))
            {
                return;
            }

            NavPoints.Add(position);
        }

        public void UnsetNavigablePoint(RVector3 position)
        {
            if (NavPoints.Contains(position))
            {
                NavPoints.Remove(position);
            }
        }

        public bool IsNavigable(RVector3 position)
        {
            return NavPoints.Contains(position);
        }

        public List<RVector3> GetConnexion(RVector3 p)
        {
            var con = new List<RVector3>();

            this.ForXyz(p.x - 1, p.x + 2, p.y - 1, p.y + 2, p.z - 1, p.z + 2, (x, y, z) =>
            {
                var cursor = new RVector3(x, y, z);
                if (cursor != p && NavPoints.Contains(cursor))
                {
                    con.Add(cursor);
                }
            });

            return con;
        }
        
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            foreach (RVector3 v in NavPoints.GetAll())
            {
                Gizmos.DrawCube(new Vector3(v.x + .5f, v.y, v.z + .5f), Vector3.one * .1f);
            }
        }
    }

    public class RVector3Table
    {
        private readonly Dictionary<int, Dictionary<int, Dictionary<int, RVector3>>> store;

        public RVector3Table()
        {
            store = new Dictionary<int, Dictionary<int, Dictionary<int, RVector3>>>();
        }

        public bool Contains(RVector3 item)
        {
            return store.ContainsKey(item.x) && 
                   store[item.x].ContainsKey(item.y) &&
                   store[item.x][item.y].ContainsKey(item.z) &&
                   store[item.x][item.y][item.z] != null;
        }

        public void Add(RVector3 item)
        {
            if (!store.ContainsKey(item.x))
            {
                store.Add(item.x, new Dictionary<int, Dictionary<int, RVector3>>());
            }

            if (!store[item.x].ContainsKey(item.y))
            {
                store[item.x].Add(item.y, new Dictionary<int, RVector3>());
            }

            if (!store[item.x][item.y].ContainsKey(item.z))
            {
                store[item.x][item.y].Add(item.z, item);
            }
        }

        public void Remove(RVector3 item)
        {
            if (!store.ContainsKey(item.x))
            {
                return;
            }

            if (!store[item.x].ContainsKey(item.y))
            {
                return;
            }
            
            if (!store[item.x][item.y].ContainsKey(item.z))
            {
                return;
            }

            store[item.x][item.y].Remove(item.z);
        }

        public IEnumerable GetAll()
        {
            foreach (var x in store.Values)
            {
                foreach (var y in x.Values)
                {
                    foreach (var rVector3 in y.Values)
                    {
                        yield return rVector3;
                    }
                }
            }
        }
    }
}