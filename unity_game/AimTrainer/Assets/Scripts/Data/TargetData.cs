using System;
using System.Collections.Generic;

[Serializable]
public class TargetData
{
    public int targetId;
    public float spawnTimeMs;
    public float despawnTimeMs;
    public float targetX;
    public float targetY;
    public string result;
    public float? reactionTimeMs;

    public TargetData(int targetId, float spawnTimeMs, float targetX, float targetY)
    {
        this.targetId = targetId;
        this.spawnTimeMs = spawnTimeMs;
        this.targetX = targetX;
        this.targetY = targetY;

        despawnTimeMs = -1f;
        result = "active";
        this.reactionTimeMs = null;
    }
}