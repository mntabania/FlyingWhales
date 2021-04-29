using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTileObjectComponent {
    public List<DemonEye> spawnedEyeWards { get; private set; }

    public PlayerTileObjectComponent() {
        spawnedEyeWards = new List<DemonEye>();
    }
    public PlayerTileObjectComponent(SaveDataPlayerTileObjectComponent data) {
        spawnedEyeWards = new List<DemonEye>();
    }

    #region Utilities
    public void AddEyeWard(DemonEye p_eyeWard) {
        if (!spawnedEyeWards.Contains(p_eyeWard)) {
            spawnedEyeWards.Add(p_eyeWard);
        }
    }
    public bool RemoveEyeWard(DemonEye p_eyeWard) {
        return spawnedEyeWards.Remove(p_eyeWard);
    }
    public void ShowAllEyeWardHighlights() {
        for (int i = 0; i < spawnedEyeWards.Count; i++) {
            spawnedEyeWards[i].ShowEyeWardHighlight();
        }
    }
    public void HideAllEyeWardHighlights() {
        for (int i = 0; i < spawnedEyeWards.Count; i++) {
            spawnedEyeWards[i].HideEyeWardHighlight();
        }
    }
    #endregion
}

public class SaveDataPlayerTileObjectComponent : SaveData<PlayerTileObjectComponent> {
    public override PlayerTileObjectComponent Load() {
        PlayerTileObjectComponent component = new PlayerTileObjectComponent(this);
        return component;
    }
}