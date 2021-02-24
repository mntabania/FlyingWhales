using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class SummonPlayerSkill : SkillData {
    public override PLAYER_SKILL_CATEGORY category { get { return PLAYER_SKILL_CATEGORY.SUMMON; } }
    public RACE race { get; protected set; }
    public string className { get; protected set; }
    public SUMMON_TYPE summonType { get; protected set; }
    public virtual string bredBehaviour => CharacterManager.Instance.GetCharacterClass(className).traitNameOnTamedByPlayer;

    public SummonPlayerSkill() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    #region Overrides
    public override void ActivateAbility(LocationGridTile targetTile) {
        Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, PlayerManager.Instance.player.playerFaction, homeRegion: targetTile.parentMap.region as Region, className: className);
        summon.OnSummonAsPlayerMonster();
        CharacterManager.Instance.PlaceSummonInitially(summon, targetTile);

        BaseSettlement settlement = null;
        if (targetTile.structure.structureType != STRUCTURE_TYPE.WILDERNESS && targetTile.structure.structureType != STRUCTURE_TYPE.OCEAN && targetTile.IsPartOfSettlement(out settlement) && settlement.locationType != LOCATION_TYPE.VILLAGE) {
            summon.MigrateHomeStructureTo(targetTile.structure);
        } else {
            summon.SetTerritory(targetTile.area, false);
        }
        summon.jobQueue.CancelAllJobs();
        Messenger.Broadcast(PlayerSignals.PLAYER_PLACED_SUMMON, summon);
        // Messenger.Broadcast(Signals.PLAYER_GAINED_SUMMON, summon);
        base.ActivateAbility(targetTile);
    }
    public override void ActivateAbility(LocationGridTile targetTile, ref Character spawnedCharacter) {
        Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, PlayerManager.Instance.player.playerFaction, homeRegion: targetTile.parentMap.region as Region, className: className);
        CharacterManager.Instance.PlaceSummonInitially(summon, targetTile);
        //summon.behaviourComponent.AddBehaviourComponent(typeof(DefaultMinion));
        spawnedCharacter = summon;
        Messenger.Broadcast(PlayerSignals.PLAYER_PLACED_SUMMON, summon);
        base.ActivateAbility(targetTile, ref spawnedCharacter);
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            if (targetTile.structure is Kennel) {
                return false;
            }
            if (targetTile.structure.IsTilePartOfARoom(targetTile, out var structureRoom)) {
                if (structureRoom is PrisonCell) {
                    return false;
                }
            }
            if (!targetTile.IsPassable()) {
                //only allow summoning on linked tiles
                return false;
            }
            if (bredBehaviour == "Defender") {
                //if minion is defender then do not allow it to be spawned on villages.
                return !targetTile.IsPartOfActiveHumanElvenSettlement();
            }
            return true;
        }
        return false;
    }
    #endregion

    #region Virtuals
    protected virtual void AfterSummoning(Summon summon) {
        //What happens after summoning this monster
    }
    #endregion
}