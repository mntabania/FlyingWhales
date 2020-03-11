using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Traits;
using Inner_Maps;

public class LightningParticleEffect : BaseParticleEffect {

    //protected override void ParticleAfterEffect(ParticleSystem particleSystem) {
    //    OnLightningStrike();
    //}
    private void OnLightningStrike() {
        //List<IPointOfInterest> pois = targetTile.GetPOIsOnTile();
        //for (int i = 0; i < pois.Count; i++) {
        //    pois[i].AdjustHP(-100, ELEMENTAL_TYPE.Electric);
        //}
        ObjectPoolManager.Instance.DestroyObject(gameObject);
    }
}
