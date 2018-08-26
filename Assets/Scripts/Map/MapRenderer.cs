using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRenderer : MonoBehaviour {

    public static float scale = 0.5f;

    public MapData mapData = new MapData();

    public Vector3Int playerPosition;

    // Use this for initialization
    void Start () {
        Render();
    }

    // Update is called once per frame
    void Update () {
    }

    public void Render () {
        mapData.Render(playerPosition);
    }
}
