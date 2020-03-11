using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFactionComponent {
    public Character owner { get; private set; }
    public int loyalty { get; private set; }
    
    public CharacterFactionComponent(Character owner) {
        this.owner = owner;
    }

    #region General
    public void OnJoinFaction(Faction factionJoined) {
        ResetLoyalty();
    }
    #endregion

    #region Loyalty
    private void ResetLoyalty() {
        loyalty = 50;
    }
    public void AdjustLoyalty(int amount) {
        if (!owner.faction.isMajorNonPlayer) { return; }
        loyalty += amount;
        loyalty = Mathf.Clamp(loyalty, -100, 100);
        if(loyalty == -100) {
            owner.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, owner, "left_faction_no_loyalty");
            //Faction prevFaction = owner.faction;
            //owner.ChangeFactionTo(FactionManager.Instance.friendlyNeutralFaction);
            //Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "left_faction_no_loyalty");
            //log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //log.AddToFillers(prevFaction, prevFaction.name, LOG_IDENTIFIER.FACTION_1);
            //owner.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        }
    }
    #endregion
}
