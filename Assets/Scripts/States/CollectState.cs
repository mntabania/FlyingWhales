using UnityEngine;
using System.Collections;

public class CollectState : State {
	private int _quantityAlreadyCollected;
	private string _itemNameToCollect;
	private int _quantityToCollect;

	public CollectState(CharacterTask parentTask, string itemNameToCollect, int quantityToCollect): base (parentTask, STATE.COLLECT){
		_quantityAlreadyCollected = 0;
		_itemNameToCollect = itemNameToCollect;
		_quantityToCollect = quantityToCollect;
	}

	#region Overrides
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }
		CollectItem ();
		return true;
	}
	#endregion

	private void CollectItem(){
        //	int collectedAmount = 0;
        //	int chance = 0;
        //	for (int i = 0; i < _targetLandmark.itemsInLandmark.Count; i++) {
        //		ECS.Item item = _targetLandmark.itemsInLandmark [i];
        //		if (item.itemName == _itemNameToCollect) {
        //			if (item.isUnlimited) {
        //				int alreadyCollected = _quantityAlreadyCollected;
        //				for (int j = alreadyCollected; j < _quantityToCollect; j++) {
        //					chance = UnityEngine.Random.Range (0, 100);
        //					if (chance < item.collectChance) {
        //						_targetLandmark.RemoveItemInLandmark (item);
        //						_assignedCharacter.PickupItem (item);
        //						_quantityAlreadyCollected++;
        //						collectedAmount++;
        //					}
        //				}
        //			} else {
        //				chance = UnityEngine.Random.Range (0, 100);
        //				if (chance < item.collectChance) {
        //					_targetLandmark.RemoveItemInLandmark (item);
        //					_assignedCharacter.PickupItem (item);
        //					_quantityAlreadyCollected++;
        //					collectedAmount++;
        //				}
        //			}
        //		}
        //	}
        //	if (_quantityAlreadyCollected >= _quantityToCollect) {
        //		_parentTask.EndTaskSuccess ();
        //	} else {
        //		_parentTask.EndTaskFail ();
        //	}
    }
}
