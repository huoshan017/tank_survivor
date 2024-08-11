using System;
using Common;
using Core;
using Logic.Reader;
using Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Main : MonoBehaviour
{
    void Awake()
    {
        DebugLog.SetDebug(new SDebugger());
        gameMgr_ = new GameManager(new GameArgs{PlayerNum=1, FrameMs=33});
        playableWorld_ = gameObject.AddComponent<PlayableWorld>();
        playableWorld_.Attach(gameMgr_.GetInst().GetGame().GetWorld());
        inputManager_ = gameObject.AddComponent<InputManager>();
        inputManager_.Attach(gameMgr_);
        Application.targetFrameRate = 120;
        #if UNITY_EDITOR
        EditorApplication.pauseStateChanged += OnPauseStateChanged;
        #endif
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!ConfigManager.LoadAll())
        {
            throw new Exception("ConfigManager LoadAll failed");
        }

        playableWorld_.Init();

        // TODO 临时代码，载入一个Map
        string mapFilePath = Accessory.GetStreamingAssetsFullPath("Maps/Map01.yaml");
        gameMgr_.RegisterAfterLoadMapHandle(playableWorld_.AfterLoadMapHandle);
        if (!gameMgr_.LoadMap(mapFilePath)) {
            throw new Exception("World LoadMap Failed");
        }

        gameMgr_.GetInst().GetGame().RegisterPlayerEnterHandle(PlayerEnterHandle);
        gameMgr_.GetInst().GetGame().RegisterPlayerLeaveHandle(PlayerLeaveHandle);

        var playerIdList = new ulong[]{ GlobalConstant.DefaultSinglePlayerId };
        gameMgr_.Start(playerIdList, false);
    }

    // Update is called once per frame
    void Update()
    {
        gameMgr_.Update();

        // 获取相机追踪的entity
        var gameObj = playableWorld_.GetEntityGameObject(localPlayerEntityId_);
        if (gameObj != null)
        {
            float originalZ = Camera.main.transform.position.z;
            float x = gameObj.transform.position.x;
            float y = gameObj.transform.position.y;
            // 设置相机位置
            Camera.main.transform.position = new Vector3(x, y, originalZ);
        }
    }

    void OnDestroy()
    {
        gameMgr_.End();
        gameMgr_.GetInst().GetGame().UnregisterPlayerLeaveHandle(PlayerLeaveHandle);
        gameMgr_.GetInst().GetGame().UnregisterPlayerEnterHandle(PlayerEnterHandle);
        gameMgr_.UnregisterAfterLoadMapHandle(playableWorld_.AfterLoadMapHandle);
        playableWorld_.Uninit();
        playableWorld_.Detach();
    }

    void PlayerEnterHandle(ulong playerId, uint entityInstId)
    {
        if (playerId != GlobalConstant.DefaultSinglePlayerId) return;
        localPlayerEntityId_ = entityInstId;
    }

    void PlayerLeaveHandle(ulong playerId, uint entityInstId)
    {
        localPlayerEntityId_ = 0;
    }

#if UNITY_EDITOR
    void OnPauseStateChanged(PauseState state)
    {
        if (state == PauseState.Paused)
        {
            gameMgr_.Pause();
            DebugLog.Info("game paused");
        }
        else
        {
            gameMgr_.Resume();
            DebugLog.Info("game resumed");
        }
    }
#endif

    GameManager gameMgr_;
    PlayableWorld playableWorld_;
    InputManager inputManager_;
    uint localPlayerEntityId_;
}
