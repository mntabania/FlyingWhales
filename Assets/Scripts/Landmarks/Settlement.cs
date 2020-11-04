/*
 This will replace the city script.
 */
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using ECS;

public class Settlement : BaseLandmark {

    private float _currentPopulationProduction;

    private int _civilianCount;

	//Crater
	private int _numOfPsytoxinated;

    #region getters/setters
	public int numOfPsytoxinated {
		get { return _numOfPsytoxinated; }
	}
    public int civilianCount {
        get { return _civilianCount; }
    }
    #endregion

    public Settlement(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        _canBeOccupied = true;
    }

    public Settlement(HexTile location, LandmarkSaveData data): base(location, data) {
        _canBeOccupied = true;
    }

    #region Overrides
	public override void DestroyLandmark (bool putRuinStructure){
		base.DestroyLandmark (putRuinStructure);
		tileLocation.RuinStructureOnTile (!putRuinStructure);
		//if(_specificLandmarkType == LANDMARK_TYPE.CRATER){
		//	DestroyCrater ();
		//}
	}
    public override void OccupyLandmark(Faction faction) {
        base.OccupyLandmark(faction);
        faction.AddSettlement(this);
    }
    public override void UnoccupyLandmark() {
        base.UnoccupyLandmark();
        _owner.RemoveSettlement(this);
    }
    #endregion

    #region Civilians
    public void SetCivilianCount(int count) {
        _civilianCount = count;
    }
    #endregion

    #region Characters
    /*
     Make a character consider this landmark as it's home.
         */
    public override void AddCharacterHomeOnLandmark(ECS.Character character) {
        if (!_charactersWithHomeOnLandmark.Contains(character)) {
            _charactersWithHomeOnLandmark.Add(character);
            ////Add new relationship with head of the settlement
            //if(_headOfSettlement != null) {
            //    if (character.GetRelationshipWith(_headOfSettlement) == null) {
            //        //Village Elders will have relationship with the characters within their village.
            //        CharacterManager.Instance.CreateNewRelationshipBetween(_headOfSettlement, character);
            //    }
            //}
        }
    }
    public List<ECS.Character> GetCharactersCreatedInSettlement(CHARACTER_ROLE charRole) {
        List<ECS.Character> characters = new List<ECS.Character>();
        for (int i = 0; i < _charactersWithHomeOnLandmark.Count; i++) {
            ECS.Character currChar = _charactersWithHomeOnLandmark[i];
            if(currChar.role.roleType == charRole) {
                characters.Add(currChar);
            }
        }
        return characters;
    }
    #endregion

    #region Materials
    public MATERIAL GetMaterialFor(PRODUCTION_TYPE productionType) {
        List<MATERIAL> preferredMats = _owner.productionPreferences[productionType].prioritizedMaterials;
        List<MATERIAL> materialReference;
        switch (productionType) {
            case PRODUCTION_TYPE.WEAPON:
                materialReference = ProductionManager.Instance.weaponMaterials;
                break;
            case PRODUCTION_TYPE.ARMOR:
                materialReference = ProductionManager.Instance.armorMaterials;
                break;
            case PRODUCTION_TYPE.CONSTRUCTION:
                materialReference = ProductionManager.Instance.constructionMaterials;
                break;
            case PRODUCTION_TYPE.TRAINING:
                materialReference = ProductionManager.Instance.trainingMaterials;
                break;
            default:
                throw new System.Exception("No material reference for " + productionType.ToString());
        }
        for (int i = 0; i < preferredMats.Count; i++) {
            MATERIAL currMat = preferredMats[i];
            if (materialReference.Contains(currMat)) {
                //if (HasAccessToMaterial(currMat)) { //Do not check if the settlement has access to the material for now
                    return currMat;
                //}
            }
        }
        return MATERIAL.NONE;
    }
    #endregion

	#region Crater
	private void EmitPsytoxin(){
		Region currRegion = this.tileLocation.region;
		for (int i = 0; i < currRegion.adjacentRegions.Count; i++) {
			Region adjacentRegion = currRegion.adjacentRegions [i];
			for (int j = 0; j < adjacentRegion.landmarks.Count; j++) {
				if(adjacentRegion.landmarks[j].charactersAtLocation.Count > 0){
					for (int k = 0; k < adjacentRegion.landmarks[j].charactersAtLocation.Count; k++) {
                        Character combatInitializer = adjacentRegion.landmarks[j].charactersAtLocation[k];
                        combatInitializer.AssignTag(CHARACTER_TAG.MILD_PSYTOXIN);
      //                  if (combatInitializer is Party){
						//	Party party = (Party)combatInitializer;
						//	for (int l = 0; l < party.partyMembers.Count; l++) {
						//		party.partyMembers [l].AssignTag (CHARACTER_TAG.MILD_PSYTOXIN);
						//	}
						//}else if(combatInitializer is ECS.Character){
						//	ECS.Character character = (ECS.Character)combatInitializer;
						//	character.AssignTag (CHARACTER_TAG.MILD_PSYTOXIN);
						//}
					}
				}
			}
		}
	}
	public void AdjustNumOfPsytoxinated(int amount){
		_numOfPsytoxinated += amount;
		if(_numOfPsytoxinated <= 0){
			_numOfPsytoxinated = 0;
			DestroyLandmark (false);
		}
	}
	public void SetNumOfPsytoxinated(int amount){
		_numOfPsytoxinated = amount;
	}
	private void DestroyCrater(){
		Messenger.RemoveListener ("Psytoxinated", ListenPsytoxinated);
		Messenger.RemoveListener ("Unpsytoxinated", ListenUnpsytoxinated);
		LandmarkManager.Instance.craterLandmark = null;
		//ChangeLandmarkType (LANDMARK_TYPE.TOWN);
		Initialize ();
	}
	public void ListenPsytoxinated(){
		AdjustNumOfPsytoxinated (1);
	}
	public void ListenUnpsytoxinated(){
		AdjustNumOfPsytoxinated (-1);
	}
    #endregion
}
