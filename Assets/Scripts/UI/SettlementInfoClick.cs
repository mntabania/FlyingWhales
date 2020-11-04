using UnityEngine;
using System.Collections;

public class SettlementInfoClick : MonoBehaviour {
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
	//				string hoverText = string.Empty;
	//				foreach (RACE race in UIManager.Instance.settlementInfoUI.currentlyShowingLandmark.civiliansByRace.Keys) {
	//					if (UIManager.Instance.settlementInfoUI.currentlyShowingLandmark.civiliansByRace[race] > 0){
	//						hoverText += "[b]" + race.ToString() + "[/b] - " + UIManager.Instance.settlementInfoUI.currentlyShowingLandmark.civiliansByRace[race].ToString() + "\n";
	//					}
	//				}
	//				hoverText.TrimEnd ('\n');
	//				UIManager.Instance.ShowSmallInfo (hoverText);
	//				return;
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
				Faction faction = UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark.owner;
				if(faction != null){
					UIManager.Instance.ShowFactionInfo (faction);
				}
			} else if(url.Contains("_character")){
				BaseLandmark landmark = UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark;
				ECS.Character character = landmark.GetCharacterAtLocationByID(idToUse, true);
				if(character != null){
					UIManager.Instance.ShowCharacterInfo(character);
				}else{
					character = landmark.tileLocation.GetCharacterAtLocationByID(idToUse, true);
					if(character != null){
						UIManager.Instance.ShowCharacterInfo(character);
					}
				}
			} else if(url.Contains("_hextile")){
				if(UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark != null && UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark.tileLocation.id == idToUse){
					UIManager.Instance.ShowHexTileInfo (UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark.tileLocation);
				}
            } else if (url.Contains("_quest")) {
				if(UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark is Settlement){
					//OldQuest.Quest quest = ((Settlement)UIManager.Instance.settlementInfoUI.currentlyShowingLandmark).GetQuestByID(idToUse);
					//if (quest != null) {
					//	UIManager.Instance.ShowQuestInfo(quest);
					//}	
				}
			} 
   //         else if (url.Contains("_party")) {
			//	Party party = UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark.GetPartyAtLocationByLeaderID(idToUse);
			//	if (party != null) {
			//		UIManager.Instance.ShowCharacterInfo(party.partyLeader);
			//	} else {
			//		party = UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark.tileLocation.GetPartyAtLocationByLeaderID(idToUse);
			//		if (party != null) {
			//			UIManager.Instance.ShowCharacterInfo(party.partyLeader);
			//		}
			//	}
			//} 
            else if(url.Contains("_prisoner")){
				BaseLandmark landmark = UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark;
				ECS.Character character = landmark.GetPrisonerByID(idToUse);
				if(character != null){
					UIManager.Instance.ShowCharacterInfo(character);
				}
			} 
        }
	}

	//void OnHover(bool isOver){
	//	isHovering = isOver;
	//	if(!isOver){
	//		UIManager.Instance.HideSmallInfo ();
	//	}
	//}

}
