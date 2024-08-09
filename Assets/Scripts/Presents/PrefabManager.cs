using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

public class PrefabManager
{
  struct TimeoutPfx : IComparable<TimeoutPfx>
  {
    public uint EndFrameNum;
    public GameObject Pfx;

    public readonly int CompareTo(TimeoutPfx other)
    {
      if (EndFrameNum < other.EndFrameNum) return -1;
      else if (EndFrameNum > other.EndFrameNum) return 1;
      else return 0;
    }
  }

  readonly EntityAssetConfigReader entityAssetReader_;
  readonly PrefabConfigReader prefabConfigReader_;
  readonly Dictionary<int, GameObject> prefabModelDict_;
  readonly Dictionary<string, GameObject> prefabPfxDict_;
  readonly HashSet<GameObject> pfxGameObjectSet_;
  readonly Dictionary<string, GameObject> prefabUiDict_;
  readonly HashSet<GameObject> uiGameObjectSet_;
  readonly MinBinaryHeap<TimeoutPfx> pfxTimeoutHeap_;

  public PrefabManager(PrefabConfigReader prefabConfigReader, EntityAssetConfigReader entityAssetReader)
  {
    prefabConfigReader_ = prefabConfigReader;
    entityAssetReader_ = entityAssetReader;
    prefabModelDict_ = new();
    prefabPfxDict_ = new();
    pfxGameObjectSet_ = new();
    prefabUiDict_ = new();
    uiGameObjectSet_ = new();
    pfxTimeoutHeap_ = new();
  }

  public GameObject InstantiateEntity(int entityId)
  {
    var assetConfig = entityAssetReader_.Get(entityId);
    if (assetConfig == null)
    {
      DebugLog.Error("Cant get asset config with entity id " + entityId);
      return null;
    }
    if (!prefabModelDict_.TryGetValue(assetConfig.Id, out var prefab))
    {
      prefab = Resources.Load<GameObject>(assetConfig.Prefab);
      if (prefab == null)
      {
        DebugLog.Error("Load resource " + assetConfig.Prefab + " failed");
        return null;
      }
      prefabModelDict_.Add(assetConfig.Id, prefab);
    }
    return UnityEngine.Object.Instantiate(prefab);
  }

  public GameObject InstantiatePfx(string name)
  {
    if (!prefabConfigReader_.GetPfx(name, out var prefabInfo))
    {
      return null;
    }
    if (!prefabPfxDict_.TryGetValue(name, out var prefab))
    {
      prefab = Resources.Load<GameObject>(prefabInfo.Path);
      if (prefab == null)
      {
        DebugLog.Error("Load resource " + prefabInfo.Path + " failed");
        return null;
      }
      prefabPfxDict_.Add(name, prefab);
    }
    var obj = UnityEngine.Object.Instantiate(prefab);
    pfxGameObjectSet_.Add(obj);
    return obj;
  }

  public bool RecyclePfxLater(GameObject pfx, uint durationMs, uint currFrameNum, uint frameMs)
  {
    if (!pfxGameObjectSet_.Remove(pfx)) return false;
    if (durationMs == 0)
    {
      UnityEngine.Object.Destroy(pfx);
      return true;
    }
    pfxTimeoutHeap_.Set(new TimeoutPfx{EndFrameNum=currFrameNum+((durationMs+frameMs-1)/frameMs), Pfx=pfx});
    return true;
  }

  public GameObject InstantiateUi(string name)
  {
    if (!prefabConfigReader_.GetUi(name, out var prefabInfo))
    {
      return null;
    }
    if (!prefabUiDict_.TryGetValue(name, out var prefab))
    {
      prefab = Resources.Load<GameObject>(prefabInfo.Path);
      if (prefab == null)
      {
        DebugLog.Error("Load resource " + prefabInfo.Path + " failed");
        return null;
      }
      prefabUiDict_.Add(name, prefab);
    }
    var obj = UnityEngine.Object.Instantiate(prefab);
    uiGameObjectSet_.Add(obj);
    return obj;
  }

  public bool RecycleUiGameObject(GameObject uiObj)
  {
    if (!uiGameObjectSet_.Remove(uiObj)) return false;
    UnityEngine.Object.Destroy(uiObj);
    return true;
  }

  public void Update(uint frameNum)
  {
    while (pfxTimeoutHeap_.Peek(out var timeoutPfx))
    {
      if (timeoutPfx.EndFrameNum > frameNum)
      {
        break;
      }
      UnityEngine.Object.Destroy(timeoutPfx.Pfx);
      pfxTimeoutHeap_.Get(out _);
    }
  }
}