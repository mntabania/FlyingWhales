using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class MinionPlayerSkill : SpellData {
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.MINION; } }
    public RACE race { get; protected set; }
    public string className { get; protected set; }

    public MinionPlayerSkill() : base() {
        race = RACE.DEMON;
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        Minion minion = PlayerManager.Instance.player.CreateNewMinion(className, RACE.DEMON, false);
        minion.SetCombatAbility(COMBAT_ABILITY.FLAMESTRIKE);
        minion.Summon(targetTile);
        minion.SetMinionPlayerSkillType(type);
        if (targetTile.structure?.settlementLocation != null && 
            targetTile.structure.settlementLocation.locationType != LOCATION_TYPE.SETTLEMENT) {
            minion.character.MigrateHomeStructureTo(targetTile.structure);	
        } else {
            minion.character.AddTerritory(targetTile.collectionOwner.partOfHextile.hexTileOwner, false);    
        }
        minion.character.jobQueue.CancelAllJobs();
        base.ActivateAbility(targetTile);
    }
    public override void ActivateAbility(LocationGridTile targetTile, ref Character spawnedCharacter) {
        Minion minion = PlayerManager.Instance.player.CreateNewMinion(className, RACE.DEMON, false);
        minion.SetCombatAbility(COMBAT_ABILITY.FLAMESTRIKE);
        //PlayerManager.Instance.player.AddMinion(minion);
        minion.Summon(targetTile);
        minion.SetMinionPlayerSkillType(type);
        spawnedCharacter = minion.character;
        base.ActivateAbility(targetTile, ref spawnedCharacter);
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            //only allow summoning on linked tiles
            return targetTile.collectionOwner.isPartOfParentRegionMap;
        }
        return false;
    }
}
