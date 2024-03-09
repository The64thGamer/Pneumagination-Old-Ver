using Godot;
using System.Collections.Generic;

/// <summary>
/// Draw line debug. Add to AutoLoad, call using DrawLine3D.Instance
/// </summary>
public partial class DrawLine3D : Node2D
{
    // Forked from https://github.com/klaykree/Godot-3D-Lines
    // Forked again from https://github.com/Rytelier/Godot-3D-Lines-CSharp

    static DrawLine3D instance;

    public static DrawLine3D Instance
    {
        get => instance;
    }

    public DrawLine3D()
    {
        instance = this;
    }

    public List<Line> lines = new List<Line>();
    public bool removedLine = false;

    public bool enabled = true;

    public override void _Process(double delta)
    {
#if DEBUG
        if (!enabled) return;

        for (int i = 0; i < lines.Count; i++)
        {
            lines[i].time -= delta;
        }

        if (lines.Count > 0 || removedLine)
        {
            QueueRedraw();
            removedLine = false;
        }
#endif
    }

    public override void _Draw()
    {
#if DEBUG
        // GD.Print("Draw called");

        if (!enabled) return;

        var Cam = GetViewport().GetCamera3D();
        for (int i = 0; i < lines.Count; i++)
        {
            var ScreenPointStart = Cam.UnprojectPosition(lines[i].start);
            var ScreenPointEnd = Cam.UnprojectPosition(lines[i].end);

            // Dont draw line if either start || end is considered behind the camera
            // this causes the line to not be drawn sometimes but avoids a bug where the
            // line is drawn incorrectly
            // TODO:  This is likely because the line properties are determined by unprojecting to the 2D viewport.
            // So, if part of the line is behind the camera, that would likely cause the problem.
            // A fix might be to truncate the line's length in this case such that it only goes to the edge of the viewport.
            if (Cam.IsPositionBehind(lines[i].start) || Cam.IsPositionBehind(lines[i].end)) continue;

            DrawLine(ScreenPointStart, ScreenPointEnd, lines[i].lineColor, lines[i].width);
        }

        //Remove lines that have timed out
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].time < 0.0)
            {
                lines.RemoveAt(i);
                removedLine = true;
            }
        }
