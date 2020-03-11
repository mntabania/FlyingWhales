using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FactionRelationshipItem : MonoBehaviour {
    [SerializeField] private FactionEmblem emblem;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI statusLbl;

    public void SetData(Faction faction, FactionRelationship rel) {
        emblem.SetFaction(faction);
        nameLbl.text = faction.name;
        statusLbl.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(rel.relationshipStatus.ToString());
    }
}
