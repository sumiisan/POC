using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapChunk {

    public static int horizontalSize = 32;
    public static int verticalSize = 256;

    public Vector2Int location;
    List<PlacedMapEntity> data;

    public MapChunk (Vector2Int location) {
        this.location = location;
    }

    public void Generate () {
        data = new List<PlacedMapEntity>();
        GeneratePlane(new Vector2Int(0, 0), GlobalLocation());
    }

    Vector3Int GlobalLocation (int offsetX = 0, int offsetY = 0, int offsetZ = 0) {
        return new Vector3Int(location.x * horizontalSize + offsetX, offsetY, location.y * horizontalSize + offsetZ);
    }

    public RectInt area {
        get {
            return new RectInt(location.x * horizontalSize, location.y * horizontalSize, horizontalSize, horizontalSize);
        }
    }

    public bool Intersects (RectInt visibleRect) {
        return visibleRect.Intersects(area);
    }

    void GeneratePlane (Vector2Int localPos, Vector3Int globalPos, int level = 0, PlacedMapEntity basePME = null) {
        int LODScale = (int)( horizontalSize / Mathf.Pow(2, level) ); //32/1=32, 32/2=16, 32/4=8, 32/8=4, 32/16=2

        PlacedMapEntity pme = MapEntityFactory.shared.Generate(MapEntityType.ground, new Vector3(localPos.x, 0, localPos.y), LODScale);

        pme.Construct(globalPos, "");
        if (basePME == null) {
            data.Add(pme);
        } else {
            basePME.children[localPos.y * 2 + localPos.x] = pme;
        }

        if (level < 5) {
            for (int z = 0; z < 2; ++z) {
                for (int x = 0; x < 2; ++x) {
                    int childLODScale = (int)LODScale / 2;
                    Vector3Int childGlobalPos = globalPos + new Vector3Int(x * childLODScale, 0, z * childLODScale);
                    GeneratePlane(new Vector2Int(x, z), childGlobalPos, level + 1, pme);  //  recursive
                }
            }

        }
    }

    public void Render (Vector3Int center) {
        foreach (PlacedMapEntity pme in data) {
            pme.Render(GlobalLocation(), center);
        }
    }

    public void Release () {
        foreach (PlacedMapEntity pme in data) {
            pme.Release();
        }
        data.Clear();
    }
}
