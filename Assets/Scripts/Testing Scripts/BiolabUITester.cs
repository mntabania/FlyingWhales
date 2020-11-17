using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiolabUITester : MonoBehaviour {

    [SerializeField] private BiolabUIController _biolabUIController;
    
    // Start is called before the first frame update
    void Start() {
        _biolabUIController.Init();
        _biolabUIController.Open();
    }
}
