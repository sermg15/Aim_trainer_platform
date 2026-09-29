using System;
using System.Collections.Generic;

[Serializable]
public class TargetData
{
    public float targetX;
    public float targetY;

    public int targetId;

    public string result;
    public float reactionTimeMs;

    public List<ClickData> clicks = new List<ClickData>();

    public TargetData(int targetId, float targetX, float targetY)
    {
        this.targetId = targetId;
        this.targetX = targetX;
        this.targetY = targetY;

        result = "active";
        reactionTimeMs = -1f;
    }
}
