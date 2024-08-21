using System;
using Common;
using Common.Geometry;
using Logic.Interface;
using Logic.Base;

namespace Logic.Component
{
  public class TransformCompDef : CompDef
  {
    public Position Pos;
    public Angle Rotation;
    public Angle OrientationRotationDiff; // 朝向和旋转的差值

    public override IComponent Create(IComponentContainer container)
    {
      return new TransformComponent(container);
    }

    public override Type GetCompType()
    {
      return typeof(TransformComponent);
    }
  }

  public class TransformComponent : BaseComponent
  {
    TransformCompDef compDef_;
    Position pos_, lastPos_; // 位置
    Angle rotation_; // 旋转
    Angle orientationRotationDiff_; // 朝向和旋转的角度差
    IEntity parent_; // 父实体
    TransformComponent parentTransform_; // 父实体变换组件

    public TransformComponent(IComponentContainer container) : base(container)
    {
    }

    public override void Init(CompDef compDef)
    {
      compDef_ = (TransformCompDef)compDef;
      pos_ = compDef_.Pos;
      rotation_ = compDef_.Rotation;
      orientationRotationDiff_ = compDef_.OrientationRotationDiff;
      CheckAndSetParent();
      lastPos_ = pos_;
    }

    public override void Uninit()
    {
    }

    public override void Update(uint frameMs)
    {
      if (lastPos_ != pos_)
      {
        lastPos_ = pos_;
      }
    }

    public void Rotate(Angle angle)
    {
      rotation_.Add(angle);
    }

    // 旋转到本地坐标系的angle角度
    public void RotateTo(Angle angle)
    {
      angle.Normalize();
      rotation_ = angle;
    }

    // 旋转到世界坐标系的angle角度
    public void WorldRotateTo(Angle angle)
    {
      var angleDiff = GetWorldRotationDiff();
      angle.Sub(angleDiff);
      rotation_ = angle;
    }

    // 朝向本地坐标系的angle角度
    public void OrientationTo(Angle angle)
    {
      RotateTo(angle);
      rotation_.Sub(orientationRotationDiff_);
    }

    // 朝向世界坐标系的angle角度
    public void WorldOrientationTo(Angle angle)
    {
      WorldRotateTo(angle);
      rotation_.Sub(orientationRotationDiff_);
    }

    // 本地朝向到世界朝向
    public Angle Orientation2Rotation(Angle orientation)
    {
      orientation.Sub(orientationRotationDiff_);
      return  orientation;
    }

    // 从世界朝向获得世界旋转
    public Angle GetWorldRotationFromWorldOrientation(Angle worldOrientation)
    {
      return Angle.Sub(worldOrientation, orientationRotationDiff_);
    }

    public Position Local2World(Position localPos)
    {
      var worldPos = WorldPos;
      var newPos = worldPos;
      newPos.Translate(localPos.X(), localPos.Y());
      newPos.Rotate(worldPos.X(), worldPos.Y(), Angle.Add(parentTransform_.rotation_, rotation_));
      return newPos;
    }

    public Angle RotateGetRotationResult(int rotateSpeed, uint rotateMs, Angle targetDir, bool isWorld = false)
    {
      Vec2 vec1;
      Angle orientation;
      if (isWorld)
      {
        vec1 = WorldOrientation.ToVec2();
        orientation = WorldOrientation;
      }
      else
      {
        vec1 = Orientation.ToVec2();
        orientation = Orientation;
      }

      Vec2 vec2 = targetDir.ToVec2();
      var result = Vec2.Cross(vec1, vec2);
      var degree = (short)(rotateSpeed * rotateMs / 1000);
      Angle s;
      if (result > 0)
      {
        s = Angle.Sub(targetDir, orientation);
        if (degree * 60 >= s.ToMinutes())
        {
          if (isWorld)
          {
            s = Orientation2Rotation(targetDir);
          }
          else
          {
            s = Orientation2Rotation(targetDir);
          }
        }
        else
        {
          if (isWorld)
          {
            s = Orientation2Rotation(Angle.Add(orientation, new Angle(degree, 0)));
          }
          else
          {
            s = Orientation2Rotation(Angle.Add(orientation, new Angle(degree, 0)));
          }
        }
      }
      else
      {
        s = Angle.Sub(orientation, targetDir);
        if (degree * 60 >= s.ToMinutes())
        {
          if (isWorld)
          {
            s = Orientation2Rotation(targetDir);
          }
          else
          {
            s = Orientation2Rotation(targetDir);
          }
        }
        else
        {
          if (isWorld)
          {
            s = Orientation2Rotation(Angle.Add(orientation, new Angle((short)-degree, 0)));
          }
          else
          {
            s = Orientation2Rotation(Angle.Add(orientation, new Angle((short)-degree, 0)));
          }
        }
      }
      return s;
    }

