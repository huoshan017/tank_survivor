using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Common.Geometry;
using Logic.Component;
using Logic.Interface;
using Logic.Reader;
using YamlDotNet.RepresentationModel;

namespace Logic
{
    public struct MapLoadCache
    {
        public int Id;
        public string Name;
        public Rect Bounds;
        public int GridWidth, GridHeight;
        public List<YamlMappingNode> EntityDataList;
        public List<YamlMappingNode> FenceEntityList;
        public YamlMappingNode[] PlayerDataNodes;
    }

    public class MapReader
    {
        readonly IContext context_;

        public MapReader(IContext context)
        {
            context_ = context;
        }

        public bool LoadFile(string filePath, ref MapLoadCache mapCache)
        {
            var reader = new StreamReader(filePath);
            var yaml = new YamlStream();
            yaml.Load(reader);
            var mappingNode = (YamlMappingNode)yaml.Documents[0].RootNode;
            if (mappingNode == null)
            {
                DebugLog.Error("Cant get root node from yaml file " + filePath);
                return false;
            }

            foreach (var n in mappingNode.Children)
            {
                string keyName = ((YamlScalarNode)n.Key).Value;
                if (keyName == "id")
                {
                    mapCache.Id = int.Parse(n.Value.ToString());
                }
                else if (keyName == "name")
                {
                    mapCache.Name = n.Value.ToString();
                }
                else if (keyName == "bounds")
                {
                    int x = 0, y = 0, w = 0, h = 0;
                    var boundsNode = (YamlMappingNode)n.Value;
                    foreach (var bn in boundsNode.Children)
                    {
                        var bnKeyName = ((YamlScalarNode)bn.Key).Value;
                        if (bnKeyName == "x")
                        {
                            x = int.Parse(bn.Value.ToString());
                        }
                        else if (bnKeyName == "y")
                        {
                            y = int.Parse(bn.Value.ToString());
                        }
                        else if (bnKeyName == "w")
                        {
                            w = int.Parse(bn.Value.ToString());
                        }
                        else if (bnKeyName == "h")
                        {
                            h = int.Parse(bn.Value.ToString());
                        }
                    }
                    mapCache.Bounds = new Rect(x, y, w, h);
                }
                else if (keyName == "grid")
                {
                    var gridNode = (YamlMappingNode)n.Value;
                    foreach (var gn in gridNode.Children)
                    {
                        var gnKeyName = ((YamlScalarNode)gn.Key).Value;
                        if (gnKeyName == "width")
                        {
                            mapCache.GridWidth = int.Parse(gn.Value.ToString());
                        }
                        else if (gnKeyName == "height")
                        {
                            mapCache.GridHeight = int.Parse(gn.Value.ToString());
                        }
                    }
                }
                else if (keyName == "entities")
                {
                    if (n.Value.GetType() == typeof(YamlSequenceNode))
                    {
                        var nodeList = (YamlSequenceNode)n.Value;
                        foreach (var node in nodeList.Children)
                        {
                            mapCache.EntityDataList ??= new();
                            mapCache.EntityDataList.Add((YamlMappingNode)node);
                        }
                    }
                }
                else if (keyName == "fences")
                {
                    if (n.Value.GetType() == typeof(YamlSequenceNode))
                    {
                        var nodeList = (YamlSequenceNode)n.Value;
                        foreach (var node in nodeList.Children)
                        {
                            mapCache.FenceEntityList ??= new();
                            mapCache.FenceEntityList.Add((YamlMappingNode)node);
                            // 地图围栏跟其他实体不太一样，不需要加入到网格系统GridSystem中
                            // 所以直接在载入地图时创建
                            // GenerateEntity((YamlMappingNode)node);
                        }
                    }
                }
                else if (keyName == "players_info")
                {
                    if (n.Value.GetType() == typeof(YamlSequenceNode))
                    {
                        var nodeList = (YamlSequenceNode)n.Value;
                        mapCache.PlayerDataNodes = new YamlMappingNode[nodeList.Children.Count];
                        foreach (YamlMappingNode node in nodeList.Children.Cast<YamlMappingNode>())
                        {
                            var noNode = node.Children[new YamlScalarNode("no")];
                            if (noNode == null)
                            {
                                DebugLog.Error("map data: players_info node cant get \'no\' subnode");
                                return false;
                            }
                            mapCache.PlayerDataNodes[int.Parse(noNode.ToString())] = node;
                        }
                    }
                }
            }
            return true;
        }

