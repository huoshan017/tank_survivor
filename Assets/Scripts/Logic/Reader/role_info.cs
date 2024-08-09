using System.IO;
using Common;
using YamlDotNet.RepresentationModel;

namespace Logic.Reader
{
    public class RoleInfo
    {
        public int EntityId;
    }

    public class RoleList 
    {
        internal RoleInfo[] roleList_;

        public int Length()
        {
            return roleList_.Length;
        }

        public RoleInfo GetAt(int index)
        {
            return roleList_[index];
        }
    }

    public class RoleReader
    {
        public bool LoadFile(string filePath, out RoleList roleList)
        {
            var reader = new StreamReader(filePath);
            var yaml = new YamlStream();
            yaml.Load(reader);
            var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
            if (rootNode == null)
            {
                DebugLog.Error("Cant get root node");
                roleList = null;
                return false;
            }

            YamlSequenceNode roleListNode = (YamlSequenceNode)rootNode.Children[new YamlScalarNode("roles")];
            if (roleListNode == null)
            {
                DebugLog.Error("Root node must sequence node");
                roleList = null;
                return false;
            }

            roleList = new();
            roleList.roleList_ = new RoleInfo[roleListNode.Children.Count];
            for (int i=0; i<roleListNode.Children.Count; i++)
            {
                var roleNode = (YamlMappingNode)roleListNode.Children[i];
                var entityIdNode = roleNode[new YamlScalarNode("entity_id")];
                if (entityIdNode == null)
                {
                    DebugLog.Error("Cant fetch \'entity_id\' node from " + filePath);
                    roleList = null;
                    return false;
                }
                int entityId = int.Parse(entityIdNode.ToString());
                roleList.roleList_[i] = new();
                roleList.roleList_[i].EntityId = entityId;
            }

            return true;
        }
    }
}