using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEntityFactory : MonoBehaviour {
    public static MapEntityFactory shared;

    public GameObject groundPrefab;
    public GameObject oceanPrefab;

    private void Awake () {
        shared = this;
    }

    public PlacedMapEntity Generate (MapEntityType type, Vector3 position, int LOD) {
        switch (type) {
        case MapEntityType.ground:
            return new PMEGround(position, LOD);

        }
        return null;
    }


    public void SetColor (GameObject o, Color32 c, float b) {
        Mesh mesh = o.GetComponent<MeshFilter>().mesh;

        c = new Color32((byte)( c.r * b ), (byte)( c.g * b ), (byte)( c.b * b ), 0xff);

        Color32[] newColors = new Color32[mesh.vertices.Length];
        for (int vertexIndex = 0; vertexIndex < newColors.Length; vertexIndex++) {
            newColors[vertexIndex] = c;
        }

        mesh.colors32 = newColors;
    }


}