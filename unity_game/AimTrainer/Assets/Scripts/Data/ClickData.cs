using System;

[Serializable]
public class ClickData
{
    public float timeMs;
    public float posX;
    public float posY;
    public bool hit;
    public int? targetId;

    public ClickData(float timeMs, float posX, float posY, bool hit, int? targetId)
    {
        this.timeMs = timeMs;
        this.posX = posX;
        this.posY = posY;
        this.hit = hit;
        this.targetId = targetId; 
    }
}
