using Common;
using Common.Geometry;
using UnityEngine;

public class RotateTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        center_ = new Position(1000, 1000);
        radius_ = 2000;
        angularVelocity_ = 50;
        lastPos_ = center_;
        lastPos_.Translate(radius_, 0);
    }

    // Update is called once per frame
    void Update()
    {
        var minutes = (short)(angularVelocity_*60*Time.deltaTime);
        currAngle_.AddMinutes(minutes);
        var pos = center_; pos.Translate(radius_, 0);
        //var x = pos.X(); var y = pos.Y();
        //var m = currAngle_.ToMinutes();
        //MathUtils.Rotate(ref x, ref y, center_.X(), center_.Y(), (float)m/60);
        //pos.Set(x, y);
        pos.Rotate(center_.X(), center_.Y(), currAngle_);
        var start = new Vector2(lastPos_.X(), lastPos_.Y())/GlobalConstant.LogicAndUnityRatio;
        var end = new Vector2(pos.X(), pos.Y())/GlobalConstant.LogicAndUnityRatio;
        lastPos_ = pos;
        Debug.DrawLine(start, end, Color.red);
        Debug.Log("start: " + start.x + ", " + start.y + "   end: " + end.x + ", " + end.y);
        Debug.Log("lastPos_: " + lastPos_.X() + ", " + lastPos_.Y());
    }

    Position lastPos_;
    public Position center_;
    public int radius_;

    public int angularVelocity_; // 每秒度数

    Angle currAngle_;
}
