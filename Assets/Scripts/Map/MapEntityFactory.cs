using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEntityFactory : MonoBehaviour {
    public static MapEntityFactory shared;

    public GameObject groundPrefab;

    private void Awake () {
        shared = this;
    }

    public PlacedMapEntity Generate (MapEntityType type, Vector3Int mapLocation) {
        switch (type) {
        case MapEntityType.ground:
            return new PMEGround(mapLocation);
        }
        return null;
    }

}