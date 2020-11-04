using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class ExploreState : State {
	public ExploreState(CharacterTask parentTask): base (parentTask, STATE.EXPLORE){
		
	}

	#region Overrides
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }
		ExploreLandmark ();
		return true;
	}
	#endregion

	/*
     What should happen when this landmark is explored?
         */
	private void ExploreLandmark() {
		//default behaviour is a random item will be given to the explorer based on the landmarks item weights
		ECS.Item generatedItem = GenerateRandomItem();
		if (generatedItem != null) {
			if (generatedItem.isObtainable) {
                //Remove item form weights if it is not unlimited
                _targetLandmark.RemoveItemInLandmark(generatedItem);

                if (!_assignedCharacter.EquipItem(generatedItem)) {
					_assignedCharacter.PickupItem(generatedItem);
				}
            } else {
                //item should only be interacted with
                Log interactLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "interact_item");
                interactLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                interactLog.AddToFillers(null, generatedItem.interactString, LOG_IDENTIFIER.OTHER);
                interactLog.AddToFillers(null, generatedItem.itemName, LOG_IDENTIFIER.ITEM_1);
                _targetLandmark.AddHistory(interactLog);
                _assignedCharacter.AddHistory(interactLog);
                
                StorylineManager.Instance.OnInteractWith(generatedItem, _targetLandmark, _assignedCharacter);
			}
		}

        //if the character has a faction, add landmark info to it's faction
        if (!_assignedCharacter.isFactionless) {
            _assignedCharacter.faction.AddLandmarkInfo(parentTask.targetLocation as BaseLandmark);
        }
	}

	/*
     Generate a random item, given the data of this landmark type
         */
	public ECS.Item GenerateRandomItem() {
		//WeightedDictionary<ECS.Item> itemWeights = GetExploreItemWeights();
		//if (itemWeights.GetTotalOfWeights() > 0) {
		//	ECS.Item chosenItem = itemWeights.PickRandomElementGivenWeights();
		//	return chosenItem;
		//}
		return null;
	}

	//private WeightedDictionary<ECS.Item> GetExploreItemWeights() {
	//	WeightedDictionary<ECS.Item> itemWeights = new WeightedDictionary<ECS.Item>();
	//	for (int i = 0; i < _targetLandmark.itemsInLandmark.Count; i++) {
	//		ECS.Item currItem =  _targetLandmark.itemsInLandmark[i];
	//		itemWeights.AddElement(currItem, currItem.exploreWeight);
	//	}
	//	return itemWeights;
	//}
}
