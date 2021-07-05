using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IElementalTrait {
    bool isPlayerSource { get; }

    void SetIsPlayerSource(bool p_state);
}
