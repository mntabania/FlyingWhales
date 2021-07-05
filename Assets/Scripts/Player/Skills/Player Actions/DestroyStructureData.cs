using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class DestroyStructureData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DESTROY_STRUCTURE;
    public override string name => "Destroy";
    public override string description => "This Ability can be used to destroy Demonic Structure.";
    public DestroyStructureData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    private LocationStructure m_targetStructure;

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        m_targetStructure = structure;
        UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation("Destroy Structure.", $"Are you sure you want to destroy {structure.name}?", OnActualDestroyStructure, showCover: true, layer: 150);
    }

    void OnActualDestroyStructure() {
        if (m_targetStructure is DemonicStructure demonicStructure) {
            demonicStructure.AdjustHP(-m_targetStructure.currentHP, isPlayerSource: true);
        }
        base.ActivateAbility(m_targetStructure);
        
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "InterventionAbility", "Destroy Structure", "activated", null, LOG_TAG.Player);
        log.AddToFillers(m_targetStructure, m_targetStructure.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
    }
    public override bool CanPerformAbilityTowards(LocationStructure structure) {
        bool canPerform = base.CanPerformAbilityTowards(structure);
        if (canPerform) {
            return !structure.hasBeenDestroyed && structure.tiles.Count > 0 && structure.currentHP > 0;
        }
        return canPerform;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if(target is DemonicStructure structure) {
            if (structure.structureType == STRUCTURE_TYPE.THE_PORTAL) {
                //Cannot destroy portal
                return false;
            }
            if (structure.hasBeenDestroyed || structure.tiles.Count <= 0) {
                return false;
            }
        } else {
            return false;
        }
        return base.IsValid(target);
    }
    #endregion
}
