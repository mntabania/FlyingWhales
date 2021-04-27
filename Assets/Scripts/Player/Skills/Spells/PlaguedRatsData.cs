using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Traits;
public class PlaguedRatData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PLAGUED_RAT;
    public override string name => "Plagued Rats";
    public override string description => "This Spell spawns 2 Plagued Rats. These virulent rats will transmit Plague to food sources they come in contact with.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public PlaguedRatData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        //CreateMeteorStrikeAt(targetTile);
        targetTile.AddPlaguedRats(isFromSpell: true); //two plagued rats
        targetTile.AddPlaguedRats(true, isFromSpell: true);
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