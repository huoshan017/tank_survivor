using System.IO;
using Common;
using YamlDotNet.RepresentationModel;

namespace Logic.Reader
{
    public class PlayerConfig
    {
        public int StartRoleCount;
        public int DefaultRoleIndex;
        public string AdditionalComponent;
    }
    
    public class PlayerConfigReader
    {
        public bool Load(string filePath, out PlayerConfig playerConfig)
        {
            var reader = new StreamReader(filePath);
            var yaml = new YamlStream();
            yaml.Load(reader);
            var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
            if (rootNode == null)
            {
                DebugLog.Error("Cant get root node");
                playerConfig = null;
                return false;
            }

            YamlMappingNode playerConfigNode = (YamlMappingNode)rootNode.Children[new YamlScalarNode("player_config")];
            if (playerConfigNode == null)
            {
                DebugLog.Error("Root node must mapping node");
                playerConfig = null;
                return false;
            }

            playerConfig = new();
            foreach (var n in playerConfigNode.Children)
            {
                string key = n.Key.ToString();
                if (key == "start_role_count")
                {
                    var startRoleCount = int.Parse(n.Value.ToString());
                    playerConfig.StartRoleCount = startRoleCount;
                }
                else if (key == "default_role_index")
                {
                    playerConfig.DefaultRoleIndex = int.Parse(n.Value.ToString());
                }
                else if (key == "additional_component")
                {
                    playerConfig.AdditionalComponent = n.Value.ToString();
                    DebugLog.Info("Player Additional Component: " + playerConfig.AdditionalComponent);
                }
            }

            return true;
        }
    }
}