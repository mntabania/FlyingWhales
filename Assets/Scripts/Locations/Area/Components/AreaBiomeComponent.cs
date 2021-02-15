using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaBiomeComponent : AreaComponent {

    #region Utilities
    public void SetBiome(BIOMES biome) {
        owner.biomeEffectTrigger.ProcessBeforeBiomeChange();
        owner.areaData.biomeType = biome;
        owner.biomeEffectTrigger.ProcessAfterBiomeChange();
    }
    public void ChangeBiomeType(BIOMES biomeType) {
        SetBiome(biomeType);
        //Biomes.Instance.UpdateTileVisuals(this);
        owner.gridTileComponent.ChangeGridTilesBiome();
    }
    public void GradualChangeBiomeType(BIOMES biomeType, System.Action onFinishChangeAction) {
        SetBiome(biomeType);
        //Biomes.Instance.UpdateTileVisuals(this);
        GameManager.Instance.StartCoroutine(owner.gridTileComponent.ChangeGridTilesBiomeCoroutine(onFinishChangeAction));
    }
    #endregion
}
