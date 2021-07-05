using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public interface IPartyQuestTarget {
    LocationStructure currentStructure { get; }
    BaseSettlement currentSettlement { get; }
}

public interface IGatheringTarget {
    LocationStructure currentStructure { get; }
    BaseSettlement currentSettlement { get; }
}

public interface IPartyTargetDestination {
    string persistentID { get; }
    string name { get; }
    PARTY_TARGET_DESTINATION_TYPE partyTargetDestinationType { get; }
    bool hasBeenDestroyed { get; }
    Region region { get; }

    LocationGridTile GetRandomPassableTile();
    bool IsAtTargetDestination(Character character);
}