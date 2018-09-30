using UnityEngine;
using System.Collections;

public enum MapEntityType {
    air = 0,
    ground = 1,
}

public struct RenderInfo {
    public Vector3Int globalPosition;
    public Vector3Int playerPosition;
    public RenderInfo (Vector3Int globalPosition, Vector3Int playerPositon) {
        this.globalPosition = globalPosition;
        this.playerPosition = playerPositon;
    }
}

//  マップ上オブジェクトとして保存できる
public interface MapCodable {
}

//  マップ上に生成できる
public interface MapConstructable {
    void Construct (Vector3Int chunkPosition, string genom);
    void AverageChildren ();
}

//  マップ上に配置できる
public interface MapPlacable {
    void Render (Vector3Int offset, Vector3Int center);
    void ClearGameObject ();
    void Release ();
}

public class MapEntity {
    public MapEntityType type = MapEntityType.ground;
}

public class PlacedMapEntity : MapEntity, MapCodable, MapPlacable, MapConstructable {
    public Vector3 position = Vector3.zero;
    public Vector3 size = Vector3.one;

    private static float[] LODList = new float[] { 32, 16, 8, 4, 2, 1 };

    private int _LOD = 5;
    public int LOD {
        get {
            return _LOD;
        }
        set {
            _LOD = value;
            LODScale = PlacedMapEntity.LODList[value];
        }
    }

    public float LODScale;
    public PlacedMapEntity parent = null;
    public PlacedMapEntity[] children = { null, null, null, null }; //  allow direct access to fast enumeration
    public string genom = "";

    protected bool isRendering = false;
    protected bool isActive = false;

    public FloatSections lodDistance = new FloatSections(
        new float[]{
            Mathf.Pow(18f, 2f) * 2f,//level 0 = 1x1
            Mathf.Pow(30f, 2f) * 2f,//level 1 = 2x2
            Mathf.Pow(38f, 2f) * 2f,//level 2 = 4x4
            Mathf.Pow(44f, 2f) * 2f,//level 3 = 8x8
            Mathf.Pow(80f, 2f) * 2f,//level 4 = 16x16
            Mathf.Pow(200f, 2f) * 2f,//level 5 = 32x32
        });

    public PlacedMapEntity (MapEntityType type, Vector3 position, int LOD) {
        this.position = position;
        this.type = type;
        this.LOD = LOD;
    }

    protected Vector3 WorldPosition (Vector3Int chunkPosition) {
        return new Vector3(position.x * LODScale + chunkPosition.x, position.y * LODScale + chunkPosition.y, position.z * LODScale + chunkPosition.z);
    }

    public void AddChild (int x, int z, PlacedMapEntity c) {
        children[z * 2 + x] = c;
        c.parent = this;
    }

    public PlacedMapEntity ChildAt (int x, int z) {
        return children[z * 2 + x];
    }

    virtual public void Construct (Vector3Int chunkPosition, string genom) {
    }
    virtual public void AverageChildren () {
    }
    virtual public void Render (Vector3Int offset, Vector3Int center) {
    }
    virtual public void ClearGameObject () {
    }
    virtual public void Release () {
    }

}

public enum BiomeType {
    abstract3d, abstract2d, canyon, desertCity, tundra, ocean, plainLakes, desert, steppe, riverHills, brookWood, dunes, highMountains, steepValley, jungle, vulkano
}


public class PMEGround : PlacedMapEntity {
    GameObject groundObject;
    Color baseColor = Color.magenta;
    float waterLevel = 0;

    public float groundLevel {
        get {
            return size.y;
        }
        set {
            size.y = value;
        }
    }

    private float _lightLevel = 0;
    public float lightLevel {
        get {
            return _lightLevel;
        }
        set {
            _lightLevel = value;
            if (groundObject != null) {
                MapEntityFactory.shared.SetColor(groundObject, baseColor, Mathf.Clamp(_lightLevel, 0.1f, 1f));
            }
        }
    }

