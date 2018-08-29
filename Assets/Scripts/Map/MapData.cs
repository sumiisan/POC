using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapData {

    Dictionary<int, MapChunk> chunks;

    public int viewDistance = 64;
    public RectInt currentBorder;

    public MapData () {
        chunks = new Dictionary<int, MapChunk>();
    }

    int ChunkIndex (Vector2Int chunkLocation) {
        return chunkLocation.y * 0x7fff + chunkLocation.x;
    }

    MapChunk Chunk (Vector2Int chunkLocation) {
        int key = ChunkIndex(chunkLocation);
        MapChunk c = chunks.Value(key);
        if (c != null) return c;
        c = new MapChunk(chunkLocation);
        Debug.Log("new map chunk:" + chunkLocation);
        c.Generate();
        chunks[key] = c;
        return c;
    }

    Vector2Int ChunkLocation (Vector3Int globalPosition) {
        return new Vector2Int(globalPosition.x / MapChunk.horizontalSize, globalPosition.z / MapChunk.horizontalSize);
    }

    public RectInt Render (Vector3Int center) {
        RectInt visibleRect = new RectInt(center.x - viewDistance, center.z - viewDistance, viewDistance * 2, viewDistance * 2);
        currentBorder = new RectInt(0, 0, 0, 0);
        for (int iz = visibleRect.yMin; iz <= visibleRect.yMax; iz += MapChunk.horizontalSize) {
            for (int ix = visibleRect.xMin; ix <= visibleRect.xMax; ix += MapChunk.horizontalSize) {
                RectInt renderedArea = Chunk(ChunkLocation(new Vector3Int(ix, 0, iz))).Render();

                if (currentBorder.size.x == 0) { currentBorder = renderedArea; }  //init

                //  extend rect TODO: make RectInt extension .
                if (currentBorder.xMin > renderedArea.xMin) currentBorder.xMin = renderedArea.xMin;
                if (currentBorder.xMax < renderedArea.xMax) currentBorder.xMax = renderedArea.xMax;
                if (currentBorder.yMin > renderedArea.yMin) currentBorder.yMin = renderedArea.yMin;
                if (currentBorder.yMax < renderedArea.yMax) currentBorder.yMax = renderedArea.yMax;
            }
        }
        return currentBorder;
    }

    void ReleaseFarChunks (RectInt visibleRect) {
        Dictionary<int, MapChunk>.KeyCollection keys = chunks.Keys;
        List<int> keysToRemove = new List<int>();

        //  add some margin
        visibleRect.x -= 5;
        visibleRect.y -= 5;
        visibleRect.width += 10;
        visibleRect.height += 10;
        foreach (int key in keys) {
            MapChunk ch = chunks[key];
            if (!ch.Intersects(visibleRect)) {
                ch.Release();
                keysToRemove.Add(key);
            }
        }
        foreach (int key in keysToRemove) {
            chunks.Remove(key);
        }
    }

    public bool RenderIfNeeded (Vector3Int center) {
        RectInt visibleRect = new RectInt(center.x - viewDistance, center.z - viewDistance, viewDistance * 2, viewDistance * 2);
        //Debug.Log("view:" + visibleRect + "  border:" + currentBorder);
        if (!currentBorder.Contains(visibleRect)) {
            Debug.Log("EXTEND!");
            Render(center);
            ReleaseFarChunks(visibleRect);
            return true;
        }
        return false;
    }
}
