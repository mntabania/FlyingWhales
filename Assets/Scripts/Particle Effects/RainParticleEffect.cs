using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainParticleEffect : BaseParticleEffect {
    protected override IEnumerator PlayParticleCoroutine() {
        //Playing particle effect is done in a coroutine so that it will wait one frame before pausing the particles if the game is paused when the particle is activated
        //This will make sure that the particle effect will show but it will be paused right away
        PlayParticle();
        yield return new WaitForSeconds(0.4f);
        if (pauseOnGamePaused && GameManager.Instance.isPaused) {
            PauseParticle();
        }
    }
}
