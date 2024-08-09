using Logic.Entity;
using UnityEngine;

public class PlayableTank : PlayableMoveEntity
{
  protected Animator[] tracks_;
  protected Vector3 gravity_;

  protected new void Start()
  {
    base.Start();
    gravity_ = new Vector3(0, 0, -9.81f);
  }

  public override void Attach(Entity entity, AssetConfig assetConfig)
  {
    base.Attach(entity, assetConfig);
    // 只在Track子游戏对象GameObject有动画组件时，这样用才没问题。
    // 如果还有其他子游戏对象有动画组件，则先获得此子游戏对象然后再获得动画组件 
    tracks_ = gameObject.GetComponentsInChildren<Animator>();
    foreach (var t in tracks_)
    {
      t.enabled = false;
    }
  }

  public override void Detach()
  {
    base.Detach();
    tracks_ = null;
  }

  protected void FixedUpdate()
  {
    Physics.gravity = gravity_;
  }

  protected void OnDestroy()
  {
    if (hpBarObj_ != null)
    {
      playableContext_.RecycleUiGameObject(hpBarObj_);
    }
  }

  protected override void OnMoveHandle()
  {
    base.OnMoveHandle();
    PlayTrack();
  }

  protected override void OnStopMoveHandle()
  {
    base.OnStopMoveHandle();
    foreach (var t in tracks_)
    {
      t.enabled = false;
    }
  }

  protected virtual void PlayTrack()
  {

  }
}