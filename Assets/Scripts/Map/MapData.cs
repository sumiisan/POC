using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapData {

    Dictionary<int, MapChunk> chunks;

    public int viewDistance = 64;

    public MapData () {
        chunks = new Dictionary<int, MapChunk>();
    }

    int ChunkIndex (Vector2Int chunkLocation) {
        return chunkLocation.y * 0x7fff + chunkLocation.x;
    }

    MapChunk Chunk (Vector2Int chunkLocation) {
        MapChunk c = chunks.Value(ChunkIndex(chunkLocation));
        if (c != null) return c;
        c = new MapChunk(chunkLocation);
        c.Generate();
        return c;
    }

    Vector2Int ChunkLocation (Vector3Int globalPosition) {
        return new Vector2Int(globalPosition.x / MapChunk.horizontalSize, globalPosition.z / MapChunk.horizontalSize);
    }

    public void Render (Vector3Int center) {
        int zBorder = center.z + viewDistance;
        int xBorder = center.x + viewDistance;
        for (int iz = center.z - viewDistance; iz < zBorder; iz += MapChunk.horizontalSize) {
            for (int ix = center.x - viewDistance; ix < xBorder; ix += MapChunk.horizontalSize) {
                Chunk(ChunkLocation(new Vector3Int(ix, 0, iz))).Render();
            }
        }
    }

}
