using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionIdeologyComponent {
    public Faction owner { get; private set; }
    public List<FactionIdeology> currentIdeologies => owner.factionType.ideologies;

    public FactionIdeologyComponent(Faction owner) {
        this.owner = owner;
    }

    //public void SwitchToIdeology(FACTION_IDEOLOGY ideologyType) {
    //    if(currentIdeologies != null && currentIdeologies.ideologyType == FACTION_IDEOLOGY.INCLUSIVE && ideologyType == FACTION_IDEOLOGY.INCLUSIVE) { return; }
    //    currentIdeologies = CreateIdeology(ideologyType);
    //    currentIdeologies.SetRequirements(owner);
    //    ReEvaluateFactionMembers();
    //}
    // public void RerollIdeologies(bool willLog = true) {
    //     FACTION_IDEOLOGY[][] categorizedIdeologies = FactionManager.Instance.categorizedFactionIdeologies;
    //     for (int i = 0; i < currentIdeologies.Length; i++) {
    //         FactionIdeology ideology = currentIdeologies[i];
    //         FACTION_IDEOLOGY categorizedIdeology =
    //             categorizedIdeologies[i][Random.Range(0, categorizedIdeologies[i].Length)];
    //         ideology = FactionManager.Instance.CreateIdeology<FactionIdeology>(categorizedIdeology);
    //         ideology.SetRequirements(owner);
    //         currentIdeologies[i] = ideology;
    //     }
    //     ReEvaluateFactionMembers(willLog);
    // }
    // public void SetCurrentIdeology(int index, FactionIdeology ideology) {
    //     currentIdeologies[index] = ideology;
    // }
    public bool DoesCharacterFitCurrentIdeologies(Character character) {
        if(currentIdeologies == null) { return true; }
        for (int i = 0; i < currentIdeologies.Count; i++) {
            FactionIdeology ideology = currentIdeologies[i]; ;
            if(ideology != null && !ideology.DoesCharacterFitIdeology(character)) {
                return false;
            }
        }
        return true;
    }

    // private void ReEvaluateFactionMembers(bool willLog = true) {
    //     for (int i = 0; i < owner.characters.Count; i++) {
    //         Character member = owner.characters[i];
    //         if(member == owner.leader) { continue; }
    //         if (owner.CheckIfCharacterStillFitsIdeology(member, willLog)) {
    //             i--;
    //         }
    //     }
    // }
}