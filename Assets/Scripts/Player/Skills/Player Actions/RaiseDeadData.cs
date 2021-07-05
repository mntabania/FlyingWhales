using System.Collections;
using System.Collections.Generic;
using Logs;
using Object_Pools;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class RaiseDeadData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RAISE_DEAD;
    public override string name => "Raise Dead";
    public override string description => "This Action can be used on a Villager corpse to spawn a Skeleton. The Skeleton will belong to the Undead Faction.";
    public RaiseDeadData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
    }
    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        //IncreaseThreatForEveryCharacterThatSeesPOI(targetPOI, 5);
        Character target = null;
        if (targetPOI is Character character) {
            target = character;
        } else if (targetPOI is Tombstone tombstone) {
            target = tombstone.character;
        }
        Assert.IsNotNull(target);
        if (target.grave != null) {
            if (target.grave.isBeingCarriedBy != null) {
                target.grave.isBeingCarriedBy.UncarryPOI(target.grave);
            }
        }
        Summon summon = CharacterManager.Instance.RaiseFromDeadReplaceCharacterWithSkeleton(target, FactionManager.Instance.undeadFaction);
        //target.RaiseFromDeath(1, faction: PlayerManager.Instance.player.playerFaction, className: target.characterClass.className);

        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "player_raise_dead", null, LogUtilities.Player_Life_Changes_Tags);
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        LogPool.Release(log);
        if (UIManager.Instance.characterInfoUI.isShowing) {
            UIManager.Instance.characterInfoUI.CloseMenu();
        }
        base.ActivateAbility(targetPOI);
        int m_addedMaxHP = Mathf.RoundToInt(summon.combatComponent.maxHP * (PlayerSkillManager.Instance.GetAdditionalMaxHpPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.RAISE_DEAD) / 100f));
        int m_addedAttack = Mathf.RoundToInt(summon.combatComponent.attack * (PlayerSkillManager.Instance.GetAdditionalAttackPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.RAISE_DEAD) / 100f));

        summon.combatComponent.AdjustMaxHPModifier(m_addedMaxHP);
        summon.combatComponent.AdjustAttackModifier(m_addedAttack);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (!targetCharacter.isDead || !targetCharacter.carryComponent.IsNotBeingCarried() || targetCharacter.marker == null || targetCharacter.characterClass.IsZombie()) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.characterClass.IsZombie()) {
            reasons += $"Cannot use Raise Dead on Zombie Villagers,";
        }
        return reasons;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(TileObject targetTileObject) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetTileObject);
        if(targetTileObject is Tombstone tombstone && tombstone.character != null && tombstone.character.traitContainer.IsBlessed()) {
            reasons += $"Cannot use Raise Dead on Blessed Villagers,";
        }
        return reasons;
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if(tileObject is Tombstone tombstone) {
            return CanPerformAbilityTowards(tombstone.character);
        }
        return false;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        Character character = null;
        if (target is Tombstone tombstone) {
            character = tombstone.character;
        } else if (target is Character dead) {
            character = dead;
        }
        if(character != null) {
            if (!character.isDead) {
                return false;
            } else if (!character.race.IsSapient()) {
                return false;
            }
        }
        bool baseIsValid = base.IsValid(target);
        if (!baseIsValid) {
            if (target is Tombstone tomb && tomb.isBeingCarriedBy != null) {
                return true; //if tombstone is being carried by someone, bypass invalidity from base IsValid because of null mapObjectVisual
            }
        }
        return baseIsValid;
    }
    #endregion
}