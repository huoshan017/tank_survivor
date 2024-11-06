using System.Collections.Generic;
using System.IO;
using Common;
using YamlDotNet.RepresentationModel;

public struct PrefabInfo
{
    public string Name;
    public string Path;
}

public class PrefabConfigReader
{
    readonly Dictionary<string, PrefabInfo> pfxName2Info_;
    readonly List<PrefabInfo> pfxInfoList_;
    readonly Dictionary<string, PrefabInfo> uiName2Info_;
    readonly List<PrefabInfo> uiInfoList_;

    public PrefabConfigReader()
    {
        pfxName2Info_ = new();
        pfxInfoList_ = new();
        uiName2Info_ = new();
        uiInfoList_ = new();
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
        var prefabsNode = (YamlMappingNode)rootNode[new YamlScalarNode("prefabs")];
        if (prefabsNode == null)
        {
            DebugLog.Error("cant get node \"prefabs\" from file " + filePath);
            return false;
        }

        foreach (var mn in prefabsNode.Children)
        {
            var key = ((YamlScalarNode)mn.Key).Value;
            if (key == "pfx_list")
            {
                var sequenceNode = (YamlSequenceNode)mn.Value;
                ParsePrefabInfoList(sequenceNode, pfxName2Info_, pfxInfoList_);
            }
            else if (key == "ui_list")
            {
                var sequenceNode = (YamlSequenceNode)mn.Value;
                ParsePrefabInfoList(sequenceNode, uiName2Info_, uiInfoList_);
            }
        }

        return true;
    }

    public bool GetPfx(string name, out PrefabInfo prefabInfo)
    {
        if (!pfxName2Info_.TryGetValue(name, out prefabInfo))
        {
            DebugLog.Error("pfx name " + name + " can not get prefab info");
            return false;
        }
        return true;
    }

    public List<PrefabInfo> GetPfxList()
    {
        return pfxInfoList_;
    }

    public bool GetUi(string name, out PrefabInfo prefabInfo)
    {
        if (!uiName2Info_.TryGetValue(name, out prefabInfo))
        {
            DebugLog.Error("ui name " + name + " cant get prefab info");
            return false;
        }
        return true;
    }

    public List<PrefabInfo> GetUiList()
    {
        return uiInfoList_;
    }

    void ParsePrefabInfoList(YamlSequenceNode sequenceNode, Dictionary<string, PrefabInfo> dictInfo, List<PrefabInfo> listInfo)
    {
        foreach (var sn in sequenceNode.Children)
        {
            var prefabInfo = new PrefabInfo();
            var snNode = (YamlMappingNode)sn;
            foreach (var snn in snNode.Children)
            {
                var snKey = ((YamlScalarNode)snn.Key).Value;
                if (snKey == "name")
                {
                    prefabInfo.Name = snn.Value.ToString();
                }
                else if (snKey == "path")
                {
                    prefabInfo.Path = snn.Value.ToString();
                }
            }
            dictInfo.Add(prefabInfo.Name, prefabInfo);
            listInfo.Add(prefabInfo);
        }
    }
}