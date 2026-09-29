using System;

[Serializable]
public class ClickData
{
    public float timeMs;
    public float x;
    public float y;
    public bool hit;

    public ClickData(float timeMs, float x, float y, bool hit)
    {
        this.timeMs = timeMs;
        this.x = x;
        this.y = y;
        this.hit = hit;
    }
}
