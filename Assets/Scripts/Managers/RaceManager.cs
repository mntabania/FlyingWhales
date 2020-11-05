using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class RaceManager : BaseMonoBehaviour {
    public static RaceManager Instance;

    public RaceDataDictionary racesDictionary;
    //private Dictionary<RACE, INTERACTION_TYPE[]> _npcRaceInteractions;

    void Awake() {
        Instance = this;    
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
    }

    public void Initialize() {
        //ConstructNPCRaceInteractions();
    }

    #region General
    public bool CanCharacterDoGoapAction(Character character, INTERACTION_TYPE goapType) {
        bool isTrue = false;
        Dictionary<INTERACTION_TYPE, GoapAction> goapActionData = InteractionManager.Instance.goapActionData;
        if (goapActionData.ContainsKey(goapType)) {
            isTrue = goapActionData[goapType].DoesCharacterMatchRace(character);
        }
        //if (!isTrue) {
        //    if (character.role.allowedInteractions != null) {
        //        isTrue = character.role.allowedInteractions.Contains(goapType);
        //    }
        //}
        //if (!isTrue) {
        //    isTrue = character.currentInteractionTypes.Contains(goapType);
        //}
        return isTrue;
    }
    public RaceData GetRaceData(RACE race) {
        if (racesDictionary.ContainsKey(race)) {
            return racesDictionary[race];
        }
        return null;
    }
    #endregion
}