    public void ReAdjustLightLevel (int baseLOD = -1) {
        if (baseLOD == -1) baseLOD = LOD;   //  use my own

        if (baseLOD >= LOD) {    // adjust parents
            if (baseLOD > LOD) {
                AverageChildren();
            }
            if (parent != null) {
                ( (PMEGround)parent ).ReAdjustLightLevel(baseLOD);
            }
        }

        if (baseLOD <= LOD) {    // adjust children
            foreach (PlacedMapEntity pme in children) {
                if (pme is PMEGround) {
                    ( (PMEGround)pme ).lightLevel = lightLevel;
                    ( (PMEGround)pme ).ReAdjustLightLevel(baseLOD);
                }
            }
        }

    }


    public PMEGround (Vector3 position, int LOD) : base(MapEntityType.ground, position, LOD) {

    }

    float Perlin (Vector2 centerPosition, float scale, float factor = 1f, float valueOffset = 0f, float valueGain = 1f) {
        float pn = ( Mathf.PerlinNoise(centerPosition.x * scale, centerPosition.y * scale) + valueOffset ) * valueGain;
        return Mathf.Clamp(pn, 0f, 1f) * factor;
    }

    public override void Construct (Vector3Int globalLoc, string genom) {
        base.Construct(globalLoc, genom);
        float LODOffset = LODScale * 0.5f;
        Vector2 p = new Vector2(globalLoc.x + LODOffset, globalLoc.z + LODOffset);

        BiomeType bt = BiomeType.canyon;
        float y = 0;
        waterLevel = 50f * MapRenderer.scale;

        switch (bt) {
        case BiomeType.ocean: {
                y = 49f;
                float bump = Perlin(p, 0.042f, 5f) + Perlin(p, 0.018f, 10f) - 8f;
                float s = 0.1f;
                float v = p.x * 0.01f * s + p.y * 0.01f * s + Perlin(p, 0.06f, 2f);
                float noise = Perlin(p, 0.05f, 0.5f) * 2f - 1f;
                float mountain = ( Mathf.Clamp(( Mathf.Sin(v * Mathf.PI * 2f) + noise ) + 1f, 1.5f, 2.5f) - 1.5f ) * 15f;
                y += bump * 1f;
                y += mountain;
            }
            break;
        case BiomeType.plainLakes: {
                y = 48.5f;
                float bump = Perlin(p, 0.042f, 5f) + Perlin(p, 0.018f, 10f);
                float v = p.x * 0.01f + p.y * 0.01f + Perlin(p, 0.08f, 1.5f);
                float brook = Mathf.Clamp(( Mathf.Sin(v * Mathf.PI * 2f) ) + 1f, 0.0f, 0.2f) * 10f;
                float bp = Mathf.Pow(( Perlin(p, 0.005f) - 0.5f ) * 3f, 3f) * bump;
                y += bp;
                waterLevel += bp;
                y += brook;
            }
            break;
        case BiomeType.canyon: {
                float v = p.x * 0.01f + p.y * 0.01f + Perlin(p, 0.02f, 3f);
                float multiplier = 30f * ( 0.5f + Perlin(p, 0.04f, 0.5f) );
                float noise = Perlin(p, 0.1f, 0.5f) - 0.7f;
                float steepNess = Perlin(p, 0.000067f, 0.8f) + 0.2f;
                y = Mathf.Clamp(( Mathf.Sin(v * steepNess * Mathf.PI * 2f) + 1f ) + noise, 0f, 1f) * multiplier + 50f;
                y -= Perlin(p, 0.01f, 1f) - 0.5f;
            }
            break;
        default: {
                y = Perlin(p, 0.2f, 3f) + Perlin(p, 0.04f, 50f) + 50f;
            }
            break;
        }
        size = new Vector3(MapRenderer.scale, 0f, MapRenderer.scale);
        groundLevel = y * MapRenderer.scale;
    }

