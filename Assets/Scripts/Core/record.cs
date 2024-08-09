using System.Collections.Generic;
using System.IO;
using Common;
using Logic.Base;
using Logic;
using Utils;
using ProtoBuf;

namespace Core
{
    public class Record
    {
        internal string name_;
        internal int mapId_;
        internal List<ulong> playerIdList_;
        internal uint frameMs_;
        internal uint frameNum_;
        internal List<frameData> frameList_;

        internal void clear()
        {
            name_ = "";
            mapId_ = 0;
            playerIdList_?.Clear();
            frameMs_ = 0;
            frameNum_ = 0;
            if (frameList_ != null)
            {
                frameList_.Clear();
            }
        }
    }

    struct recordNamePair
    {
        internal string recordName_;
        internal string fileName_;
    }

    public class RecordManager
    {
        readonly Instance inst_;
        readonly List<recordNamePair> nameList_;
        int selIndex_;
        string savePath_;
        bool loaded_;
        const string recordsDir = "records";
        const string recordFileSuffix = "record";

        public RecordManager(Instance inst)
        {
            inst_ = inst;
            selIndex_ = -1;
            genSavePath();
        }

        public void LoadRecords()
        {
            if (loaded_) return;
            DirectoryInfo dirInfo = new(savePath_);
            var fileList = dirInfo.GetFiles("*." + recordFileSuffix);
            for (int i=0; i<fileList.Length; i++)
            {
                var record = readPbRecord(fileList[i].Name);
                if (record == null)
                {
                    DebugLog.Error("RecordManager read file " + fileList[i].Name + " failed");
                }
                var pair = new recordNamePair
                {
                    recordName_ = record.Name,
                    fileName_ = fileList[i].Name
                };
                nameList_.Add(pair);
            }
            loaded_ = true;
        }

        public void Save(string mapName, int mapId)
        {
            var record = new Record
            {
                mapId_ = mapId,
                frameMs_ = inst_.frameMs_,
                name_ = "Record." + mapName + ": " + GTime.CurrTimeString(),
            };
            var fileName = GTime.CurrTimeString() + ".record";
            persistent(fileName, record);
            var recordPair = new recordNamePair
            {
                recordName_ = record.name_,
                fileName_ = fileName
            };
            nameList_.Add(recordPair);
        }

        public bool Delete(int index)
        {
            if (index >= inst_.frameList_.Count)
            {
                return false;
            }
            nameList_.RemoveAt(index);
            return true;
        }

        public void Select(int index)
        {
            if (index <= nameList_.Count)
            {
                selIndex_ = index;
            }
        }

        public Record SelectedRecord()
        {
            if (selIndex_ < 0)
            {
                DebugLog.Error("Cant select record");
                return null;
            }

            var namePair = nameList_[selIndex_];
            return readRecord(namePair.fileName_);
        }

        public int GetRecordCount()
        {
            return nameList_.Count;
        }

        public string GetRecordName(int index)
        {
            return nameList_[index].recordName_;
        }

        void genSavePath()
        {
            var dir = Accessory.GetCurrentDirectory();
            var savePath = dir + "/" + recordsDir;
            if (!Directory.Exists(savePath)) {
                Directory.CreateDirectory(savePath);
            }
            savePath_ = savePath;
        }

        PbRecord readPbRecord(string fileName)
        {
            var filePath = savePath_ + "/" + fileName;
            PbRecord record;
            using (var stream = File.OpenRead(filePath)) {
                record = Serializer.Deserialize<PbRecord>(stream);
            }
            return record;
        }

        Record readRecord(string fileName)
        {
            var pbRecord = readPbRecord(fileName);
            if (pbRecord == null) return null;
            var record = new Record();
            unserializeRecord(pbRecord, record);
            return record;
        }

        void persistent(string fileName, Record record)
        {
            var pbr = new PbRecord();
            serializeRecord(record, pbr);
            var filePath = savePath_ + "/" + fileName;
            using (FileStream stream = File.OpenWrite(filePath)) {
                Serializer.Serialize<PbRecord>(stream, pbr);
            }
        }

        void serializeRecord(Record record, PbRecord pbr)
        {
            pbr.Name = record.name_;
            pbr.MapId = record.mapId_;
            pbr.FrameMs = record.frameMs_;
            pbr.FrameNum = record.frameNum_;
            for (int i=0; i<record.frameList_.Count; i++)
            {
                var pbFrameData = new PbFrameData();
                var fd = record.frameList_[i];
                for (int j=0; j<fd.playerDataList.Count; j++)
                {
                    var pbPlayerFrame = new PbPlayerFrame();
                    var pd = fd.playerDataList[j];
                    for (int k=0; k<pd.cmdList.Count; k++)
                    {
                        var cmd = pd.cmdList[k];
                        var pbCmdData = new PbCmdData();
                        pbCmdData.Code = cmd.Cmd;
                        pbCmdData.Args = cmd.Args;
                        pbPlayerFrame.CmdLists.Add(pbCmdData);
                    }
                    pbPlayerFrame.PlayerId = pd.playerId;
                    pbFrameData.PlayerLists.Add(pbPlayerFrame);
                }
                pbFrameData.FrameNum = fd.frameNum;
                pbr.FrameLists.Add(pbFrameData);
            }
        }

        void unserializeRecord(PbRecord pbr, Record record)
        {
            record.name_ = pbr.Name;
            record.mapId_ = pbr.MapId;
            record.frameMs_ = pbr.FrameMs;
            record.frameNum_ = pbr.FrameNum;
            for (int i=0; i<pbr.FrameLists.Count; i++)
            {
                var pbFrameData = pbr.FrameLists[i];
                var fd = new frameData
                {
                    frameNum = pbFrameData.FrameNum
                };
                for (int j=0; j<pbFrameData.PlayerLists.Count; j++)
                {
                    var pbPlayerFrame = pbFrameData.PlayerLists[j];
                    var pd = new playerData();
                    pd.playerId = pbPlayerFrame.PlayerId;
                    for (int k=0; k<pbPlayerFrame.CmdLists.Count; k++)
                    {
                        var pbCmdData = pbPlayerFrame.CmdLists[k];
                        var cd = new CmdData();
                        cd.Cmd = pbCmdData.Code;
                        cd.Args = pbCmdData.Args;
                        pd.cmdList.Add(cd);
                    }
                    fd.playerDataList.Add(pd);
                    // 保证只添加一次
                    if (!record.playerIdList_.Contains(pd.playerId)){
                        record.playerIdList_.Add(pd.playerId);
                    }
                }
                record.frameList_.Add(fd);
            }
        }
    }
}