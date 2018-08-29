using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRenderer : MonoBehaviour {

    public static float scale = 1f;

    public MapData mapData = new MapData();
    public Transform cameraPosition;

    public Vector3Int playerPosition;

    private void Awake () {
        playerPosition = new Vector3Int(200, 30, 200);
    }
    // Use this for initialization
    void Start () {
        cameraPosition.transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
        Render();
    }

    // Update is called once per frame
    void Update () {
        playerPosition = new Vector3Int(Mathf.RoundToInt(cameraPosition.position.x), Mathf.RoundToInt(cameraPosition.position.y), Mathf.RoundToInt(cameraPosition.position.z));
        mapData.RenderIfNeeded(playerPosition);
    }

    public void Render () {
        mapData.Render(playerPosition);
    }
}
