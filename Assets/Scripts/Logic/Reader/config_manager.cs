using Common;
using Logic.Base;
using Utils;

namespace Logic.Reader
{
  public class ConfigManager
    {
        static EntityInfoReader entityInfoReader_;
        static CampInfoReader campInfoReader_;
        static RoleList roleList_;
        static PlayerConfig playerConfig_;

        public static bool LoadAll()
        {
            // entity def load
            string filePath = "Configuration/EntityDef.yaml";
            string filePath2 = "Configuration/ProjectileDef.yaml";
            string filePath3 = "Configuration/BotEntityDef.yaml";
            var filePathList = new string[]{filePath, filePath2, filePath3};
            var fullPathList = new string[filePathList.Length];
            for (int i=0; i<filePathList.Length; i++)
            {
                var path = Accessory.GetStreamingAssetsFullPath(filePathList[i]);
                if (path == null)
                {
                    DebugLog.Error("Path " + path + " StreamingAssetsContent is null");
                    return false;
                }
                fullPathList[i] = path;
            }
            entityInfoReader_ = new EntityInfoReader();
            if (!entityInfoReader_.LoadMulti(fullPathList))
            {
                DebugLog.Error("Load entity def yaml configure file failed");
                return false;
            }

            // camp info load
            filePath = "Configuration/CampInfo.yaml";
            var fullPath = Accessory.GetStreamingAssetsFullPath(filePath);
            campInfoReader_ = new CampInfoReader();
            if (!campInfoReader_.Load(fullPath))
            {
                DebugLog.Error("Load camp info yaml configure file failed");
                return false;
            }

            // role list load
            filePath = "Configuration/Roles.yaml";
            fullPath = Accessory.GetStreamingAssetsFullPath(filePath);
            var roleListReader = new RoleReader();
            if (!roleListReader.LoadFile(fullPath, out roleList_))
            {
                DebugLog.Error("Load role info yaml configure file failed");
                return false;
            }

            // player config load
            filePath = "Configuration/PlayerConfig.yaml";
            fullPath = Accessory.GetStreamingAssetsFullPath(filePath);
            var playerConfigReader = new PlayerConfigReader();
            if (!playerConfigReader.Load(fullPath, out playerConfig_))
            {
                DebugLog.Error("Load player config yaml configure file failed");
                return false;
            }

            return true;
        }

        public static EntityDef GetEntityDef(int entityId)
        {
            return entityInfoReader_.GetEntity(entityId);
        }

        public static CampRelation GetRelation(CampType campType1, CampType campType2)
        {
            return campInfoReader_.RelationOfTwoCamp(campType1, campType2);
        }

        public static CampRelation[] GetRelations(CampType campType)
        {
            return campInfoReader_.Relations(campType);
        }

        public static RoleList GetRoleList()
        {
            return roleList_;
        }

        public static PlayerConfig GetPlayerConfig()
        {
            return playerConfig_;
        }
    }
}