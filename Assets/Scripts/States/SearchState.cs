using UnityEngine;
using System.Collections;
using System;

public class SearchState : State {

    private object searchingFor;
    private Action _afterFindingAction;
	//private Action _afterFindingTraceAction;
    private Action _searchAction;
	private bool _isSearchingUnique;

    public SearchState(CharacterTask parentTask, object searchingFor) : base(parentTask, STATE.SEARCH) {
        this.searchingFor = searchingFor;
		this._isSearchingUnique = false;
        if (searchingFor is string) {
            if ((searchingFor as string).Equals("Heirloom Necklace")) {
				this._isSearchingUnique = true;
				SetSearchAction(() => SearchForItemInCharacter());
				if(_parentTask.parentQuest != null && _parentTask.parentQuest is EliminateLostHeir){
					SetAfterFindingTraceAction (() => _parentTask.EndTaskSuccess ());
				}
            } else if ((searchingFor as string).Equals("Book of Inimical Incantations")) {
				SetSearchAction(() => SearchForItemInLandmark());
            } else if ((searchingFor as string).Equals("Neuroctus")) {
				SetSearchAction(() => SearchForItemInLandmark());
            } else if ((searchingFor as string).Equals("Herbalist")) {
				SetSearchAction(() => SearchForATag());
				if(_parentTask.parentQuest != null){
					if(_parentTask.parentQuest is PsytoxinCure){
						SetAfterFindingAction(() => CurePsytoxin());
					}
				}
            }
        }
    }

    #region Overrides
    public override bool PerformStateAction() {
        if (!base.PerformStateAction()) { return false; }
        if (_searchAction != null) {
            _searchAction();
        } else {
            throw new Exception("NO SEARCH ACTION!");
        }
        return true;
    }
    #endregion

	#region Utilities
	public void SetAfterFindingAction(Action action){
		_afterFindingAction = action;
	}
	public void SetAfterFindingTraceAction(Action action){
		//_afterFindingTraceAction = action;
	}
	public void SetSearchAction(Action action){
		_searchAction = action;
	}	
	#endregion

    #region Search for Item in Character
    private void SearchForItemInCharacter() {
		bool hasBeenFound = false;
		string itemName = (string)searchingFor;
        for (int i = 0; i < _targetLandmark.charactersAtLocation.Count; i++) {
			ECS.Character currCharacter = _targetLandmark.charactersAtLocation[i];
			if (currCharacter.HasItem(itemName)) {
				hasBeenFound = true;
                //Each day while he is in Search State, if the character with the Heirloom Necklace is in the location then he would successfully perform the action and end the Search State.
                if (_afterFindingAction != null) {
                    _afterFindingAction();
                }
                parentTask.EndTask(TASK_STATUS.SUCCESS);
                break;
                //_assignedCharacter.questData.AdvanceToNextPhase();
            }
        }
		if(!hasBeenFound){
			CheckTraces (itemName, "item");
        } else {
            Messenger.Broadcast(Signals.FOUND_ITEM, _assignedCharacter, itemName);
        }
    }
    #endregion

	#region Search for a Tag
	private void SearchForATag() {
		bool hasBeenFound = false;
		string tagName = (string)searchingFor;
		for (int i = 0; i < _targetLandmark.charactersAtLocation.Count; i++) {
			ECS.Character currCharacter = _targetLandmark.charactersAtLocation[i];
			if (currCharacter.HasTag(tagName, true)) {
				hasBeenFound = true;
				if (_afterFindingAction != null) {
					_afterFindingAction();
				}
				parentTask.EndTask(TASK_STATUS.SUCCESS);
				break;
			}
		}
		if(!hasBeenFound){
			CheckTraces (tagName, "tag");
		}
    }
	#endregion

    #region Search for Landmark Items
    private void SearchForItemInLandmark() {
		bool hasBeenFound = false;
		string itemName = searchingFor as string;
		//for (int i = 0; i < _targetLandmark.itemsInLandmark.Count; i++) {
		//	ECS.Item item = _targetLandmark.itemsInLandmark[i];
		//	if (item.itemName == itemName) {
		//		int chance = UnityEngine.Random.Range(0, 100);
		//		if (chance < item.collectChance) {
		//			//_assignedCharacter.AddHistory ("Found a " + (string)_searchingFor + "!");
		//			//_targetLandmark.AddHistory (_assignedCharacter.name +  " found a " + (string)_searchingFor + "!");
		//			hasBeenFound = true;
		//			_targetLandmark.RemoveItemInLandmark(item);
		//			_assignedCharacter.PickupItem(item);
		//			if (_afterFindingAction != null) {
		//				_afterFindingAction();
		//			}
		//			parentTask.EndTask(TASK_STATUS.SUCCESS);
		//			break;
		//		}
		//	}
		//}
		if(!hasBeenFound){
			CheckTraces (itemName, "item");
		} else {
			Messenger.Broadcast(Signals.FOUND_ITEM, _assignedCharacter, itemName);
		}
    }
    #endregion

    #region Psytoxin Herbalist
    private void CurePsytoxin() {
        ECS.Item meteorite = _assignedCharacter.GetItemInInventory("Meteorite");
        ECS.Item neuroctus = _assignedCharacter.GetItemInInventory("Neuroctus");
        if (meteorite != null && neuroctus != null) {
            _assignedCharacter.ThrowItem(meteorite, false);
            _assignedCharacter.ThrowItem(neuroctus, false);
            string psytoxinLevel = string.Empty;
            if (!_assignedCharacter.RemoveCharacterTag(CHARACTER_TAG.MILD_PSYTOXIN)) {
                if (!_assignedCharacter.RemoveCharacterTag(CHARACTER_TAG.MODERATE_PSYTOXIN)) {
                    _assignedCharacter.RemoveCharacterTag(CHARACTER_TAG.SEVERE_PSYTOXIN);
                    psytoxinLevel = "Severe";
                } else {
                    psytoxinLevel = "Moderate";
                }
            } else {
                psytoxinLevel = "Mild";
            }
            Log cureLog = new Log(GameManager.Instance.Today(), "CharacterTags", "Psytoxin", "cured");
            cureLog.AddToFillers(null, psytoxinLevel, LOG_IDENTIFIER.OTHER);
        }
    }
    #endregion

	#region Traces
	private void CheckTraces(string strSearchingFor, string identifier, bool includeParty = false){
		if (identifier == "tag") {
			foreach (ECS.Character currCharacter in _targetLandmark.characterTraces.Keys) {
				if (currCharacter.HasTag (strSearchingFor, includeParty)) {
					_assignedCharacter.AddTraceInfo (currCharacter, strSearchingFor, this._isSearchingUnique);
                    Messenger.Broadcast(Signals.FOUND_TRACE, _assignedCharacter, strSearchingFor);
					//if(_afterFindingTraceAction != null){
					//	_afterFindingTraceAction ();
					//}
					break;
				}
			}
		} else if (identifier == "item") {
			foreach (ECS.Character currCharacter in _targetLandmark.characterTraces.Keys) {
				if (currCharacter.HasItem (strSearchingFor)) {
					_assignedCharacter.AddTraceInfo (currCharacter, strSearchingFor, this._isSearchingUnique);
                    Messenger.Broadcast(Signals.FOUND_TRACE, _assignedCharacter, strSearchingFor);
                    //if(_afterFindingTraceAction != null){
                    //	_afterFindingTraceAction ();
                    //}
                    break;
				}
			}
		}
	}
	#endregion
}