    public Angle RotateProcess(int rotateSpeed, uint frameMs, Angle targetDir, bool isWorld = false)
    {
      Vec2 vec1;
      Angle orientation;
      if (isWorld)
      {
        vec1 = WorldOrientation.ToVec2();
        orientation = WorldOrientation;
      }
      else
      {
        vec1 = Orientation.ToVec2();
        orientation = Orientation;
      }

      Vec2 vec2 = targetDir.ToVec2();
      var result = Vec2.Cross(vec1, vec2);
      var degree = (short)(rotateSpeed * frameMs / 1000);
      Angle s;
      if (result > 0)
      {
        s = Angle.Sub(targetDir, orientation);
        if (degree * 60 >= s.ToMinutes())
        {
          if (isWorld)
          {
            WorldOrientationTo(targetDir);
          }
          else
          {
            OrientationTo(targetDir);
          }
        }
        else
        {
          if (isWorld)
          {
            WorldOrientationTo(Angle.Add(orientation, new Angle(degree, 0)));
          }
          else
          {
            OrientationTo(Angle.Add(orientation, new Angle(degree, 0)));
          }
        }
      }
      else
      {
        s = Angle.Sub(orientation, targetDir);
        if (degree * 60 >= s.ToMinutes())
        {
          if (isWorld)
          {
            WorldOrientationTo(targetDir);
          }
          else
          {
            OrientationTo(targetDir);
          }
        }
        else
        {
          if (isWorld)
          {
            WorldOrientationTo(Angle.Add(orientation, new Angle((short)-degree, 0)));
          }
          else
          {
            OrientationTo(Angle.Add(orientation, new Angle((short)-degree, 0)));
          }
        }
      }
      return s;
    }

    public TransformCompDef CompDef
    {
      get => compDef_;
    }

    public Position Pos
    {
      get => pos_;
      set => pos_ = value;
    }

    public Position LastPos
    {
      get => lastPos_;
      set => lastPos_ = value;
    }

    public Position WorldPos
    {
      get
      {
        CheckAndSetParent();
        if (parent_ == null) return pos_;
        Position pos = parentTransform_.Pos;
        var wpos = pos;
        wpos.Translate(pos_.X(), pos_.Y());
        // 旋转
        var rotation = parentTransform_.rotation_;
        wpos.Rotate(pos.X(), pos.Y(), rotation);
        return wpos;
      }
    }

    public Angle Rotation
    {
      get => rotation_;
      set => rotation_ = value;
    }

    public Angle WorldRotation
    {
      get
      {
        CheckAndSetParent();
        if (parent_ == null) return rotation_;
        return Angle.Add(rotation_, parentTransform_.rotation_);
      }
    }

    public Angle OrientationRotationDiff
    {
      get => orientationRotationDiff_;
      set => orientationRotationDiff_ = value;
    }

    public Angle Orientation
    {
      get
      {
        return Angle.Add(rotation_, orientationRotationDiff_);
      }
    }

    public Angle WorldOrientation
    {
      get => Angle.Add(WorldRotation, orientationRotationDiff_);
    }

    Angle GetWorldRotationDiff()
    {
      Angle angle = new();
      IEntity entity = (IEntity)container_;
      var parent = entity.Parent;
      while (parent != null)
      {
        var transformComp = parent.GetComponent<TransformComponent>();
        angle.Add(transformComp.rotation_);
        parent = parent.Parent;
      }
      return angle;
    }

    void CheckAndSetParent()
    {
      parent_ = ((IEntity)container_).Parent;
      if (parent_ != null)
      {
        parentTransform_ = parent_.GetComponent<TransformComponent>();
      }
    }
  }
}