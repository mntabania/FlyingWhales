using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISingletonPattern {
    void Initialize();
    void AddCleanupListener();
    void CleanUpAndRemoveCleanUpListener();
}