    public override void AverageChildren () {
        float sumGroundLevel = 0;
        float sumWaterLevel = 0;
        float sumLightLevel = 0;
        int c = 0;

        foreach (PlacedMapEntity pme in children) {
            if (pme is PMEGround) {
                PMEGround pmeg = (PMEGround)pme;
                sumGroundLevel += pmeg.groundLevel;
                sumWaterLevel += pmeg.waterLevel;
                sumLightLevel += pmeg.lightLevel;
                ++c;
            }
        }

        groundLevel = sumGroundLevel / c;
        waterLevel = sumWaterLevel / c;
        lightLevel = sumLightLevel / c;
    }

    public override void Render (Vector3Int globalLoc, Vector3Int center) {
        if (isRendering) return;
        base.Render(globalLoc, center);
        isRendering = true;
        RenderInfo info = new RenderInfo(globalLoc, center);
        //        MapEntityFactory.shared.StartCoroutine(RenderCR(info));
        RenderCR(info);

        isRendering = false;
    }

    private void RenderCR (RenderInfo info) {

        Vector3 pos = info.globalPosition;
        Vector3Int center = info.playerPosition;
        float sqrDist = Mathf.Pow(pos.x - center.x, 2f) + Mathf.Pow(pos.z - center.z, 2f);
        int desiredLODScale = (int)Mathf.Pow(2f, lodDistance.Index(sqrDist));

        //        Render(offset, desiredLODScale);
        if (LODScale <= desiredLODScale) {
            if (isActive) return;// break;
            isActive = true;
            //
            //  active
            //
            float h = groundLevel;
            float LODOffset = LODScale * 0.5f;
            if (h > waterLevel) { //  above water
                if (groundObject == null) {
                    baseColor = Color.white;
                    groundObject = NewMapObject(MapEntityFactory.shared.groundPrefab, baseColor).gameObject;
                }
            } else { //  below water
                if (groundObject == null) {
                    baseColor = Color.blue;
                    groundObject = NewMapObject(MapEntityFactory.shared.oceanPrefab, baseColor).gameObject;
                }
                h = waterLevel;
            }
            groundObject.transform.position = new Vector3(pos.x + LODOffset, h * 0.5f, pos.z + LODOffset) * MapRenderer.scale;
            groundObject.transform.localScale = new Vector3(size.x * LODScale, h, size.z * LODScale);
            for (int i = 0; i < 4; ++i) {
                PlacedMapEntity child = children[i];
                if (child != null) {
                    child.ClearGameObject();
                }
            }
        } else {
            isActive = false;
            //
            //  inactive (pass to children)
            //
            if (groundObject) {
                ClearGameObject();
            }
            for (int z = 0; z < 2; ++z) {
                for (int x = 0; x < 2; ++x) {
                    PlacedMapEntity child = ChildAt(x, z);
                    if (child != null) {
                        int childLODScale = (int)LODScale / 2;
                        child.Render(info.globalPosition + new Vector3Int(x * childLODScale, 0, z * childLODScale), center);
                    }
                }
            }
            //yield return null;
        }
    }

    private MapObject NewMapObject (GameObject prefab, Color color) {
        GameObject o = MapEntityFactory.Instantiate(prefab);
        MapEntityFactory.shared.SetColor(o, color, Mathf.Clamp(lightLevel, 0.1f, 1f));
        MapObject mo = o.AddComponent<MapObject>();
        mo.pme = this;
        return mo;
    }

    public override void ClearGameObject () {
        isActive = false;
        if (groundObject) {
            MapEntityFactory.Destroy(groundObject);
        }
        for (int i = 0; i < 4; ++i) {
            if (children[i] != null) children[i].ClearGameObject();
        }
    }

    public override void Release () {
        base.Release();
        isActive = false;
        for (int i = 0; i < 4; ++i) {
            if (children[i] != null) children[i].Release();
        }
        MapEntityFactory.Destroy(groundObject);
    }

}

