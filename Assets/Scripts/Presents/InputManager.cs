using System.Collections.Generic;
using System.Linq;
using Core;
using Logic.Base;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class InputManager : MonoBehaviour
{
    readonly KeyValuePair<Key, CmdData>[] keyDown2CmdList_ = new KeyValuePair<Key, CmdData>[]
    {
        new(Key.W, new CmdData() { Cmd = CommandDefine.CmdMove, Args = new long[]{ 90 } } ), // 向上移动
        new(Key.S, new CmdData() { Cmd = CommandDefine.CmdMove, Args = new long[]{ 270 } } ), // 向下移动
        new(Key.A, new CmdData() { Cmd = CommandDefine.CmdMove, Args = new long[]{ 180 } } ), // 向左移动
        new(Key.D, new CmdData() { Cmd = CommandDefine.CmdMove, Args = new long[]{ 0 } } ), // 向右移动
    };

    readonly KeyValuePair<Key[], CmdData>[] keysDown2CmdList_ = new KeyValuePair<Key[], CmdData>[]
    {
        new(new Key[]{ Key.W, Key.D }, new CmdData() { Cmd = CommandDefine.CmdMove, Args = new long[]{ 45 } } ), // 右上移动
        new(new Key[]{ Key.S, Key.D }, new CmdData() { Cmd = CommandDefine.CmdMove, Args = new long[]{ 315 } } ), // 右下移动
        new(new Key[]{ Key.A, Key.W }, new CmdData() { Cmd = CommandDefine.CmdMove, Args = new long[]{ 135 } } ), // 左上移动
        new(new Key[]{ Key.A, Key.S }, new CmdData() { Cmd = CommandDefine.CmdMove, Args = new long[]{ 225 } } ), // 左下移动
    };

    readonly KeyValuePair<Key, CmdData>[] keyUp2CmdList_ = new KeyValuePair<Key, CmdData>[]
    {
        new( Key.W, new CmdData() { Cmd = CommandDefine.CmdStopMove } ),
        new( Key.S, new CmdData() { Cmd = CommandDefine.CmdStopMove } ),
        new( Key.A, new CmdData() { Cmd = CommandDefine.CmdStopMove } ),
        new( Key.D, new CmdData() { Cmd = CommandDefine.CmdStopMove } ),
    };

    readonly Keyboard keyboard_ = Keyboard.current;

    readonly Mouse mouse_ = Mouse.current;

    GameManager gameManager_;

    readonly long[] logicWorldCoord_;
    readonly Key[] cacheKeys_;

    public InputManager()
    {
        logicWorldCoord_ = new long[2];
        cacheKeys_ = new Key[2];
    }

    public void Update()
    {
        byte n = 0, kn = 0;
        for (int i=0; i<keyDown2CmdList_.Length; i++)
        {
            var key = keyDown2CmdList_[i].Key;
            if (keyboard_[key].isPressed)
            {
                cacheKeys_[kn] = key;
                kn += 1;
                n = (byte)i;
                if (kn >= 2) break;
            }
        }

        if (kn == 1)
        {
            gameManager_.PushSyncPlayerCmd(GlobalConstant.DefaultSinglePlayerId, keyDown2CmdList_[n].Value);
        }
        else if (kn == 2)
        {
            for (int i=0; i<keysDown2CmdList_.Length; i++)
            {
                var pair = keysDown2CmdList_[i];
                if (pair.Key.Contains(cacheKeys_[0]) && pair.Key.Contains(cacheKeys_[1]))
                {
                    gameManager_.PushSyncPlayerCmd(GlobalConstant.DefaultSinglePlayerId, pair.Value);
                    break;
                }
            }
        }

        foreach (var pair in keyUp2CmdList_)
        {
            if (keyboard_[pair.Key].wasReleasedThisFrame)
            {
                gameManager_.PushSyncPlayerCmd(GlobalConstant.DefaultSinglePlayerId, pair.Value);
            }
        }
        
        var mousePos = mouse_.position.ReadValue();
        {
            var worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            var posX_ = (long)(worldPos.x * GlobalConstant.LogicAndUnityRatio);
            var posZ_ = (long)(worldPos.z * GlobalConstant.LogicAndUnityRatio); // 3d坐标系的z坐标对应逻辑坐标系的y坐标
            logicWorldCoord_[0] = posX_;
            logicWorldCoord_[1] = posZ_;
            // 输入头部朝向的命令
            gameManager_.PushSyncPlayerCmd(GlobalConstant.DefaultSinglePlayerId, new CmdData
            {
                Cmd = CommandDefine.CmdHeadForward,
                Args = logicWorldCoord_
            });
        }
        
        if (mouse_.leftButton.wasPressedThisFrame)
        {
            gameManager_.PushSyncPlayerCmd(GlobalConstant.DefaultSinglePlayerId, new CmdData{ Cmd=CommandDefine.CmdFire });
        }
        if (mouse_.leftButton.wasReleasedThisFrame)
        {
            gameManager_.PushSyncPlayerCmd(GlobalConstant.DefaultSinglePlayerId, new CmdData{ Cmd=CommandDefine.CmdStopFire });
        }

        if (Camera.main.orthographic)
        {
          Camera.main.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * 10;
        }
        else
        {
          Camera.main.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * 10;
        }
    }

    public void Attach(GameManager gameManager)
    {
        gameManager_ = gameManager;
    }
}