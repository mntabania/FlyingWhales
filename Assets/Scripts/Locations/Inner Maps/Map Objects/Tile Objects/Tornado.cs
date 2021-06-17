using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class Tornado : MovingTileObject {

    public int radius { get; private set; }
    public GameDate expiryDate { get; private set; }
    private TornadoMapObjectVisual _tornadoMapObjectVisual;
    public override string neutralizer => "Wind Master";
    protected override int affectedRange => 2;
    public override System.Type serializedData => typeof(SaveDataTornado);
    public Tornado() {
        Initialize(TILE_OBJECT_TYPE.TORNADO, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        SetRadius(2);
    }
    public Tornado(SaveDataTornado data) : base(data) {
        //SaveDataTornado saveDataTornado = data as SaveDataTornado;
        Assert.IsNotNull(data);
        expiryDate = data.expiryDate;
        SetRadius(data.radius);
        hasExpired = data.hasExpired;
    }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _tornadoMapObjectVisual = mapVisual as TornadoMapObjectVisual;
        Assert.IsNotNull(_tornadoMapObjectVisual, $"Map Object Visual of {this} is null!");
    }
    public override void Neutralize() {
        _tornadoMapObjectVisual.Expire();
    }
    public void SetRadius(int radius) {
        this.radius = radius;
    }
    public void SetExpiryDate(GameDate expiry) {
        expiryDate = expiry;
    }
    public override void Expire() {
        if (hasExpired) {
            return;
        }
        base.Expire();
        Messenger.Broadcast<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, this, null, base.gridTileLocation);
    }
    public override string ToString() {
        return $"Tornado {id.ToString()}";
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        traitContainer.AddTrait(this, "Dangerous");
        traitContainer.RemoveTrait(this, "Flammable");
    }

    #region Moving Tile Object
    protected override bool TryGetGridTileLocation(out LocationGridTile tile) {
        if (mapVisual != null) {
            TornadoMapObjectVisual tornadoMapObjectVisual = mapVisual as TornadoMapObjectVisual;
            if (tornadoMapObjectVisual.isSpawned) {
                tile = tornadoMapObjectVisual.gridTileLocation;
                return true;
            }
        }
        tile = null;
        return false;
    }
    #endregion
}
#region Save Data
public class SaveDataTornado : SaveDataMovingTileObject {
    public GameDate expiryDate;
    public int radius;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Tornado tornado = tileObject as Tornado;
        Assert.IsNotNull(tornado);
        expiryDate = tornado.expiryDate;
        radius = tornado.radius;
    }
}
#endregion