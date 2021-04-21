using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Locations.Settlements;

public class MinionPlayerSkill : SkillData {
    public MINION_TYPE minionType = MINION_TYPE.Envy;
    public override PLAYER_SKILL_CATEGORY category { get { return PLAYER_SKILL_CATEGORY.MINION; } }
    public RACE race { get; protected set; }
    public string className { get; protected set; }

    public MinionPlayerSkill() : base() {
        race = RACE.DEMON;
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        Minion minion = CharacterManager.Instance.CreateNewMinion(className, RACE.DEMON, false);
        //minion.SetCombatAbility(COMBAT_ABILITY.FLAMESTRIKE);
        minion.Summon(targetTile);
        minion.SetMinionPlayerSkillType(type);
        BaseSettlement settlement = null;
        if (targetTile.IsPartOfSettlement(out settlement) && settlement.locationType != LOCATION_TYPE.VILLAGE && targetTile.structure.structureType != STRUCTURE_TYPE.WILDERNESS && targetTile.structure.structureType != STRUCTURE_TYPE.OCEAN) {
            minion.character.MigrateHomeStructureTo(targetTile.structure);
        } else {
            minion.character.SetTerritory(targetTile.area, false);
        }
        minion.character.jobQueue.CancelAllJobs();
        base.ActivateAbility(targetTile);
    }
    public override void ActivateAbility(LocationGridTile targetTile, ref Character spawnedCharacter) {
        Minion minion = CharacterManager.Instance.CreateNewMinion(className, RACE.DEMON, false);
        //minion.SetCombatAbility(COMBAT_ABILITY.FLAMESTRIKE);
        //PlayerManager.Instance.player.AddMinion(minion);
        minion.Summon(targetTile);
        minion.SetMinionPlayerSkillType(type);
        spawnedCharacter = minion.character;
        base.ActivateAbility(targetTile, ref spawnedCharacter);
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            if (!targetTile.IsPassable()) {
                //only allow summoning on linked tiles
                return false;
            }
            CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(className);
            if (characterClass.traitNameOnTamedByPlayer == "Defender") {
                //if minion is defender then do not allow it to be spawned on villages.
                return !targetTile.IsPartOfActiveHumanElvenSettlement();
            }
            return true;
        }
        return false;
    }
    public override void FinishCooldown() {
        base.FinishCooldown();
        SetCooldown(-1);
    }
}
