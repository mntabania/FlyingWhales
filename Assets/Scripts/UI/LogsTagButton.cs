using System.Collections.Generic;
using EZObjectPools;
using UnityEngine;
using UnityEngine.UI;

public class LogsTagButton : PooledObject {

    [SerializeField] private Image mainTagImage;
    [SerializeField] private GameObject additionalTagsPlusObject;
    [SerializeField] private GameObject additionalTagsGO;
    [SerializeField] private RectTransform additionalTagsRect;
    [SerializeField] private GameObject tagWithNamePrefab;

    private List<LOG_TAG> localTags;
    private bool hasPopulatedTagsGO;
    private List<PooledObject> _logTagObjects; 
    
    private void Awake() {
        localTags = new List<LOG_TAG>();
        _logTagObjects = new List<PooledObject>();
    }
    public void SetTags(List<LOG_TAG> tags) {
        LOG_TAG mainTag = tags[0];
        mainTagImage.sprite = UIManager.Instance.GetLogTagSprite(mainTag);
        localTags.AddRange(tags);
        additionalTagsPlusObject.gameObject.SetActive(tags.Count > 1);
    }

    public void ShowAllTags() {
        if (!hasPopulatedTagsGO) {
            CreateTagItems();
        }
        additionalTagsGO.gameObject.SetActive(true);    
    }
    private void CreateTagItems() {
        //populate additional tags
        for (int i = 0; i < localTags.Count; i++) {
            LOG_TAG logTag = localTags[i];
            GameObject tagGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(tagWithNamePrefab.name, Vector3.zero, Quaternion.identity, additionalTagsRect);
            LogTagWithName logTagWithName = tagGO.GetComponent<LogTagWithName>();
            logTagWithName.SetTag(logTag);
            _logTagObjects.Add(logTagWithName);
        }
        hasPopulatedTagsGO = true;
    }
    public void HideAllTags() {
        additionalTagsGO.gameObject.SetActive(false);    
    }
    
    public override void Reset() {
        hasPopulatedTagsGO = false;
        localTags.Clear();
        for (int i = 0; i < _logTagObjects.Count; i++) {
            PooledObject logTagObj = _logTagObjects[i];
            ObjectPoolManager.Instance.DestroyObjectWithoutCheckingChildren(logTagObj);
        }
        _logTagObjects.Clear();
    }
}
