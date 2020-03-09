using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public interface IParty {
    int id { get; }
    int numOfAttackers { get; set; }
    string name { get; }
    string coloredUrlName { get; }
    string urlName { get; }
    float computedPower { get; }
    MODE currentMode { get; }
    Faction attackedByFaction { get; set; }
    Faction faction { get; }
    CharacterAvatar icon { get; }
    //NPCSettlement home { get; }
    //Combat currentCombat { get; set; }
    BaseLandmark landmarkLocation { get; }
    BaseLandmark homeLandmark { get; }
    Character mainCharacter { get; }
    ILocation specificLocation { get; }
    List<Character> characters { get; }

    void EndAction();
    void GoHome(Action action = null, Action actionOnStartOfMovement = null);
    void RemoveCharacter(Character icharacter);
    //void AdvertiseSelf(ActionThread actionThread);
    void SetSpecificLocation(ILocation location);
    bool AddCharacter(Character icharacter);
}
