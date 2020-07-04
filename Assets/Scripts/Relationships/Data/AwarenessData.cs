using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwarenessData {
    public AWARENESS_STATE state { get; private set; }

    public AwarenessData() {
        SetAwarenessState(AWARENESS_STATE.Available);
    }

    public void SetAwarenessState(AWARENESS_STATE state) {
        this.state = state;
    }
}
