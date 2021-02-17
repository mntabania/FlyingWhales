using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Scriptable_Object_Scripts;

public class FreezingTrapData : SkillData {

    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FREEZING_TRAP;
    public override string name => "Freezing Trap";
    public override string description => "This Spell places an invisible trap on a target unoccupied tile. Any character that walks into the tile will activate it and become Frozen.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    public virtual int abilityRadius => 1;

    public FreezingTrapData() : base() {
        targetTypes = new[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        targetTile.SetHasFreezingTrap(true);
        AudioManager.Instance.TryCreateAudioObject(
            PlayerSkillManager.Instance.GetPlayerSkillData<FreezingTrapSkillData>(PLAYER_SKILL_TYPE.FREEZING_TRAP).placeTrapSound, targetTile, 1, false);
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            return !targetTile.hasFreezingTrap;
        }
        return false;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}