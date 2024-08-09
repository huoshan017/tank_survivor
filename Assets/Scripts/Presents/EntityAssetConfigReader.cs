using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Common.Geometry;
using Logic.Reader;
using UnityEngine;
using YamlDotNet.RepresentationModel;

public class PrefabSubNode
{
  public string Name;
  public string ComponentScript;
}

public class AssetConfig
{
  public int Id;
  public string Prefab;
  public string ComponentScript;
  public Position HpBarOffset;
  public List<PrefabSubNode> SubObjs;
}

public class EntityAssetConfigReader
{
  readonly Dictionary<int, AssetConfig> entity2AssetDict_;
  readonly List<AssetConfig> assetConfigList_;

  public EntityAssetConfigReader()
  {
    entity2AssetDict_ = new();
    assetConfigList_ = new();
  }

  public bool Load(string filePath)
  {
    var reader = new StreamReader(filePath);
    var yaml = new YamlStream();
    yaml.Load(reader);
    var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
    if (rootNode == null)
    {
      DebugLog.Error("Cant get root node from yaml file " + filePath);
      return false;
    }
    var mapListNode = (YamlSequenceNode)rootNode[new YamlScalarNode("entity_assets")];
    if (mapListNode == null)
    {
      DebugLog.Error("cant get node \"entity_asset_map\" from file " + filePath);
      return false;
    }
    foreach (YamlMappingNode mn in mapListNode.Cast<YamlMappingNode>())
    {
      var a = mn[new YamlScalarNode("id")];
      int id = int.Parse(a.ToString());
      if (id <= 0)
      {
        DebugLog.Warning("entity id " + id + " invalid");
        continue;
      }

      var assetConfig = new AssetConfig
      {
        Id = id
      };
      foreach (var child in mn.Children)
      {
        var key = ((YamlScalarNode)child.Key).Value;
        if (key == "prefab")
        {
          assetConfig.Prefab = child.Value.ToString();
        }
        else if (key == "component_script")
        {
          assetConfig.ComponentScript = child.Value.ToString();
        }
        else if (key == "hpbar_offset")
        {
          assetConfig.HpBarOffset = ComponentInfoReader.ParsePos((YamlMappingNode)child.Value);
        }
        else if (key == "sub_objs")
        {
          foreach (var sn in ((YamlSequenceNode)child.Value).Children)
          {
            PrefabSubNode prefabSubNode = null;
            var snNode = (YamlMappingNode)sn;
            foreach (var snn in snNode.Children)
            {
              var snKey = ((YamlScalarNode)snn.Key).Value;
              if (snKey == "name")
              {
                prefabSubNode ??= new();
                prefabSubNode.Name = snn.Value.ToString();
              }
              else if (snKey == "component_script")
              {
                prefabSubNode ??= new();
                prefabSubNode.ComponentScript = snn.Value.ToString();
              }
            }
            if (prefabSubNode != null)
            {
              assetConfig.SubObjs ??= new();
              assetConfig.SubObjs.Add(prefabSubNode);
            }
          }
        }
      }
      entity2AssetDict_.Add(id, assetConfig);
      assetConfigList_.Add(assetConfig);
    }

    return true;
  }

  public AssetConfig Get(int entityId)
  {
    if (!entity2AssetDict_.TryGetValue(entityId, out var config))
    {
      DebugLog.Error("entity id " + entityId + " can not get asset id");
      return null;
    }
    return config;
  }

  public List<AssetConfig> GetAssetConfigList()
  {
    return assetConfigList_;
  }
}