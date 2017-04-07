using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MapController : NetworkBehaviour
{

    //   [SyncVar]
    //   public Color color = Color.magenta;

    //   // Use this for initialization
    //   void Start ()
    //   {
    //       SyncColor();
    //   }

    //   public void Awake()
    //   {
    //       InitState();
    //   }

    //   [Server]
    //   private void InitState()
    //   {
    //       Color[] colors = { Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.yellow };
    //       color = colors[Random.Range(0, colors.Length)];
    //   }

    //   private void SyncColor()
    //   {
    //       GetComponentInChildren<Renderer>().material.color = color; // (isLocalPlayer ? Color.white : Color.grey) * color;
    //   }

    //   // Update is called once per frame
    //   void Update ()
    //   {
    //       InitState();
    //       SyncColor();
    //}
}
