using UnityEngine;
using System.Collections;

public class SpecificCharacterTaskButton : MonoBehaviour {
	public CharacterTask task;
	public object target;
	public UILabel btnLabel;

	public void SetTask (CharacterTask task){
		this.task = task;
	}
	public void SetTarget (object target){
		this.target = target;
		ChangeButtonText ();
	}

	private void ChangeButtonText(){
		string text = string.Empty;
		if(this.target is ECS.Character){
			text = (this.target as ECS.Character).name;
		}else if(this.target is ECS.Item){
			text = (this.target as ECS.Item).itemName;
		} else if (this.target is Quest) {
			text = (this.target as Quest).questName;
        }
        btnLabel.text = text;
	}

	void OnClick(){
		if(UICamera.currentTouchID == -1){
			ClickAction ();
		}
	}

	private void ClickAction(){
		ECS.Character character = UIManager.Instance.characterInfoUI.activeCharacter;
		if(character == null){
			UIManager.Instance.HidePlayerActions ();
			return;
		}
		//if(character.avatar != null && character.avatar.isMovingToHex){
		//	character.avatar.SetQueuedAction (() => ClickAction ());
		//	UIManager.Instance.HidePlayerActions ();
		//	return;
		//}
		//if(character.actionData.currentAction != null){
		//  character.currentAction.SetIsHalted (true);
		//}
		task.ResetTask ();
		task.SetLocation (PlayerActionsUI.Instance.location);
		task.SetSpecificTarget (target);
        Log overrideLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "action_override_specific_target");
        overrideLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        overrideLog.AddToFillers(null, task.GetLeaveActionString(), LOG_IDENTIFIER.ACTION_DESCRIPTION);
        character.AddHistory(overrideLog);

        task.OnChooseTask (character);
		UIManager.Instance.HidePlayerActions ();
//		task.PerformTask ();
	}
}
