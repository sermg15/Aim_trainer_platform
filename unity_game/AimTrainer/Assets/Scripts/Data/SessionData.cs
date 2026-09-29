using System;
using System.Collections.Generic;

[Serializable]
public class SessionData
{
    public string sessionId;
    public string date;

    public int maxTargets;
    public float targetLifetimeMs;

    public List<TargetData> targets = new List<TargetData>();
}