#endif
    }

    public void DrawLine(Vector3 start, Vector3 end, Color? color = null, float time = 0, float width = -1)
    {
#if DEBUG
        Color col = color == null ? new Color(1, 1, 1, 1) : (Color)color;
        if (!enabled) return;
        lines.Add(new Line(start, end, col, time, width));
#endif
    }

    public void DrawRay(Vector3 start, Vector3 ray, Color? color = null, float time = 0, float width = -1)
    {
#if DEBUG
        Color col = color == null ? new Color(1, 1, 1, 1) : (Color)color;
        if (!enabled) return;
        lines.Add(new Line(start, start + ray, col, time, width));
#endif
    }

    public void DrawCapsuleRay(Vector3 origin, float radius, Vector3 direction, float distance, Color? color = null, float time = 0, int density = 8)
    {
#if DEBUG
        Color col = color == null ? new Color(1, 1, 1, 1) : (Color)color;

        Vector3 axis = direction.Cross(Vector3.Up).Normalized();
        Vector3 axisR = direction.Cross(Vector3.Forward).Normalized();
        Vector3 dirCircle = axis == Vector3.Zero ? Vector3.Zero : direction.Normalized().Rotated(axis, Mathf.DegToRad(90)).Normalized();
        Vector3 dirCircleR = axisR == Vector3.Zero ? Vector3.Zero : direction.Normalized().Rotated(axisR, Mathf.DegToRad(90)).Normalized();
        Vector3 axisLoop = dirCircle.Cross(dirCircleR).Normalized();

        Vector3 tip = origin - direction.Normalized() * radius;
        Vector3 tip2 = origin + direction.Normalized() * distance + direction.Normalized() * radius;

        for (int i = 0; i < density; i++)
        {
            Vector3 dirCircleLoop = axisLoop == Vector3.Zero ? Vector3.Zero : dirCircle.Rotated(axisLoop, Mathf.DegToRad(i * (360 / density)));
            Vector3 dirCircleLoopPrev = axisLoop == Vector3.Zero ? Vector3.Zero : dirCircle.Rotated(axisLoop, Mathf.DegToRad((i - 1) * (360 / density)));

            Vector3 loopEnd = origin + dirCircleLoop * radius;
            Vector3 loopEndPrev = origin + dirCircleLoopPrev * radius;

            //Rays along cylinder
            DrawRay(loopEnd, direction.Normalized() * distance, col, time);

            //Rays connecting cylinder cap
            // Vector3 toPrev = MathExtend.RayTarget(loopEnd, loopEndPrev);
            // DrawRay(loopEnd, toPrev, col, time);
            // DrawRay(origin + direction.Normalized() * distance + dirCircleLoop * radius, toPrev, col, time);

            //Rays to the tip
            DrawLine(loopEnd, tip, col, time);
            DrawLine(origin + direction.Normalized() * distance + dirCircleLoop * radius, tip2, col, time);
        }
#endif
    }

    public void DrawCube(Vector3 center, float halfExtents, Color? color = null, float time = 0)
    {
#if DEBUG
        Color col = color == null ? new Color(1, 1, 1, 1) : (Color)color;
        if (!enabled) return;
        //Start at the 'top left'
        Vector3 LinePointStart = center;
        LinePointStart.X -= halfExtents;
        LinePointStart.Y += halfExtents;
        LinePointStart.Z -= halfExtents;

        //Draw top square
        var LinePointEnd = LinePointStart + new Vector3(0, 0, halfExtents * 2);
        DrawLine(LinePointStart, LinePointEnd, col, time);
        LinePointStart = LinePointEnd;
        LinePointEnd = LinePointStart + new Vector3(halfExtents * 2, 0, 0);
        DrawLine(LinePointStart, LinePointEnd, col, time);
        LinePointStart = LinePointEnd;
        LinePointEnd = LinePointStart + new Vector3(0, 0, -halfExtents * 2);
        DrawLine(LinePointStart, LinePointEnd, col, time);
        LinePointStart = LinePointEnd;
        LinePointEnd = LinePointStart + new Vector3(-halfExtents * 2, 0, 0);
        DrawLine(LinePointStart, LinePointEnd, col, time);

        //Draw bottom square
        LinePointStart = LinePointEnd + new Vector3(0, -halfExtents * 2, 0);
        LinePointEnd = LinePointStart + new Vector3(0, 0, halfExtents * 2);
        DrawLine(LinePointStart, LinePointEnd, col, time);
        LinePointStart = LinePointEnd;
        LinePointEnd = LinePointStart + new Vector3(halfExtents * 2, 0, 0);
        DrawLine(LinePointStart, LinePointEnd, col, time);
        LinePointStart = LinePointEnd;
        LinePointEnd = LinePointStart + new Vector3(0, 0, -halfExtents * 2);
        DrawLine(LinePointStart, LinePointEnd, col, time);
        LinePointStart = LinePointEnd;
        LinePointEnd = LinePointStart + new Vector3(-halfExtents * 2, 0, 0);
        DrawLine(LinePointStart, LinePointEnd, col, time);

        //Draw vertical lines
        LinePointStart = LinePointEnd;
        DrawRay(LinePointStart, new Vector3(0, halfExtents * 2, 0), col, time);
        LinePointStart += new Vector3(0, 0, halfExtents * 2);
        DrawRay(LinePointStart, new Vector3(0, halfExtents * 2, 0), col, time);
        LinePointStart += new Vector3(halfExtents * 2, 0, 0);
        DrawRay(LinePointStart, new Vector3(0, halfExtents * 2, 0), col, time);
        LinePointStart += new Vector3(0, 0, -halfExtents * 2);
        DrawRay(LinePointStart, new Vector3(0, halfExtents * 2, 0), col, time);
#endif
    }
}

public class Line
{
    public Vector3 start;
    public Vector3 end;
    public Color lineColor;
    public double time;
    public float width;

    public Line(Vector3 _start, Vector3 _end, Color _lineColor, float _time, float _width)
    {
        this.start = _start;
        this.end = _end;
        this.lineColor = _lineColor;
        this.time = _time;
        this.width = _width;
    }
}