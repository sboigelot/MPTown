using Assets.Scripts.Controllers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Assets.Scripts.Network;
using Assets.Scripts.Data;

public class MapController : MonoBehaviour
{
    //private NetworkBroadcastBus networkBus;

    private MapData mapData;
    public MapData MapData
    {
        get
        {
            return mapData;
        }

        set
        {
            if(mapData != value)
            {
                mapData = value;
                OnMapDataChanged(mapData);
            }
        }
    }

    private NetworkIdentity GetLocalPlayer()
    {
        var allNetworkPlayer = FindObjectsOfType<NetworkIdentity>();
        foreach (var p in allNetworkPlayer)
        {
            if (p.isLocalPlayer)
            {
                return p;
            }
        }
        return null;
    }

    private void OnMapDataChanged(MapData mapData)
    {        
        InvalidateVisual();
                      
        if (NetworkBroadcastBus.IsServer)
        {
            NetworkBroadcastBus.LocalBus.SendMessage<MapData>(mapData);
        }
    }

    private void InvalidateVisual()
    {
        transform.ClearChildren();

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    if (MapData.Chunks[0, 0].Blocks[x, y, z].BlockIndex != 0)
                    {
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.SetParent(transform);
                        cube.transform.position = new Vector3(x, y, z);
                        cube.transform.localScale = Vector3.one;
                    }
                }
            }
        }
    }

    public void Start()
    {
        NetworkBroadcastBus.OnLocalBusFound += OnLocalBusFound;
    }

    private void OnLocalBusFound(NetworkBroadcastBus bus)
    {
        bus.OnMessageReceived += OnMessageReceived;
    }

    public void OnDestroy()
    {
        //networkBus.OnMessageReceived -= OnMessageReceived;
    }

    public void Initialize()
    {
        if(NetworkBroadcastBus.IsServer)
        {
            InitMapData();
        }
    }

    private void InitMapData()
    {
        var map = new MapData();
        map.Chunks = new ChunkData[1, 1];
        map.Chunks[0, 0] = new ChunkData();
        map.Chunks[0, 0].Blocks = new BlockData[2, 2, 2];

        for (int x = 0; x < 2; x++)
        {
            for (int y=0; y<2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    map.Chunks[0, 0].Blocks[x, y, z] = new BlockData
                    {
                        BlockIndex = UnityEngine.Random.Range(0, 2),
                        Inventory = null,
                        MeshIndex = 0,
                        TextureIndex = 0
                    };
                }
           }
        }

        MapData = map;
    }
    
    private void OnMessageReceived(NetworkBusEnvelope envelope)
    {
        if (!NetworkBroadcastBus.IsServer && envelope.PayloadType == "MapData")
        {
            MapData = envelope.Open<MapData>();
        }
    }
}


public static class TransformExtensions
{
    public static void ClearChildren(this Transform t)
    {
        for (var i = 0; i < t.childCount; i++)
        {
            UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }
    }
}
                  