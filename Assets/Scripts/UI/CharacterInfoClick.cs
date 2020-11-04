using UnityEngine;
using System.Collections;

public class CharacterInfoClick : MonoBehaviour {
	bool isHovering = false;
	UILabel lbl;

	void Start(){
		lbl = GetComponent<UILabel> ();
	}
	//void Update(){
	//	if(isHovering){
	//		string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
	//		if (!string.IsNullOrEmpty (url)) {
	//			if(url == "civilians"){
	//				if(UIManager.Instance.characterInfoUI.currentlyShowingCharacter.civiliansByRace != null){
	//					string hoverText = string.Empty;
	//					foreach (RACE race in UIManager.Instance.characterInfoUI.currentlyShowingCharacter.civiliansByRace.Keys) {
	//						if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter.civiliansByRace[race] > 0){
	//							hoverText += "[b]" + race.ToString() + "[/b] - " + UIManager.Instance.characterInfoUI.currentlyShowingCharacter.civiliansByRace[race].ToString() + "\n";
	//						}
	//					}
	//					hoverText.TrimEnd ('\n');
	//					UIManager.Instance.ShowSmallInfo (hoverText);
	//					return;
	//				}

	//			}
	//		}

	//		if(UIManager.Instance.smallInfoGO.activeSelf){
	//			UIManager.Instance.HideSmallInfo ();
	//		}
	//	}
	//}
	void OnClick(){
		string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
		if (!string.IsNullOrEmpty (url)) {
			if(!url.Contains("_")){
				return;
			}
			string id = url.Substring (0, url.IndexOf ('_'));
			int idToUse = int.Parse (id);
			//Debug.Log("Clicked " + url);
			if(url.Contains("_faction")){
				Faction faction = UIManager.Instance.characterInfoUI.currentlyShowingCharacter.faction;
				if(faction != null){
					UIManager.Instance.ShowFactionInfo (faction);
				}
			} else if(url.Contains("_landmark")){
				if(UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.home.id == idToUse){
					UIManager.Instance.ShowLandmarkInfo (UIManager.Instance.characterInfoUI.currentlyShowingCharacter.home);
				}
            }
   //         else if (url.Contains("_quest")) {
   //             OldQuest.Quest quest = FactionManager.Instance.GetQuestByID(idToUse);
   //             if (quest != null) {
   //                 UIManager.Instance.ShowQuestInfo(quest);
   //             }
			//}
            else if (url.Contains("_party")) {
				Party party = UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party;
				if(party != null){
					UIManager.Instance.ShowCharacterInfo(party.partyLeader);
				}
			} else if (url.Contains("_character")) {
                ECS.Character character = UIManager.Instance.characterInfoUI.currentlyShowingCharacter.GetFollowerByID(idToUse);
                if (character != null) {
                    UIManager.Instance.ShowCharacterInfo(character);
                }
            }
        }
	}
	void OnHover(bool isOver){
		isHovering = isOver;
		if(!isOver){
			UIManager.Instance.HideSmallInfo ();
		}
	}
}
