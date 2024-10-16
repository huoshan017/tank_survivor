using System;
using System.Collections.Generic;
using Common;
using Logic;
using Logic.Entity;
using Utils;
using UnityEngine;

public class PlayableWorld : MonoBehaviour, IPlayableContext
{
  void Awake()
  {
    assetReader_ = new();
    string configPath = "Configuration/EntityAssetConfig.yaml";
    if (!assetReader_.Load(Accessory.GetStreamingAssetsFullPath(configPath)))
    {
      DebugLog.Error("Load asset config " + configPath + " failed");
      return;
    }
    PrefabConfigReader prefabConfigReader = new();
    configPath = "Configuration/PrefabConfig.yaml";
    if (!prefabConfigReader.Load(Accessory.GetStreamingAssetsFullPath(configPath)))
    {
      DebugLog.Error("Load prefab config " + configPath + " failed");
      return;
    }
    prefabManager_ = new(prefabConfigReader, assetReader_);
    playableGameObjectDict_ = new Dictionary<uint, GameObject>();
  }

  void Update()
  {
    for (int i = 0; i < linePositionPairs_.Length; i++)
    {
      var lineRenderer = lineRendererArray_.GetLineRendererWithLine(i);
      lineRenderer.positionCount = 2;
      lineRenderer.SetPositions(linePositionPairs_[i]);
    }
    for (int i = 0; i < columnPositionPairs_.Length; i++)
    {
      var lineRenderer = lineRendererArray_.GetLineRendererWithColumn(i);
      lineRenderer.positionCount = 2;
      lineRenderer.SetPositions(columnPositionPairs_[i]);
    }
    prefabManager_.Update(world_.FrameNum());
  }

  public void Init()
  {
    world_.RegisterEntityAddHandle(AddEntityHandle);
    world_.RegisterEntityRemoveHandle(RemoveEntityHandle);
  }

  public void Uninit()
  {
    world_.UnregisterEntityRemoveHandle(RemoveEntityHandle);
    world_.UnregisterEntityAddHandle(AddEntityHandle);
  }

  public void Attach(World world)
  {
    world_ = world;
  }

  public void Detach()
  {
    world_ = null;
  }

  public void AfterLoadMapHandle()
  {
    InitDrawGridLineComp();
  }

  public GameObject GetEntityGameObject(uint entityInstId)
  {
    if (!playableGameObjectDict_.TryGetValue(entityInstId, out var obj)) return null;
    return obj;
  }

  public GameObject InstantiateEntityGameObject(Entity e)
  {
    var asset = assetReader_.Get(e.Id());
    if (asset == null)
    {
      DebugLog.Error("entity " + e.Id() + " asset config not found");
      return null;
    }
    
    if (playableGameObjectDict_.TryGetValue(e.InstId(), out var obj))
    {
      return obj;
    }

    obj = prefabManager_.InstantiateEntity(e.Id());
    PlayableEntity playableEntity = (PlayableEntity)obj.GetComponent(asset.ComponentScript);
    playableEntity.PlayableContext = this;
    playableEntity.Attach(e, asset);

    playableGameObjectDict_.Add(e.InstId(), obj);

    // 处理prefab中的子物体
    var children = e.GetChildren();
    if (children != null && children.Length > 0)
    {
      for (int i = 0; i < asset.SubObjs.Count; i++)
      {
        var subObj = asset.SubObjs[i];
        var childTransform = obj.transform.Find(subObj.Name);
        PlayableEntity subPlayableEntity = (PlayableEntity)childTransform.gameObject.GetComponent(subObj.ComponentScript);
        if (subPlayableEntity == null)
        {
          DebugLog.Error("Cant get component by script " + subObj.ComponentScript);
        }
        subPlayableEntity.Attach((Entity)children[i], asset);
      }
    }

    if (e.Parent != null)
    {
      if (playableGameObjectDict_.TryGetValue(e.Parent.InstId(), out var parentPlayableEntity))
      {
        playableEntity.transform.parent = parentPlayableEntity.transform;
      }
    }
    else
    {
      if (root_ == null)
      {
        root_ = GameObject.Find("Root");
        if (root_ == null)
        {
          throw new Exception("Cant found Root GameObject");
        }
      }
      var entitiesNode = CheckAndBuildParentNode("Entities");
      playableEntity.transform.parent = entitiesNode.transform;
    }

    return obj;
  }

