using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using UtilityScripts;

public class HeartOfTheWind : Artifact {

    public HeartOfTheWind() : base(ARTIFACT_TYPE.Heart_Of_The_Wind) {
        maxHP = 700;
        currentHP = maxHP;
    }
    public HeartOfTheWind(SaveDataArtifact data) : base(data) { }

    #region Overrides
    public override void ConstructDefaultActions() {
        base.ConstructDefaultActions();
        AddAdvertisedAction(INTERACTION_TYPE.INSPECT);
    }
    public override void ActivateTileObject() {
        if (gridTileLocation != null) {
            base.ActivateTileObject();
            SpawnTornado();
        }
    }
    private void SpawnTornado() {
        Tornado tornado = new Tornado();
        tornado.SetRadius(1);
        tornado.SetExpiryDate(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(Random.Range(1, 4))));
        tornado.SetGridTileLocation(gridTileLocation);
        tornado.OnPlacePOI();
    }
    public override void OnInspect(Character inspector) {
        base.OnInspect(inspector);
        SpawnTornado();
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Tile Object", "Berserk Orb", "inspect", providedTags: LOG_TAG.Life_Changes);
        log.AddToFillers(inspector, inspector.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToDatabase(true);

        if (GameUtilities.RollChance(30)) {
            gridTileLocation.structure.RemovePOI(this, inspector);
        }
    }
    #endregion
}
