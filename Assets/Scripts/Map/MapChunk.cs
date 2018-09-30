using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapChunk {

    public const int horizontalSize = 32;
    public const int verticalSize = 256;

    public Vector2Int location;
    PlacedMapEntity lodTree;
    public PlacedMapEntity[,] data = new PlacedMapEntity[horizontalSize, horizontalSize];

    public MapChunk (Vector2Int location) {
        this.location = location;
    }

    public void Generate () {
        GenerateMap(GlobalPosition());
    }

    Vector3Int GlobalPosition (int offsetX = 0, int offsetY = 0, int offsetZ = 0) {
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

    void GenerateMap (Vector3Int globalPos) {
        //  first create the raw (smallest size) ground data
        for (int z = 0; z < horizontalSize; ++z) {
            for (int x = 0; x < horizontalSize; ++x) {
                PlacedMapEntity pme = MapEntityFactory.shared.Generate(MapEntityType.ground, new Vector3(x, 0, z), 5);
                pme.Construct(new Vector3Int(globalPos.x + x, 0, globalPos.z + z), "");
                data[x, z] = pme;
            }
        }

        //  make lod tree
        lodTree = GenerateLODNode(new Vector2Int(0, 0), globalPos);
    }

    PlacedMapEntity GenerateLODNode (Vector2Int localPos, Vector3Int globalPos, int LOD = 0) {
        PlacedMapEntity pmeNode = MapEntityFactory.shared.Generate(MapEntityType.ground, new Vector3(localPos.x, 0, localPos.y), LOD);

        //  first dive down until bottom level recursively to grab the detail data
        if (LOD < 5) {
            for (int z = 0; z < 2; ++z) {
                for (int x = 0; x < 2; ++x) {
                    int childLODScale = (int)pmeNode.LODScale / 2;
                    Vector3Int childGlobalPos = globalPos + new Vector3Int(x * childLODScale, 0, z * childLODScale);
                    PlacedMapEntity childNode = GenerateLODNode(new Vector2Int(x, z), childGlobalPos, LOD + 1);  //  recursive
                    pmeNode.AddChild(x, z, childNode);
                }
            }
            pmeNode.AverageChildren();
            return pmeNode;
        } else {
            //  we are on level 5 (detail)
            //            Debug.Log("x:" + globalPos.x + "  y:" + globalPos.z);
            Vector3Int p = GlobalPosition();
            return data[globalPos.x - p.x, globalPos.z - p.z];
        }
    }


    public void Render (Vector3Int center) {
        lodTree.Render(GlobalPosition(), center);
    }

    public void Release () {
        lodTree.Release();
        lodTree = null;
    }
}
