using UnityEngine;
using System.Collections;

namespace ECS{
	public class CharacterListOnClick : MonoBehaviour {

		void OnClick(){
			UILabel lbl = GetComponent<UILabel> ();
			string url = lbl.GetUrlAtPosition (UICamera.lastWorldPosition);
			if (!string.IsNullOrEmpty (url)) {
				string id = url.Substring (0, url.IndexOf ('_'));
				int idToUse = int.Parse (id);
                //Debug.Log("Clicked " + url);
				//if(url.Contains("_sideA")){
    //                CombatPrototypeUI.Instance.SetCharacterAsSelected(CombatManager.Instance.combat.charactersSideA[idToUse]);
				//}else if(url.Contains("_sideB")){
				//	CombatPrototypeUI.Instance.SetCharacterAsSelected(CombatManager.Instance.combat.charactersSideB[idToUse]);
    //            }
			}
		}
	}
}

