using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapChunk {

    public static int horizontalSize = 32;
    public static int verticalSize = 256;

    public Vector2Int location;
    public MapEntity[,,] expandedData;
    List<PlacedMapEntity> data;


    public MapChunk (Vector2Int location) {
        this.location = location;
    }

    public void Generate () {
        data = new List<PlacedMapEntity>();
        GeneratePlane();
    }

    Vector3Int GlobalLocation (int offsetX = 0, int offsetY = 0, int offsetZ = 0) {
        return new Vector3Int(location.x * horizontalSize + offsetX, offsetY, location.y * horizontalSize + offsetZ);
    }

    void GeneratePlane () {
        for (int iz = 0; iz < horizontalSize; ++iz) {
            for (int ix = 0; ix < horizontalSize; ++ix) {
                PlacedMapEntity pme = MapEntityFactory.shared.Generate(MapEntityType.ground, new Vector3Int(ix, 0, iz));
                pme.Construct(GlobalLocation(ix, 0, iz), "");
                data.Add(pme);
            }
        }
    }

    public void Render () {
        foreach (PlacedMapEntity pme in data) {
            pme.Render(GlobalLocation());
        }
    }
}
