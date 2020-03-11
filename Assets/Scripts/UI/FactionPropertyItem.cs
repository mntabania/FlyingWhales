using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FactionPropertyItem : MonoBehaviour {

    //private NPCSettlement npcSettlement;

    [SerializeField] private TextMeshProUGUI areaNameLbl;

    public void SetArea(NPCSettlement npcSettlement) {
        //this.npcSettlement = npcSettlement;
        areaNameLbl.text = npcSettlement.name;
    }
}
