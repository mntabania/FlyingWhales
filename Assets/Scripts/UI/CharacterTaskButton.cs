using UnityEngine;
using System.Collections;

public class CharacterTaskButton : MonoBehaviour {

	public CharacterTask task;
	public UILabel btnLabel;
//	private ILocation location;

	public void SetTask (CharacterTask task){
		this.task = task;
		ChangeButtonText ();
	}
//	public void SetLocation(ILocation location){
//		this.location = location;
//	}

	private void ChangeButtonText(){
		btnLabel.text = Utilities.NormalizeString (task.taskType.ToString ());
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
		if (!task.needsSpecificTarget) {
			//if (character.avatar != null && character.avatar.isMovingToHex) {
			//	character.avatar.SetQueuedAction (() => ClickAction ());
			//	UIManager.Instance.HidePlayerActions ();
			//	return;
			//}
			//if (character.actionData.currentAction != null) {
			//  character.currentAction.SetIsHalted (true);
			//}
		}

		if(!task.needsSpecificTarget){
            Log overrideLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "action_override");
            overrideLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideLog.AddToFillers(null, task.GetLeaveActionString(), LOG_IDENTIFIER.ACTION_DESCRIPTION);
			overrideLog.AddToFillers(PlayerActionsUI.Instance.location, PlayerActionsUI.Instance.location.locationName, LOG_IDENTIFIER.LANDMARK_1);
            character.AddHistory(overrideLog);

			task.ResetTask ();
			task.SetLocation (PlayerActionsUI.Instance.location);
            task.OnChooseTask (character);
			UIManager.Instance.HidePlayerActions ();
		}else{
			UIManager.Instance.playerActionsUI.ShowSpecificTargets (task);
		}
//		task.PerformTask ();
	}
}
