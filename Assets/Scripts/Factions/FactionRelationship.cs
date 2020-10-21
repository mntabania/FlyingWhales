using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UtilityScripts;

public class FactionRelationship {

    protected Faction _faction1;
    protected Faction _faction2;


    private int _relationshipStatInt;
    //protected FACTION_RELATIONSHIP_STATUS _relationshipStatus;

    #region getters/setters
    public FACTION_RELATIONSHIP_STATUS relationshipStatus => (FACTION_RELATIONSHIP_STATUS)_relationshipStatInt;
    public int relationshipStatInt => _relationshipStatInt;
    public Faction faction1 {
		get { return _faction1; }
	}
	public Faction faction2 {
		get { return _faction2; }
	}
    #endregion

    public FactionRelationship(Faction faction1, Faction faction2, int relationshipStatInt = 0) {
        _faction1 = faction1;
        _faction2 = faction2;
        _relationshipStatInt = relationshipStatInt; //Default is Friendly
    }

    #region Relationship Status
    public bool SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS newStatus) {
		if(newStatus == relationshipStatus) {
            return false;
        }
        if ((_faction1.factionType.type == FACTION_TYPE.Vagrants || _faction2.factionType.type == FACTION_TYPE.Vagrants) && newStatus == FACTION_RELATIONSHIP_STATUS.Hostile) {
            if (faction1.factionType.type == FACTION_TYPE.Human_Empire || faction1.factionType.type == FACTION_TYPE.Elven_Kingdom || faction2.factionType.type == FACTION_TYPE.Human_Empire || faction2.factionType.type == FACTION_TYPE.Elven_Kingdom) {
                throw new Exception($"Setting relationship between {_faction2.name} and {_faction1.name} to Hostile!");    
            }
        }
        FACTION_RELATIONSHIP_STATUS oldStatus = relationshipStatus;
        //_relationshipStatus = newStatus;
        _relationshipStatInt = (int)newStatus;
        Messenger.Broadcast(Signals.CHANGE_FACTION_RELATIONSHIP, _faction1, _faction2, relationshipStatus, oldStatus);
        //if (_relationshipStatus != FACTION_RELATIONSHIP_STATUS.AT_WAR) {
        //    currentWarCombatCount = 0;
        //}
        return true;
    }
    #endregion
}
