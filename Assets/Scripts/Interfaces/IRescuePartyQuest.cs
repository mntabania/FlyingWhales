using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRescuePartyQuest {
    Character targetCharacter { get; }

    void SetIsReleasing(bool state);
    void SetIsSuccessful(bool state);
    void EndQuest(string reason);
}
