using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class ResourceStorageComponent {
    public Dictionary<RESOURCE, int> storedResources { get; }
    public Dictionary<RESOURCE, int> maxResourceValues { get; }
    public Dictionary<CONCRETE_RESOURCES, int> specificStoredResources { get; }

    /// <summary>
    /// Dictionary used for looping specificStoredResources, but loop needs to
    /// edit values inside specificStoredResources. This is to prevent
    /// InvalidOperationException: Collection was modified; enumeration operation may not execute.
    /// when using function <see cref="ReduceMainResourceUsingRandomSpecificResources"/> and <see cref="ClearAllResources"/>
    /// </summary>
    private Dictionary<CONCRETE_RESOURCES, int> _specificStoredResourcesCopy;
    private Dictionary<RESOURCE, int> _storedResourcesCopy;
    
    public ResourceStorageComponent() {
        storedResources = new Dictionary<RESOURCE, int>();
        //maxResourceValues = new Dictionary<RESOURCE, int>();
        _specificStoredResourcesCopy = new Dictionary<CONCRETE_RESOURCES, int>();
        _storedResourcesCopy = new Dictionary<RESOURCE, int>();
        RESOURCE[] resourceTypes = CollectionUtilities.GetEnumValues<RESOURCE>();
        for (int i = 0; i < resourceTypes.Length; i++) {
            RESOURCE resourceType = resourceTypes[i];
            if (resourceType != RESOURCE.NONE) {
                storedResources.Add(resourceType, 0);
                //maxResourceValues.Add(resourceType, 1000);
            }
        }
        specificStoredResources = new Dictionary<CONCRETE_RESOURCES, int>();
        CONCRETE_RESOURCES[] resources = CollectionUtilities.GetEnumValues<CONCRETE_RESOURCES>();
        for (int i = 0; i < resources.Length; i++) {
            CONCRETE_RESOURCES resourceType = resources[i];
            specificStoredResources.Add(resourceType, 0);    
        }
    }
    public ResourceStorageComponent(SaveDataResourceStorageComponent p_data) {
        storedResources = p_data.storedResources;
        //maxResourceValues = p_data.maxResources;
        specificStoredResources = p_data.specificStoredResources;
        _specificStoredResourcesCopy = new Dictionary<CONCRETE_RESOURCES, int>();
        _storedResourcesCopy = new Dictionary<RESOURCE, int>();
    }

    #region Resource Management
    public void SetResourceCap(RESOURCE p_resource, int p_cap) {
        //if (!maxResourceValues.ContainsKey(p_resource)) {
        //    maxResourceValues.Add(p_resource, 0);
       //// }
        //maxResourceValues[p_resource] = p_cap;
    }
    private void SetResource(RESOURCE resourceType, int amount) {
        storedResources[resourceType] = amount;
        storedResources[resourceType] = Mathf.Max(storedResources[resourceType], 0);
    }
    private void AdjustResource(RESOURCE resourceType, int amount) {
        storedResources[resourceType] += amount;
        storedResources[resourceType] = Mathf.Max(storedResources[resourceType], 0);
    }
    public void ClearAllResources() {
        //clear stored resources
        _storedResourcesCopy.Clear();
        foreach (var kvp in storedResources) {
            _storedResourcesCopy.Add(kvp.Key, kvp.Value);    
        }
        foreach (var kvp in _storedResourcesCopy) {
            storedResources[kvp.Key] = 0;
        }
        
        //clear specific resources
        _specificStoredResourcesCopy.Clear();
        foreach (var kvp in specificStoredResources) {
            _specificStoredResourcesCopy.Add(kvp.Key, kvp.Value);    
        }
        foreach (var kvp in _specificStoredResourcesCopy) {
            specificStoredResources[kvp.Key] = 0;
        }
    }
    public void ReduceMainResourceUsingRandomSpecificResources(RESOURCE p_resource, int amount) {
        //Even though this function reduces resources, it will only accept positive amounts.
        //This is to prevent complications in the computation.
        Assert.IsTrue(amount > 0);
        int remainingAmountToReduce = amount;
        _specificStoredResourcesCopy.Clear();
        foreach (var kvp in specificStoredResources) {
            _specificStoredResourcesCopy.Add(kvp.Key, kvp.Value);    
        }
        foreach (var kvp in _specificStoredResourcesCopy) {
            if (remainingAmountToReduce <= 0) { break; }
            int specificResourceAmount = kvp.Value;
            CONCRETE_RESOURCES specificResource = kvp.Key;
            if (specificResource.GetResourceCategory() == p_resource && specificResourceAmount > 0) {
                int reduction = remainingAmountToReduce;
                if (reduction > specificResourceAmount) {
                    reduction = specificResourceAmount;
                }
                remainingAmountToReduce -= reduction;
                AdjustResource(specificResource, -reduction);
            }
        }
        _specificStoredResourcesCopy.Clear();
        if (remainingAmountToReduce > 0) {
#if DEBUG_LOG
            Debug.LogWarning($"wanted to reduce {p_resource.ToString()} by {amount.ToString()} but resource storage did not have enough resources to reduce by that amount {remainingAmountToReduce} was left");
#endif
        }
    }
    public void AdjustResource(CONCRETE_RESOURCES p_resource, int p_amount) {
        specificStoredResources[p_resource] += p_amount;
        specificStoredResources[p_resource] = Mathf.Max(specificStoredResources[p_resource], 0);
        RESOURCE resourceCategory = p_resource.GetResourceCategory();
        AdjustResource(resourceCategory, p_amount);
    }
    public void SetResource(CONCRETE_RESOURCES p_resource, int p_amount) {
        Assert.IsTrue(p_amount >= 0, $"A concrete resource is being set as negative! Resource is {p_resource.ToString()}");
        int previousAmount = specificStoredResources[p_resource];
        specificStoredResources[p_resource] = p_amount;
        specificStoredResources[p_resource] = Mathf.Max(specificStoredResources[p_resource], 0);
        int newAmount = specificStoredResources[p_resource];
        int change = newAmount - previousAmount;
        
        RESOURCE resourceCategory = p_resource.GetResourceCategory();
        AdjustResource(resourceCategory, change);
    }
    #endregion

    #region Inquiry
    public bool HasResourceAmount(RESOURCE resourceType, int amount) {
        return storedResources[resourceType] >= amount;
    }
    public bool IsAtMaxResource(RESOURCE resource) {
        return false;
        //return storedResources[resource] >= maxResourceValues[resource];
    }
    public bool HasEnoughSpaceFor(RESOURCE resource, int amount) {
        int newAmount = storedResources[resource] + amount;
        return true;
        //return newAmount <= maxResourceValues[resource];
    }
    public int GetResourceValue(RESOURCE resource) {
        return storedResources[resource];
    }
    #endregion
}

#region Save Data
public class SaveDataResourceStorageComponent : SaveData<ResourceStorageComponent> {
    public Dictionary<RESOURCE, int> storedResources;
    public Dictionary<RESOURCE, int> maxResources;
    public Dictionary<CONCRETE_RESOURCES, int> specificStoredResources;
    public override void Save(ResourceStorageComponent data) {
        base.Save(data);
        storedResources = data.storedResources;
        maxResources = data.maxResourceValues;
        specificStoredResources = data.specificStoredResources;
    }
    public override ResourceStorageComponent Load() {
        return new ResourceStorageComponent(this);
    }
}
#endregion