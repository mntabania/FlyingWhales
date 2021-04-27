using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Scriptable_Object_Scripts;

public class FreezingTrapData : SkillData {

    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FREEZING_TRAP;
    public override string name => "Freezing Trap";
    public override string description => "This Spell places an invisible trap on a target unoccupied tile. Any character that walks into the tile will activate it and become Frozen." +
        "\nTrapping a hostile Villager produces 2 Chaos Orbs. Additionally, characters Frozen by the player has a small chance of periodically producing Chaos Orbs.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    public virtual int abilityRadius => 1;

    public FreezingTrapData() : base() {
        targetTypes = new[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        targetTile.tileObjectComponent.SetHasFreezingTrap(true, true);
        AudioManager.Instance.TryCreateAudioObject(
            PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<FreezingTrapSkillData>(PLAYER_SKILL_TYPE.FREEZING_TRAP).placeTrapSound, targetTile, 1, false);
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            return !targetTile.tileObjectComponent.hasFreezingTrap;
        }
        return false;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}