using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Traits;
public class MeteorData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.METEOR;
    public override string name => "Meteor";
    public override string description => "This Spell spawns a flaming Meteor that will crash down and deal major Fire damage to a small target area.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public MeteorData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        //CreateMeteorStrikeAt(targetTile);
        targetTile.AddMeteor();
        
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