using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Traits;
public class SpawnNecronomiconData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPAWN_NECRONOMICON;
    public override string name => "Spawn Necronomicon";
    public override string description => "This Spell will create a Necronomicon on the target ground. If an appropriate character picks it up, it will turn into a Necromancer." +
        "\nA Necromancer produces a Chaos Orb each time it raises a Skeleton. It also produces 2 Chaos Orbs whenever it or its army of skeletons kill a Villager.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public SpawnNecronomiconData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        //CreateMeteorStrikeAt(targetTile);
        targetTile.AddNecronomicon(); //two plagued rats
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