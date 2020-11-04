using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerActionsUI : MonoBehaviour {
	public static PlayerActionsUI Instance;

	public GameObject characterTaskButtonGO;
	public GameObject specificCharacterTaskButtonGO;
	public GameObject bottomSpriteGO;

	public GameObject specificTargetsGO;
	public UIGrid buttonsGrid;
	public UI2DSprite stretchableBG;

	public UIGrid specificButtonsGrid;
	public UI2DSprite specificStretchableBG;

	public ILocation location;
	private List<CharacterTaskButton> taskButtons = new List<CharacterTaskButton>();
	private List<SpecificCharacterTaskButton> specificTaskButtons = new List<SpecificCharacterTaskButton>();

//	private UIWidget buttonsGridWidget;
	private int defaultWidgetHeight;
	private int characterTaskButtonHeight;
	private int numOfShowingButtons;
	private int specificNumOfShowingButtons;

	void Awake(){
//		buttonsGridWidget = buttonsGrid.GetComponent<UIWidget>();
		Instance = this;
		characterTaskButtonHeight = characterTaskButtonGO.GetComponent<UI2DSprite>().height;
		defaultWidgetHeight = buttonsGrid.GetComponent<UIWidget>().height;
	}
	public void ShowPlayerActionsUI(ILocation location){
		this.location = location;
		specificTargetsGO.SetActive (false);
		OnShow ();
	}
	public void HidePlayerActionsUI(){
		this.gameObject.SetActive (false);
	}
	public void Reposition(){
		gameObject.transform.position = Vector3.zero;
		Vector3 newPosX = location.tileLocation.transform.position;
		Vector3 screenPosX = Camera.main.WorldToScreenPoint (newPosX);
		if(screenPosX.x < 120f){
			screenPosX.x = 120f;
			newPosX = Camera.main.ScreenToWorldPoint (screenPosX);
		}else if(screenPosX.x > 1390f){
			screenPosX.x = 1390f;
			newPosX = Camera.main.ScreenToWorldPoint (screenPosX);
		}
		gameObject.transform.position = newPosX;

//		Vector3 newPosY = bottomSpriteGO.transform.position;
		Vector3 screenPosY = Camera.main.WorldToScreenPoint (bottomSpriteGO.transform.position);
		if(screenPosY.y < 5f){
			double additionalPosY = System.Math.Round((double)((((float)(stretchableBG.height - characterTaskButtonHeight)) / 4f) * 0.1f), 1);
			newPosX.y += (1f + (float)additionalPosY);
//			gameObject.transform.position = new Vector3 (newPosX.x, newPosY.y, newPosX.z);
		}
		gameObject.transform.position = newPosX;

	}

	private void OnShow(){
		ECS.Character character = UIManager.Instance.characterInfoUI.activeCharacter;
		if(character == null){
			return;
		}
		numOfShowingButtons = 0;
		List<CharacterTask> tasksAvailable = character.GetAllPossibleTasks (this.location);
		if(tasksAvailable.Count > 0){
			if(tasksAvailable.Count > taskButtons.Count){
				for (int i = 0; i < tasksAvailable.Count; i++) {
					if(i < taskButtons.Count){
						taskButtons [i].SetTask (tasksAvailable [i]);
//						taskButtons [i].SetLocation (this.location);
						taskButtons [i].gameObject.SetActive (true);
						numOfShowingButtons++;
					}else{
						CreateButton (tasksAvailable [i]);
						numOfShowingButtons++;
					}
				}
			}else{
				for (int i = 0; i < taskButtons.Count; i++) {
					if(i < tasksAvailable.Count){
						taskButtons [i].SetTask (tasksAvailable [i]);
//						taskButtons [i].SetLocation (this.location);
						taskButtons [i].gameObject.SetActive (true);
						numOfShowingButtons++;
					}else{
						taskButtons [i].gameObject.SetActive (false);
					}
				}
			}
			this.gameObject.SetActive (true);
			buttonsGrid.Reposition ();
			UpdateGrid();
		}else{
			this.gameObject.SetActive (false);
		}
	}

	public void ShowSpecificTargets(CharacterTask task){
        ECS.Character character = UIManager.Instance.characterInfoUI.activeCharacter;
        specificNumOfShowingButtons = 0;
		if(task.specificTargetClassification == "character"){
			List<ECS.Character> characters = CharacterTargets (character, task);
			if(characters.Count > 0){
				if(characters.Count > specificTaskButtons.Count){
					for (int i = 0; i < characters.Count; i++) {
						if(i < specificTaskButtons.Count){
							specificTaskButtons [i].SetTask (task);
							specificTaskButtons [i].SetTarget (characters [i]);
							specificTaskButtons [i].gameObject.SetActive (true);
							specificNumOfShowingButtons++;
						}else{
							CreateSpecificButton (task, characters [i]);
							specificNumOfShowingButtons++;
						}
					}
				}else{
					for (int i = 0; i < specificTaskButtons.Count; i++) {
						if(i < characters.Count){
							specificTaskButtons [i].SetTask (task);
							specificTaskButtons [i].SetTarget (characters [i]);
							specificTaskButtons [i].gameObject.SetActive (true);
							specificNumOfShowingButtons++;
						}else{
							specificTaskButtons [i].gameObject.SetActive (false);
						}
					}
				}
				specificTargetsGO.SetActive (true);
				specificButtonsGrid.Reposition ();
				UpdateSpecificGrid();
			}
		} else if(task.specificTargetClassification == "item"){
			//TODO: Item Targets
		} else if (task.specificTargetClassification == "quest") {
            //TODO: Quest Targets
            List<Quest> availableQuests = QuestManager.Instance.GetAvailableQuestsForCharacter(character);
            if (availableQuests.Count > 0) {
                if (availableQuests.Count > specificTaskButtons.Count) {
                    for (int i = 0; i < availableQuests.Count; i++) {
                        if (i < specificTaskButtons.Count) {
                            specificTaskButtons[i].SetTask(task);
                            specificTaskButtons[i].SetTarget(availableQuests[i]);
                            specificTaskButtons[i].gameObject.SetActive(true);
                            specificNumOfShowingButtons++;
                        } else {
                            CreateSpecificButton(task, availableQuests[i]);
                            specificNumOfShowingButtons++;
                        }
                    }
                } else {
                    for (int i = 0; i < specificTaskButtons.Count; i++) {
                        if (i < availableQuests.Count) {
                            specificTaskButtons[i].SetTask(task);
                            specificTaskButtons[i].SetTarget(availableQuests[i]);
                            specificTaskButtons[i].gameObject.SetActive(true);
                            specificNumOfShowingButtons++;
                        } else {
                            specificTaskButtons[i].gameObject.SetActive(false);
                        }
                    }
                }
                specificTargetsGO.SetActive(true);
                specificButtonsGrid.Reposition();
                UpdateSpecificGrid();
            }
        }
    }

	private List<ECS.Character> CharacterTargets(ECS.Character assignedCharacter, CharacterTask task){
		List<ECS.Character> characters = new List<ECS.Character> ();
		BaseLandmark landmark = this.location as BaseLandmark;
		for (int i = 0; i < landmark.charactersAtLocation.Count; i++) {
			ECS.Character character = landmark.charactersAtLocation [i];
			if(!task.canTargetSelf && character.id == assignedCharacter.id){
				continue;
			}
			if(task.CanMeetRequirements(character, assignedCharacter)){
				characters.Add (character);
			}
		}
		return characters;
	}

    private void CreateButton(CharacterTask task){
		GameObject characterTaskButton = GameObject.Instantiate (characterTaskButtonGO, buttonsGrid.transform) as GameObject;
		characterTaskButton.transform.localScale = Vector3.one;
		characterTaskButton.transform.localPosition = Vector3.zero;

		CharacterTaskButton taskButton = characterTaskButton.GetComponent<CharacterTaskButton> ();
		taskButton.SetTask (task);
//		taskButton.SetLocation (this.location);
		taskButtons.Add (taskButton);
	}

	private void CreateSpecificButton(CharacterTask task, object target){
		GameObject specificCharacterTaskButton = (GameObject)GameObject.Instantiate (specificCharacterTaskButtonGO, specificButtonsGrid.transform);
		specificCharacterTaskButton.transform.localScale = Vector3.one;
		specificCharacterTaskButton.transform.localPosition = Vector3.zero;

		SpecificCharacterTaskButton taskButton = specificCharacterTaskButton.GetComponent<SpecificCharacterTaskButton> ();
		taskButton.SetTask (task);
		taskButton.SetTarget (target);
		specificTaskButtons.Add (taskButton);
	}

	[ContextMenu("Update Grid")]
	public void UpdateGrid(){
		if(numOfShowingButtons > 0){
			stretchableBG.height = numOfShowingButtons * characterTaskButtonHeight;
		}else{
			stretchableBG.height = defaultWidgetHeight;
		}
	}

	private void UpdateSpecificGrid(){
		if(specificNumOfShowingButtons > 0){
			specificStretchableBG.height = specificNumOfShowingButtons * characterTaskButtonHeight;
		}else{
			specificStretchableBG.height = defaultWidgetHeight;
		}
	}

	#region For Testing
	[ContextMenu("Show Screen Pos")]
	public void ScreenPos(){
		Debug.Log (Camera.main.WorldToScreenPoint (gameObject.transform.position));
	}
	[ContextMenu("Show Bottom Screen Pos")]
	public void BotScreenPos(){
		Debug.Log (bottomSpriteGO.transform.position);
		Debug.Log (Camera.main.WorldToScreenPoint (bottomSpriteGO.transform.position));
	}
	#endregion
}
