using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Logic.Base;
using Logic.Component;
using YamlDotNet.RepresentationModel;

namespace Logic.Reader
{
  public class EntityInfoReader
  {
    private static readonly Dictionary<string, Func<YamlNode, EntityDef, bool>> compName2HandleDict_ = new(){
            {"transform_comp", ParseTransformCompDef}, // 变换组件
            {"movement_comp", ParseMovementCompDef}, // 运动组件
            {"input_comp", ParseInputCompDef}, // 输入组件
            {"tracking_comp", ParseTrackingCompDef}, // 跟踪组件
            {"camp_comp", ParseCampCompDef}, // 阵营组件
            {"tag_comp", ParseTagCompDef}, // Tag组件
            {"shooting_comp", ParseShootingCompDef}, // 射击组件
            {"lifetime_comp", ParseLifeTimeCompDef}, // 生命时间组件
            {"projectile_comp", ParseProjectileCompDef}, //projectile组件
            {"search_comp", ParseSearchCompDef}, // search组件
            {"behaviour_comp", ParseBehaviourCompDef}, // behaviour组件
            {"collider_comp", ParseColliderCompDef}, // collider组件
            {"character_comp", ParseCharCompDef}, // character组件
        };
    private readonly Dictionary<int, EntityDef> entityDefDict_;

    public EntityInfoReader()
    {
      entityDefDict_ = new();
    }

    public bool Load(string filePath)
    {
      StreamReader reader = new(filePath);
      YamlStream yaml = new();
      yaml.Load(reader);
      YamlMappingNode rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
      if (rootNode == null)
      {
        DebugLog.Error("Cant get root node");
        return false;
      }

      YamlSequenceNode entityListNode = (YamlSequenceNode)rootNode.Children[new YamlScalarNode("entities_def")];
      if (entityListNode == null)
      {
        DebugLog.Error("Root node tag must entities");
        return false;
      }

      foreach (YamlNode n in entityListNode.Children)
      {
        YamlMappingNode entityNode = (YamlMappingNode)n;
        EntityDef def = ParseEntityDef(entityNode);
        if (def.Id == 0)
        {
          DebugLog.Warning("Entity node not found id");
          continue;
        }
        entityDefDict_.Add(def.Id, def);
      }
      return true;
    }

    public bool LoadMulti(string[] filesPath)
    {
      foreach (string filePath in filesPath)
      {
        if (!Load(filePath))
        {
          return false;
        }
      }
      return true;
    }

    public EntityDef GetEntity(int id)
    {
      return !entityDefDict_.TryGetValue(id, out EntityDef entityDef) ? null : entityDef;
    }

    private EntityDef ParseEntityDef(YamlMappingNode entityNode)
    {
      EntityDef def = new();
      foreach (KeyValuePair<YamlNode, YamlNode> e in entityNode.Children)
      {
        string keyName = ((YamlScalarNode)e.Key).Value;
        if (keyName == "id")
        {
          def.Id = int.Parse(e.Value.ToString());
        }
        else if (keyName == "name")
        {
          def.Name = e.Value.ToString();
        }
        else if (keyName == "inherit")
        {
          int baseId = int.Parse(e.Value.ToString());
          if (!entityDefDict_.TryGetValue(baseId, out EntityDef baseEntity))
          {
            throw new Exception("Cant found entity " + baseId + " definition");
          }
          baseEntity.CloneOrMerge(def);
        }
        else if (keyName == "components")
        {
          YamlMappingNode components = (YamlMappingNode)e.Value;
          foreach (KeyValuePair<YamlNode, YamlNode> comp in components.Children)
          {
            string compName = ((YamlScalarNode)comp.Key).Value;
            if (compName2HandleDict_.TryGetValue(compName, out Func<YamlNode, EntityDef, bool> parseHandle))
            {
              if (!parseHandle(comp.Value, def))
              {
                DebugLog.Warning("Parse" + compName + " node failed");
                continue;
              }
            }
          }
        }
        else if (keyName == "sub_entities")
        {
          YamlSequenceNode subEntitys = (YamlSequenceNode)e.Value;
          foreach (YamlNode subEntity in subEntitys.Children)
          {
            YamlMappingNode subEntityMap = (YamlMappingNode)subEntity;
            EntityDef subDef = ParseEntityDef(subEntityMap);
            if (subDef == null)
            {
              DebugLog.Warning("Parse SubEntityDef of EntityDef" + def.Id + " failed");
              continue;
            }
            def.SubEntityDefList ??= new();
            def.SubEntityDefList.Add(subDef);
          }
        }
      }
      return def;
    }

