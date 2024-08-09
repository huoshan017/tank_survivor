using Common;
using UnityEngine;



public class DebugDrawer
{
    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        Debug.DrawLine(start, end, color);
    }


    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
    {
        Debug.DrawLine(start, end, color, duration);
    }

    public static void DrawLine(Vector3 start, Vector3 end)
    {
        Debug.DrawLine(start, end);
    }

    public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration)
    {
        Debug.DrawRay(start, dir, color, duration);
    }

    public static void DrawRay(Vector3 start, Vector3 dir, Color color)
    {
        Debug.DrawRay(start, dir, color);
    }

    public static void DrawRay(Vector3 start, Vector3 dir)
    {
        Debug.DrawRay(start, dir);
    }
}

public class SDebugger : IDebugger
{
    public void Error(object message)
    {
        Debug.LogError(message);
    }

    public void Info(object message)
    {
        Debug.Log(message);
    }

    public void Warning(object message)
    {
        Debug.LogWarning(message);
    }
}