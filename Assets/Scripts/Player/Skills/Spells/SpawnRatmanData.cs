using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Traits;
public class SpawnRatmanData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPAWN_RATMAN;
    public override string name => "Spawn Ratman";
    public override string description => "This Spell will spawns a single Ratman on the target ground.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public SpawnRatmanData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        //CreateMeteorStrikeAt(targetTile);
        CharacterManager.Instance.GenerateRatmen(targetTile, 1);
        base.ActivateAbility(targetTile);
    }
    //private void CreateMeteorStrikeAt(LocationGridTile tile) {
    //    GameObject meteorGO = InnerMapManager.Instance.mapObjectFactory.CreateNewMeteorObject();
    //    meteorGO.transform.SetParent(tile.parentMap.structureParent);
    //    meteorGO.transform.position = tile.centeredWorldLocation;
    //    meteorGO.GetComponent<MeteorVisual>().MeteorStrike(tile, abilityRadius);
    //}
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}