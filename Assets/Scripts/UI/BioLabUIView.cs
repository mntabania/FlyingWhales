using System;
using TMPro;
using UnityEngine;
namespace UI {
    public class BioLabUIView : BaseMonoBehaviour {
        public static BioLabUIView Instance;

        [SerializeField] private TextMeshProUGUI _activeCasesLbl;
        [SerializeField] private TextMeshProUGUI _deathsLbl;
        [SerializeField] private TextMeshProUGUI _recoveriesLbl;
        [SerializeField] private TextMeshProUGUI _plaguedRatsLbl;
        [SerializeField] private TextMeshProUGUI _plaguePointsLbl;
        
        // [Header("Transmissions")]
        
        
        private void Awake() {
            Instance = this;
        }
        protected override void OnDestroy() {
            base.OnDestroy();
            Instance = null;
        }
        
        
    }
}