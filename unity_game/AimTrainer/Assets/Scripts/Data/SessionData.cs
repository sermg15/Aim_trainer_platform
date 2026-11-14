using System;
using System.Collections.Generic;

[Serializable]
public class SessionData
{
    public string schemaVersion;
    public string sessionId;
    public string playerId;
    public string timestamp;
    public int maxTargets;
    public float targetLifetimeSeconds;

    public List<TargetData> targets = new List<TargetData>();
    public List<ClickData> clicks = new List<ClickData>();
}