    // 解析transform组件
    private static bool ParseTransformCompDef(YamlNode value, EntityDef def)
    {
      YamlMappingNode mappingNode = (YamlMappingNode)value;
      TransformCompDef compDef = new();
      ComponentInfoReader.ParseTransform(mappingNode, compDef, null);
      def.CompDefList ??= new List<CompDef>();
      def.CompDefList.Add(compDef);
      return true;
    }

    // 解析movement组件
    private static bool ParseMovementCompDef(YamlNode value, EntityDef def)
    {
      YamlMappingNode mappingNode = (YamlMappingNode)value;
      MovementCompDef compDef = null;
      ComponentInfoReader.ParseMovement(mappingNode, ref compDef, true);
      def.CompDefList ??= new List<CompDef>();
      def.CompDefList.Add(compDef);
      return true;
    }

    private static bool ParseInputCompDef(YamlNode value, EntityDef def)
    {
      InputCompDef compDef = new();
      def.CompDefList ??= new List<CompDef>();
      def.CompDefList.Add(compDef);
      return true;
    }

    private static bool ParseTrackingCompDef(YamlNode value, EntityDef def)
    {
      return true;
    }

    private static bool ParseCampCompDef(YamlNode value, EntityDef def)
    {
      YamlMappingNode mappingNode = (YamlMappingNode)value;
      CampCompDef compDef = new();
      ComponentInfoReader.ParseCamp(mappingNode, compDef, null);
      def.CompDefList ??= new();
      def.CompDefList.Add(compDef);
      return true;
    }

    private static bool ParseTagCompDef(YamlNode value, EntityDef def)
    {
      YamlMappingNode mappingNode = (YamlMappingNode)value;
      TagCompDef compDef = new();
      ComponentInfoReader.ParseTag(mappingNode, compDef, null);
      def.CompDefList ??= new();
      def.CompDefList.Add(compDef);
      return true;
    }

    private static bool ParseShootingCompDef(YamlNode value, EntityDef def)
    {
      YamlMappingNode mappingNode = (YamlMappingNode)value;
      ShootingCompDef compDef = new();
      ComponentInfoReader.ParseShooting(mappingNode, compDef);
      def.CompDefList ??= new();
      def.CompDefList.Add(compDef);
      return true;
    }

    private static bool ParseLifeTimeCompDef(YamlNode value, EntityDef def)
    {
      YamlMappingNode mappingNode = (YamlMappingNode)value;
      LifeTimeCompDef compDef = new();
      ComponentInfoReader.ParseLifeTime(mappingNode, compDef);
      def.CompDefList ??= new();
      def.CompDefList.Add(compDef);
      return true;
    }

    /*private static bool ParseAABBCompDef(YamlNode value, EntityDef def)
    {
      YamlMappingNode mappingNode = (YamlMappingNode)value;
      AABBCompDef compDef = new();
      ComponentInfoReader.ParseAABB(mappingNode, compDef, null);
      def.CompDefList ??= new();
      def.CompDefList.Add(compDef);
      return true;
    }*/

    private static bool ParseProjectileCompDef(YamlNode value, EntityDef def)
    {
      YamlMappingNode mappingNode = (YamlMappingNode)value;
      ProjectileCompDef compDef = ComponentInfoReader.ParseProjectile(mappingNode);
      def.CompDefList ??= new();
      def.CompDefList.Add(compDef);
      return true;
    }

    private static bool ParseSearchCompDef(YamlNode value, EntityDef def)
    {
      YamlMappingNode mappingNode = (YamlMappingNode)value;
      SearchCompDef compDef = new();
      ComponentInfoReader.ParseSearch(mappingNode, compDef);
      def.CompDefList ??= new();
      def.CompDefList.Add(compDef);
      return true;
    }

    private static bool ParseBehaviourCompDef(YamlNode value, EntityDef def)
    {
      YamlMappingNode mappingNode = (YamlMappingNode)value;
      BehaviourCompDef compDef = new();
      ComponentInfoReader.ParseBehaviour(mappingNode, compDef);
      def.CompDefList ??= new();
      def.CompDefList.Add(compDef);
      return true;
    }

    private static bool ParseColliderCompDef(YamlNode value, EntityDef def)
    {
      YamlMappingNode mappingNode = (YamlMappingNode)value;
      ColliderCompDef compDef = new();
      ComponentInfoReader.ParseCollider(mappingNode, compDef, null);
      def.CompDefList ??= new();
      def.CompDefList.Add(compDef);
      return true;
    }

    private static bool ParseCharCompDef(YamlNode value, EntityDef def)
    {
      YamlMappingNode mappingNode = (YamlMappingNode)value;
      CharacterCompDef compDef = new();
      ComponentInfoReader.ParseChar(mappingNode, compDef, null);
      def.CompDefList ??= new();
      def.CompDefList.Add(compDef);
      return true;
    }
  }
}