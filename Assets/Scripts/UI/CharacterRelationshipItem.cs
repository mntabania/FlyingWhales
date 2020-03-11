using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterRelationshipItem : PooledObject {

    [SerializeField] private CharacterPortrait portrait;

    //private AlterEgoData alterEgo;
    //private IRelationshipData data;
    private Character target;
    private Character owner;

    public void Initialize(Character owner, Character target) {
        this.owner = owner;
        this.target = target;
        //this.alterEgo = alterEgo;
        //this.data = data;

        portrait.GeneratePortrait(target, false);
        //portrait.SetSize(70f);
    }

    public override void Reset() {
        base.Reset();
    }

    public void ShowRelationshipData() {
        //string summary = $"Relationships with {alterEgo.owner.name} ({alterEgo.name})";
        //summary += $"\nStatus: {data.relationshipStatus.ToString()}";
        //for (int i = 0; i < data.relationships.Count; i++) {
        //    summary += $"\n- {data.relationships[i].ToString()}";
        //}
        int opinionOfOther = target.relationshipContainer.GetTotalOpinion(owner);
        string summary = target.name;
        summary += "\n---------------------";
        Dictionary<string, int> opinions = owner.relationshipContainer.GetOpinionData(target).allOpinions;
        foreach (KeyValuePair<string, int> kvp in opinions) {
            summary += $"\n{kvp.Key}: <color={OpinionColor(kvp.Value)}>{GetOpinionText(kvp.Value)}</color>";
        }
        summary += "\n---------------------";
        summary +=
            $"\nTotal: <color={OpinionColor(opinionOfOther)}>{GetOpinionText(owner.relationshipContainer.GetTotalOpinion(target))}</color> <color={OpinionColor(opinionOfOther)}>({GetOpinionText(opinionOfOther)})</color>";
        UIManager.Instance.ShowSmallInfo(summary);
    }
    public void HideSmallInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    private string OpinionColor(int number) {
        if(number < 0) {
            return "red";
        }
        return "green";
    }
    private string GetOpinionText(int number) {
        if (number < 0) {
            return $"{number}";
        }
        return $"+{number}";
    }
}
