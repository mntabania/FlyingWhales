using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionIdeologyComponent {
    public Faction owner { get; private set; }
    public FactionIdeology[] currentIdeologies { get; private set; }

    public FactionIdeologyComponent(Faction owner) {
        this.owner = owner;
        currentIdeologies = new FactionIdeology[FactionManager.Instance.categorizedFactionIdeologies.Length];
    }

    //public void SwitchToIdeology(FACTION_IDEOLOGY ideologyType) {
    //    if(currentIdeologies != null && currentIdeologies.ideologyType == FACTION_IDEOLOGY.INCLUSIVE && ideologyType == FACTION_IDEOLOGY.INCLUSIVE) { return; }
    //    currentIdeologies = CreateIdeology(ideologyType);
    //    currentIdeologies.SetRequirements(owner);
    //    ReEvaluateFactionMembers();
    //}
    public void RerollIdeologies(bool willLog = true) {
        FACTION_IDEOLOGY[][] categorizedIdeologies = FactionManager.Instance.categorizedFactionIdeologies;
        for (int i = 0; i < currentIdeologies.Length; i++) {
            FactionIdeology ideology = currentIdeologies[i];
            ideology = FactionManager.Instance.CreateIdeology(categorizedIdeologies[i][UnityEngine.Random.Range(0, categorizedIdeologies[i].Length)]);
            ideology.SetRequirements(owner);
            currentIdeologies[i] = ideology;
        }
        ReEvaluateFactionMembers(willLog);
    }
    public void SetCurrentIdeology(int index, FactionIdeology ideology) {
        currentIdeologies[index] = ideology;
    }
    public bool DoesCharacterFitCurrentIdeologies(Character character) {
        if(currentIdeologies == null) { return true; }
        for (int i = 0; i < currentIdeologies.Length; i++) {
            FactionIdeology ideology = currentIdeologies[i]; ;
            if(ideology != null && !ideology.DoesCharacterFitIdeology(character)) {
                return false;
            }
        }
        return true;
        //return currentIdeologies.DoesCharacterFitIdeology(character);
    }

    private void ReEvaluateFactionMembers(bool willLog = true) {
        for (int i = 0; i < owner.characters.Count; i++) {
            Character member = owner.characters[i];
            if(member == owner.leader) { continue; }
            if (owner.CheckIfCharacterStillFitsIdeology(member, willLog)) {
                i--;
            }
        }
    }
}