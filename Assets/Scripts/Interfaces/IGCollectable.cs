using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGCollectable {
    bool isDeadReference { get; }

    void SetIsDeadReference(bool p_state);
}
