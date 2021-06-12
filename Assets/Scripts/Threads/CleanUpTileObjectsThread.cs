using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UtilityScripts;

public class CleanUpTileObjectsThread : Multithread {
    private List<WeakReference> listToBeCleanedUp;
    private Dictionary<string, WeakReference> dictionaryToBeCleanedUp;
    private Dictionary<string, WeakReference> cleanDictionary;
    public bool isProcessing { get; private set; }
    public CleanUpTileObjectsThread() {
    }

    #region Overrides
    public override void DoMultithread() {
        base.DoMultithread();
        RemoveAllDeadReferences();
    }
    public override void FinishMultithread() {
        base.FinishMultithread();
        SetListToBeCleanedUp(null);
        SetIsProcessing(false);
        DatabaseManager.Instance.tileObjectDatabase.DoneProcessCleanUpDestroyedTileObjects(cleanDictionary);
        cleanDictionary = null;
        DatabaseManager.Instance.tileObjectDatabase.AfterDone();
    }
    #endregion

    #region Process
    private void RemoveAllDeadReferences() {
        for (int i = 0; i < listToBeCleanedUp.Count; i++) {
            WeakReference wr = listToBeCleanedUp[i];
            if (!wr.IsAlive) {
                listToBeCleanedUp.RemoveAt(i);
                i--;
            }
        }
        if (dictionaryToBeCleanedUp != null) {
            //This is okay since we do not change the dictionaryToBeCleanedUp collection, we simply traverse it
            //This will not result in any race conditions because the collection does not change
            foreach (string item in dictionaryToBeCleanedUp.Keys) {
                WeakReference wr = dictionaryToBeCleanedUp[item];
                if (wr.IsAlive) {
                    cleanDictionary.Add(item, wr);
                }
            }
        }
    }
    #endregion

    public void SetDictionaryToBeCleanedUp(Dictionary<string, WeakReference> p_dictionaryToBeCleanedUp) {
        dictionaryToBeCleanedUp = p_dictionaryToBeCleanedUp;

        //Claim a new dictionary here
        //We will add all alive references here
        //After processing the removal of dead references, we will switch this dictionary to the destroyedTileObjectsDictionary
        //Then the destroyedTileObjectsDictionary will be put back to the object pool
        //That is the reason we claim a new dictionary here
        //So that there will be no memory to waste, hence, no garbage
        cleanDictionary = RuinarchCleanUpDictionaryPool.Claim();
    }
    public void SetListToBeCleanedUp(List<WeakReference> p_list) {
        listToBeCleanedUp = p_list;
    }
    public void SetIsProcessing(bool p_state) {
        isProcessing = p_state;
    }
}
