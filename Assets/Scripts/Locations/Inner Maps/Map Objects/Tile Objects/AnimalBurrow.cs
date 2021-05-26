using UnityEngine;

public abstract class AnimalBurrow : TileObject {
    public override Vector2 selectableSize => new Vector2(1.7f, 1.7f);
    public override Vector3 worldPosition {
        get {
            Vector2 pos = mapVisual.transform.position;
            pos.x += 0.5f;
            pos.y += 0.5f;
            return pos;
        }
    }
    public SUMMON_TYPE monsterToSpawn { get; private set; }

    public AnimalBurrow(SUMMON_TYPE p_summonType) {
        monsterToSpawn = p_summonType;
    }
    public AnimalBurrow(SaveDataTileObject data, SUMMON_TYPE p_summonType) : base(data) {
        monsterToSpawn = p_summonType;
    }
}
