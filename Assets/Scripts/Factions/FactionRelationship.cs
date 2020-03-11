using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UtilityScripts;

public class FactionRelationship {

    protected Faction _faction1;
    protected Faction _faction2;


    private int relationshipStatInt;
    //protected FACTION_RELATIONSHIP_STATUS _relationshipStatus;

    public int currentWarCombatCount { get; private set; } //this will be reset once relationship status is set to anything but AT_WAR

    #region getters/setters
    public FACTION_RELATIONSHIP_STATUS relationshipStatus {
		get { return (FACTION_RELATIONSHIP_STATUS)relationshipStatInt; }
    }
	public Faction faction1 {
		get { return _faction1; }
	}
	public Faction faction2 {
		get { return _faction2; }
	}
    #endregion

    public FactionRelationship(Faction faction1, Faction faction2) {
        _faction1 = faction1;
        _faction2 = faction2;
        relationshipStatInt = 0; //Friendly
    }

    #region Relationship Status
    public void SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS newStatus) {
		if(newStatus == relationshipStatus) {
            return;
        }
        FACTION_RELATIONSHIP_STATUS oldStatus = relationshipStatus;
        //_relationshipStatus = newStatus;
        relationshipStatInt = (int)newStatus;
        Messenger.Broadcast(Signals.CHANGE_FACTION_RELATIONSHIP, _faction1, _faction2, relationshipStatus, oldStatus);
        //if (_relationshipStatus != FACTION_RELATIONSHIP_STATUS.AT_WAR) {
        //    currentWarCombatCount = 0;
        //}
    }
    public void AdjustRelationshipStatus(int amount) {
        int previousValue = relationshipStatInt;
        relationshipStatInt += amount;
        relationshipStatInt = Mathf.Clamp(relationshipStatInt, 1, CollectionUtilities.GetEnumValues<FACTION_RELATIONSHIP_STATUS>().Length - 1);
        if (relationshipStatInt != previousValue) {
            Messenger.Broadcast(Signals.FACTION_RELATIONSHIP_CHANGED, this);
        }
    }
    #endregion

    public void AdjustWarCombatCount(int amount) {
        currentWarCombatCount += amount;
        currentWarCombatCount = Mathf.Max(0, currentWarCombatCount);
    }
}
