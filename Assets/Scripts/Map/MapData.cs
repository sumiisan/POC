using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapData {

    Dictionary<int, MapChunk> chunks;

    public int viewDistance = 128;//64;
    public RectInt currentBorder;
    Vector3Int lastDrawnCenter = new Vector3Int(0, 0, 0);

    public MapData () {
        chunks = new Dictionary<int, MapChunk>();
    }

    /*
     * 
     *  chunk control
     * 
     */
    private int ChunkIndex (Vector2Int chunkLocation) {
        return chunkLocation.y * 0x7fff + chunkLocation.x;
    }

    private Vector2Int ChunkLocation (Vector3Int globalPosition) {
        return new Vector2Int(globalPosition.x / MapChunk.horizontalSize, globalPosition.z / MapChunk.horizontalSize);
    }

    private MapChunk Chunk (Vector2Int chunkLocation) {
        int key = ChunkIndex(chunkLocation);
        MapChunk c = chunks.Value(key);
        if (c != null) return c;
        c = new MapChunk(chunkLocation);
        Debug.Log("new map chunk:" + chunkLocation);
        c.Generate();
        chunks[key] = c;
        return c;
    }

    private void ReleaseFarChunks (RectInt holdRect) {
        Dictionary<int, MapChunk>.KeyCollection keys = chunks.Keys;
        List<int> keysToRemove = new List<int>();

        foreach (int key in keys) {
            MapChunk ch = chunks[key];
            if (!ch.Intersects(holdRect)) {
                ch.Release();
                keysToRemove.Add(key);
            }
        }
        foreach (int key in keysToRemove) {
            chunks.Remove(key);
        }
    }

    /*----------------------------------------------------
    * 
    *
    *   Public Map Access
    *
    * 
    *----------------------------------------------------*/

    public PlacedMapEntity MapEntityAt (Vector3Int globalPosition) {
        return Chunk(ChunkLocation(globalPosition)).data[globalPosition.x % MapChunk.horizontalSize, globalPosition.z % MapChunk.horizontalSize];
    }

    public void PutLight (Vector3Int globalPosition, float intensity) {
        float radius = intensity * 20f;
        Vector3 lp = GroundPosition(globalPosition, 2f);
        Collider[] cols = Physics.OverlapSphere(lp, radius);

        foreach (Collider col in cols) {
            GameObject o = col.gameObject;
            MapObject mo = o.GetComponent<MapObject>();
            if (mo != null) {   //its a map object!
                //  raycast from top of component.
                Vector3 top = o.transform.position + new Vector3(0f, o.transform.localScale.y * 0.5f + 1f /*offset*/, 0f);
                Ray r = new Ray(top, lp - top);
                bool ocluded = Physics.Raycast(r, radius);
                if (!ocluded) {
                    if (mo.pme is PMEGround) {
                        float dist = Vector3.Distance(lp, new Vector3(o.transform.position.x, lp.y, o.transform.position.z));
                        ( (PMEGround)mo.pme ).lightLevel += ( 1f - ( dist / radius ) );
                    }
                }
            }
        }
        foreach (Collider col in cols) {
            GameObject o = col.gameObject;
            MapObject mo = o.GetComponent<MapObject>();
            if (mo != null && mo.pme is PMEGround) {   //its a map object!
                ( (PMEGround)mo.pme ).ReAdjustLightLevel(mo.pme.LOD);
            }
        }

    }

    public Vector3 GroundPosition (Vector3Int globalPosition, float offset = 0) {
        PlacedMapEntity pme = MapEntityAt(globalPosition);
        float y = ( (PMEGround)pme ).groundLevel + offset;
        return new Vector3(globalPosition.x, y, globalPosition.z);
    }


    /*----------------------------------------------------
     * 
     *
     * 
     *  Render
     * 
     * 
     * 
     *----------------------------------------------------*/
    public RectInt Render (Vector3Int center) {
        RectInt visibleRect = new RectInt(center.x - viewDistance, center.z - viewDistance, viewDistance * 2, viewDistance * 2);
        currentBorder = new RectInt(0, 0, 0, 0);
        MapEntityFactory.shared.StartCoroutine(RenderCR(center));

        for (int iz = visibleRect.yMin; iz <= visibleRect.yMax; iz += MapChunk.horizontalSize) {
            for (int ix = visibleRect.xMin; ix <= visibleRect.xMax; ix += MapChunk.horizontalSize) {

                RectInt renderedArea = Chunk(ChunkLocation(new Vector3Int(ix, 0, iz))).area;

                if (currentBorder.size.x == 0) { currentBorder = renderedArea; }  //init

                //  extend rect TODO: make RectInt extension .
                if (currentBorder.xMin > renderedArea.xMin) currentBorder.xMin = renderedArea.xMin;
                if (currentBorder.xMax < renderedArea.xMax) currentBorder.xMax = renderedArea.xMax;
                if (currentBorder.yMin > renderedArea.yMin) currentBorder.yMin = renderedArea.yMin;
                if (currentBorder.yMax < renderedArea.yMax) currentBorder.yMax = renderedArea.yMax;
            }
        }
        lastDrawnCenter = center;
        return currentBorder;

    }

    private IEnumerator RenderCR (Vector3Int center) {
        RectInt visibleRect = new RectInt(center.x - viewDistance, center.z - viewDistance, viewDistance * 2, viewDistance * 2);
        for (int iz = visibleRect.yMin; iz <= visibleRect.yMax; iz += MapChunk.horizontalSize) {
            for (int ix = visibleRect.xMin; ix <= visibleRect.xMax; ix += MapChunk.horizontalSize) {
                Chunk(ChunkLocation(new Vector3Int(ix, 0, iz))).Render(center);
                yield return null;
            }
        }
    }

    public bool RenderIfNeeded (Vector3Int center) {
        RectInt visibleRect = new RectInt(center.x - viewDistance, center.z - viewDistance, viewDistance * 2, viewDistance * 2);
        //Debug.Log("view:" + visibleRect + "  border:" + currentBorder);

        if (Vector3Int.Distance(center, lastDrawnCenter) > 0.5f) {
            //        if (!currentBorder.Contains(visibleRect)) {
            Render(center);
            //  add some margin
            visibleRect.x -= 5;
            visibleRect.y -= 5;
            visibleRect.width += 10;
            visibleRect.height += 10;

            ReleaseFarChunks(visibleRect);
            return true;
        }

        return false;
    }
}
