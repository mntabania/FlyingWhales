﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class RestAction : TaskAction {

    List<ECS.Character> charactersToRest;

    public RestAction(CharacterTask task) : base(task) {}

    #region overrides
    public override void InitializeAction(ECS.Character target) {
        base.InitializeAction(target);
        charactersToRest = new List<ECS.Character>();
        if (target.party != null) {
            charactersToRest.AddRange(target.party.partyMembers);
        } else {
            charactersToRest.Add(target);
        }
    }
    public override void ActionDone(TASK_ACTION_RESULT result) {
        Messenger.RemoveListener(Signals.DAY_END, Rest);
        base.ActionDone(result);
    }
    #endregion

    public void RestIndefinitely() {
        for (int i = 0; i < charactersToRest.Count; i++) {
            ECS.Character currCharacter = charactersToRest[i];
            currCharacter.AdjustHP(currCharacter.raceSetting.restRegenAmount);
        }
    }
    public void Rest() {
        for (int i = 0; i < charactersToRest.Count; i++) {
            ECS.Character currCharacter = charactersToRest[i];
            currCharacter.AdjustHP(currCharacter.raceSetting.restRegenAmount);
        }
        CheckIfCharactersAreFullyRested(charactersToRest);
    }

    private void CheckIfCharactersAreFullyRested(List<ECS.Character> charactersToRest) {
        bool allCharactersRested = true;
        for (int i = 0; i < charactersToRest.Count; i++) {
            ECS.Character currCharacter = charactersToRest[i];
            if (!currCharacter.IsHealthFull()) {
                allCharactersRested = false;
                break;
            }
        }
        if (allCharactersRested) {
            ActionDone(TASK_ACTION_RESULT.SUCCESS);
        }
    }
}
