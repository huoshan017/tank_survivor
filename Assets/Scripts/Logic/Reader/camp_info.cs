using System.Collections.Generic;
using System.IO;
using Common;
using Logic.Base;
using YamlDotNet.RepresentationModel;

namespace Logic.Reader
{
    public class CampInfoReader
    {
        readonly CampRelation[,] campRelationTable_;
        readonly Dictionary<CampType, CampRelation[]> relationDict_;

        public CampInfoReader()
        {
            campRelationTable_ = new CampRelation[(int)CampType.Max, (int)CampType.Max];
            relationDict_ = new();
        }

        public bool Load(string filePath)
        {
            var reader = new StreamReader(filePath);
            var yaml = new YamlStream();
            yaml.Load(reader);
            var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
            if (rootNode == null)
            {
                DebugLog.Error("Cant get root node");
                return false;
            }

            YamlSequenceNode campRelationListNode = (YamlSequenceNode)rootNode.Children[new YamlScalarNode("camp_relations")];
            if (campRelationListNode == null)
            {
                DebugLog.Error("Root node must sequence node");
                return false;
            }

            foreach (var n in campRelationListNode.Children)
            {
                var campRelationNode = (YamlMappingNode)n;
                var typeNode = campRelationNode[new YamlScalarNode("type")];
                if (typeNode == null)
                {
                    DebugLog.Error("Cant fetch \'type\' node from " + filePath);
                    return false;
                }
                CampType ct = (CampType)int.Parse(typeNode.ToString());
                var relationListNode = (YamlSequenceNode)campRelationNode[new YamlScalarNode("relation_list")];
                if (relationListNode == null)
                {
                    DebugLog.Error("Cant fetch \'relation_list\' node from " + filePath);
                    return false;
                }
                for (int i=0; i<relationListNode.Children.Count; i++)
                {
                    campRelationTable_[(int)ct, i] = (CampRelation)int.Parse(relationListNode.Children[i].ToString());
                }
            }
            return true;
        }

        public CampRelation RelationOfTwoCamp(CampType ct1, CampType ct2)
        {
            return campRelationTable_[(int)ct1, (int)ct2];
        }

        public CampRelation[] Relations(CampType camp)
        {
            if (!relationDict_.TryGetValue(camp, out var relations))
            {
                relations = new CampRelation[(int)CampType.Max];
                for (int i=(int)CampType.One; i<(int)CampType.Max; i++)
                {
                    relations[i] = campRelationTable_[(int)camp, i];
                }
                relationDict_.Add(camp, relations);
            }
            return relations;
        }
    }
}