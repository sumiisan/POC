using UnityEngine;
using System.Collections;

public enum MapEntityType {
    air = 0,
    ground = 1,
}

//  マップ上オブジェクトとして保存できる
public interface MapCodable {
}

//  マップ上に生成できる
public interface MapConstructable {
    void Construct (Vector3Int chunkPosition, string genom);
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
    public float LODScale;
    public PlacedMapEntity[] children = { null, null, null, null };
    public string genom = "";

    public FloatSections lodDistance = new FloatSections(
        new float[]{
            Mathf.Pow(24f, 2f) * 2f,//level 0 = 1x1
            Mathf.Pow(32f, 2f) * 2f,//level 1 = 2x2
            Mathf.Pow(40f, 2f) * 2f,//level 2 = 4x4
            Mathf.Pow(48f, 2f) * 2f,//level 3 = 8x8
            Mathf.Pow(56f, 2f) * 2f,//level 4 = 16x16
            Mathf.Pow(100f, 2f) * 2f,//level 5 = 32x32
    });


    public PlacedMapEntity (MapEntityType type, Vector3 position, float LODScale) {
        this.position = position;
        this.type = type;
        this.LODScale = LODScale;
    }

    protected Vector3 WorldPosition (Vector3Int chunkPosition) {
        return new Vector3(position.x * LODScale + chunkPosition.x, position.y * LODScale + chunkPosition.y, position.z * LODScale + chunkPosition.z);
    }

    virtual public void Construct (Vector3Int chunkPosition, string genom) {
    }
    virtual public void Render (Vector3Int offset, Vector3Int center) {
    }
    virtual public void ClearGameObject () {
    }
    virtual public void Release () {
    }

}

public class PMEGround : PlacedMapEntity {
    GameObject gameObject;

    public PMEGround (Vector3 position, float LODScale) : base(MapEntityType.ground, position, LODScale) {

    }

    public override void Construct (Vector3Int globalLoc, string genom) {
        base.Construct(globalLoc, genom);
        float LODOffset = LODScale * 0.5f;
        Vector2 p = new Vector2(globalLoc.x + LODOffset, globalLoc.z + LODOffset);

        float detailScale = 0.2f;
        float detail = Mathf.PerlinNoise(( p.x + 0x7fff ) * detailScale, ( p.y + 0x7fff ) * detailScale) * 3f;
        float coarseScale = 0.04f;
        float coarse = Mathf.PerlinNoise(( p.x + 0x3fff ) * coarseScale, ( p.y + 0x3fff ) * coarseScale) * 50f;
        float flatnessScale = 0.007f;
        float flatness = Mathf.PerlinNoise(( p.x + 0x3fff ) * flatnessScale, ( p.y + 0x3fff ) * flatnessScale);

        size = new Vector3(MapRenderer.scale, ( ( coarse + detail ) * flatness ) * MapRenderer.scale, MapRenderer.scale);
        //debug:       size = new Vector3(MapRenderer.scale, MapRenderer.scale * globalLoc.x * 0.1f, MapRenderer.scale);
    }

    public override void Render (Vector3Int globalLoc, Vector3Int center) {
        base.Render(globalLoc, center);
        Vector3 pos = globalLoc;
        float sqrDist = Mathf.Pow(pos.x - center.x, 2f) + Mathf.Pow(pos.z - center.z, 2f);
        int desiredLODScale = (int)Mathf.Pow(2f, lodDistance.Index(sqrDist));

        //        Render(offset, desiredLODScale);
        if (LODScale <= desiredLODScale) {
            if (gameObject == null) {
                gameObject = MapEntityFactory.Instantiate(MapEntityFactory.shared.groundPrefab);
            }
            float LODOffset = LODScale * 0.5f;
            gameObject.transform.position = new Vector3(globalLoc.x + LODOffset, size.y * 0.5f, globalLoc.z + LODOffset) * MapRenderer.scale;
            gameObject.transform.localScale = new Vector3(size.x * LODScale, size.y, size.z * LODScale);

            for (int i = 0; i < 4; ++i) {
                PlacedMapEntity child = children[i];
                if (child != null) {
                    child.ClearGameObject();
                }
            }
        } else {
            if (gameObject) {
                ClearGameObject();
            }
            for (int z = 0; z < 2; ++z) {
                for (int x = 0; x < 2; ++x) {
                    PlacedMapEntity child = children[z * 2 + x];
                    if (child != null) {
                        int childLODScale = (int)LODScale / 2;
                        child.Render(globalLoc + new Vector3Int(x * childLODScale, 0, z * childLODScale), center);
                    }
                }
            }

        }
    }

    public override void ClearGameObject () {
        if (gameObject) {
            MapEntityFactory.Destroy(gameObject);
        }
        for (int i = 0; i < 4; ++i) {
            if (children[i] != null) children[i].ClearGameObject();
        }
    }

    public override void Release () {
        base.Release();
        for (int i = 0; i < 4; ++i) {
            if (children[i] != null) children[i].Release();
        }
        MapEntityFactory.Destroy(gameObject);
    }

}

