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
    void Construct (Vector3Int globalLocation, string genom);
}

//  マップ上に配置できる
public interface MapPlacable {
    void Render (Vector3Int offset);
    void Release ();
}

public class MapEntity : MapCodable, MapPlacable, MapConstructable {
    public MapEntityType type = MapEntityType.ground;
    virtual public void Construct (Vector3Int globalLocation, string genom) {
    }
    virtual public void Render (Vector3Int offset) {
    }
    virtual public void Release () {
    }
}

public class PlacedMapEntity : MapEntity {
    public Vector3Int mapLocation;
    public Vector3 size = Vector3.one;
    public string genom = "";

    public PlacedMapEntity (MapEntityType type, Vector3Int mapLocation) {
        this.mapLocation = mapLocation;
        this.type = type;
    }

    protected Vector3 WorldPosition (Vector3Int offset) {
        return new Vector3(mapLocation.x + offset.x, mapLocation.y + offset.y, mapLocation.z + offset.z);
    }
}

public class PMEGround : PlacedMapEntity {
    GameObject gameObject;


    public PMEGround (Vector3Int mapLocation) : base(MapEntityType.ground, mapLocation) {

    }

    public override void Construct (Vector3Int globalLocation, string genom) {
        base.Construct(globalLocation, genom);

        float detailScale = 0.2f;
        float detail = Mathf.PerlinNoise(( globalLocation.x + 0x7fff ) * detailScale, ( globalLocation.z + 0x7fff ) * detailScale) * 3f;
        float coarseScale = 0.04f;
        float coarse = Mathf.PerlinNoise(( globalLocation.x + 0x3fff ) * coarseScale, ( globalLocation.z + 0x3fff ) * coarseScale) * 50f;
        float flatnessScale = 0.007f;
        float flatness = Mathf.PerlinNoise(( globalLocation.x + 0x3fff ) * flatnessScale, ( globalLocation.z + 0x3fff ) * flatnessScale);

        size = new Vector3(MapRenderer.scale, ( ( coarse + detail ) * flatness ) * MapRenderer.scale, MapRenderer.scale);
    }

    public override void Render (Vector3Int offset) {
        base.Render(offset);

        if (gameObject == null) {
            gameObject = MapEntityFactory.Instantiate(MapEntityFactory.shared.groundPrefab);
        }

        gameObject.transform.position = WorldPosition(offset) * MapRenderer.scale + new Vector3(0f, size.y * 0.5f, 0f);
        gameObject.transform.localScale = size;
    }

    public override void Release () {
        base.Release();
        MapEntityFactory.Destroy(gameObject);
    }
}