        public IEntity GenerateEntity(YamlMappingNode entityMappingNode)
        {
            var idNode = entityMappingNode.Children[new YamlScalarNode("id")];
            if (idNode == null)
            {
                DebugLog.Error("Entity data node not found id subnode");
                return null;
            }
            var id = int.Parse(idNode.ToString());
            var entityDef = ConfigManager.GetEntityDef(id);
            if (entityDef == null)
            {
                DebugLog.Error("Entity def " + id + " not found");
                return null;
            }
            DebugLog.Info("entity def (id: " + id + ") (name: " + entityDef.Name + ") found, component list length " + entityDef.CompDefList.Count);

            var entity = context_.CreateEntity(entityDef);
            EntityInitData(entity, entityMappingNode);
            return entity;
        }

        public static void EntityInitData(IEntity entity, YamlMappingNode componentDataNode)
        {
            foreach (var n in componentDataNode.Children)
            {
                var key = ((YamlScalarNode)n.Key).Value;
                if (key == "transform_data")
                {
                    var transformComp = entity.GetComponent<TransformComponent>();
                    if (transformComp == null)
                    {
                        DebugLog.Error("!!! Cant get TransformComponent in entity " + entity.Id() + ", component data init failed");
                        return;
                    }
                    var tranformMappingNode = (YamlMappingNode)n.Value;
                    ComponentInfoReader.ParseTransform(tranformMappingNode, null, transformComp);
                }
                else if (key == "camp_data")
                {
                    var campComp = entity.GetComponent<CampComponent>();
                    if (campComp == null)
                    {
                        DebugLog.Error("!!! Cant get CampComponent in entity " + entity.Id() + ", component data init failed");
                        return;
                    }
                    var campMappingNode = (YamlMappingNode)n.Value;
                    ComponentInfoReader.ParseCamp(campMappingNode, null, campComp);
                }
                /*else if (key == "movement_data")
                {
                    var movementComp = entity.GetComponent<MovementComponent>();
                    if (movementComp == null)
                    {
                        DebugLog.Error("!!! Cant get MovementComponent in entity " + entity.Id() + ", component data init failed");
                        return;
                    }
                    var movementMappingNode = (YamlMappingNode)n.Value;
                    MovementCompDef compDef = null;
                    ComponentInfoReader.ParseMovement(movementMappingNode, ref compDef);
                    movementComp.Init(compDef);
                }*/
                /*else if (key == "aabb_data")
                {
                    var aabbComp = entity.GetComponent<AABBComponent>();
                    if (aabbComp == null)
                    {
                        DebugLog.Error("!!! Cant get AABBComponent in entity " + entity.Id() + " , component data init failed");
                        return;
                    }
                    var aabbMappingNode = (YamlMappingNode)n.Value;
                    ComponentInfoReader.ParseAABB(aabbMappingNode, null, aabbComp);
                }*/
                else if (key == "collider_data")
                {
                    var colliderComp = entity.GetComponent<ColliderComponent>();
                    if (colliderComp == null)
                    {
                        DebugLog.Error("!!! Cant get ColliderComponent in entity " + entity.Id() + " , component data init failed");
                        return;
                    }
                    var colliderMappingNode = (YamlMappingNode)n.Value;
                    ComponentInfoReader.ParseCollider(colliderMappingNode, null, colliderComp);
                }
            }
        }
    }
}