  public bool RecycleEntityGameObject(uint entityInstId, int entityId)
  {
    var asset = assetReader_.Get(entityId);
    if (asset == null) return false;
    if (!playableGameObjectDict_.Remove(entityInstId, out var obj)) return false;
    if (asset.ComponentScript != "")
    {
      var playableEntity = (PlayableEntity)obj.GetComponent(asset.ComponentScript);
      if (playableEntity != null)
      {
        playableEntity.Detach();
        if (asset.SubObjs != null)
        {
          for (int i = 0; i < asset.SubObjs.Count; i++)
          {
            var subObj = asset.SubObjs[i];
            var childTransform = obj.transform.Find(subObj.Name);
            PlayableEntity subPlayableEntity = (PlayableEntity)childTransform.gameObject.GetComponent(subObj.ComponentScript);
            if (subPlayableEntity == null)
            {
              DebugLog.Error("Cant get component by script " + subObj.ComponentScript);
            }
            subPlayableEntity.Detach();
          }
        }
      }
    }
    Destroy(obj);
    return true;
  }

  bool AddEntityHandle(uint entityInstId)
  {
    var entity = world_.GetEntity(entityInstId);
    if (entity == null) return false;
    if (InstantiateEntityGameObject((Entity)entity) == null)
    {
      return false;
    }
    Debug.Log("create playableEntity " + entity.InstId());
    return true;
  }

  bool RemoveEntityHandle(uint entityInstId, int entityId)
  {
    var removed = RecycleEntityGameObject(entityInstId, entityId);
    if (removed)
    {
      Debug.Log("GameObject for entity" + entityInstId + " was destroyed");
    }
    return removed;
  }

  void InitDrawGridLineComp()
  {
    // 初始化画线组件
    int line = 1 + world_.MapHeight / world_.GridHeight;
    int column = 1 + world_.MapWidth / world_.GridWidth;
    lineRendererArray_ = new LineRendererArray(gameObject.transform);
    lineRendererArray_.Init(line, column);
    linePositionPairs_ = new Vector3[line][];
    for (int i = 0; i < line; i++)
    {
      linePositionPairs_[i] = new Vector3[2];
      linePositionPairs_[i][0] = new Vector3(world_.MapLeft, 0, world_.MapBottom + i * world_.GridHeight) / GlobalConstant.LogicAndUnityRatio;
      linePositionPairs_[i][1] = new Vector3(world_.MapLeft + world_.MapWidth, 0, world_.MapBottom + i * world_.GridHeight) / GlobalConstant.LogicAndUnityRatio;
    }
    columnPositionPairs_ = new Vector3[column][];
    for (int i = 0; i < column; i++)
    {
      columnPositionPairs_[i] = new Vector3[2];
      columnPositionPairs_[i][0] = new Vector3(world_.MapLeft + i * world_.GridWidth, 0, world_.MapBottom) / GlobalConstant.LogicAndUnityRatio;
      columnPositionPairs_[i][1] = new Vector3(world_.MapLeft + i * world_.GridWidth, 0, world_.MapBottom + world_.MapHeight) / GlobalConstant.LogicAndUnityRatio;
    }
  }

  public GameObject InstantiatePfxGameObject(string name)
  {
    return prefabManager_.InstantiatePfx(name);
  }

  public GameObject InstantiatePfxGameObject(string name, string parent)
  {
    var parentNode = CheckAndBuildParentNode(parent);
    var pfx = prefabManager_.InstantiatePfx(name);
    pfx.transform.parent = parentNode.transform;
    return pfx;
  }

  public bool RecyclePfxGameObject(GameObject obj)
  {
    return RecyclePfxGameObject(obj, 0);
  }

  public bool RecyclePfxGameObject(GameObject obj, uint afterMs)
  {
    return prefabManager_.RecyclePfxLater(obj, afterMs, world_.FrameNum(), world_.FrameMs());
  }

  public GameObject InstantiateUiGameObject(string name)
  {
    return prefabManager_.InstantiateUi(name);
  }

  public GameObject InstantiateUiGameObject(string name, string parent)
  {
    var parentNode = CheckAndBuildParentNode(parent);
    var ui = prefabManager_.InstantiateUi(name);
    ui.transform.parent = parentNode.transform;
    return ui;
  }

  public bool RecycleUiGameObject(GameObject uiObj)
  {
    return prefabManager_.RecycleUiGameObject(uiObj);
  }

  GameObject CheckAndBuildParentNode(string parent)
  {
    nodeDict_ ??= new();
    if (!nodeDict_.TryGetValue(parent, out var parentNode))
    {
      parentNode = new GameObject(parent);
      parentNode.transform.parent = root_.transform;
      nodeDict_.Add(parent, parentNode);
    }
    return parentNode;
  }

  internal World world_;
  Dictionary<uint, GameObject> playableGameObjectDict_;
  GameObject root_;
  Dictionary<string, GameObject> nodeDict_;
  EntityAssetConfigReader assetReader_;

  PrefabManager prefabManager_;
  LineRendererArray lineRendererArray_;
  Vector3[][] linePositionPairs_;
  Vector3[][] columnPositionPairs_;
}