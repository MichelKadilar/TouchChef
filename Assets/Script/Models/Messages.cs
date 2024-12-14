using System;

[Serializable]
public class WebSocketTaskMessage
{
    public string type;
    public string from;
    public string to;
    public AssignedTaskData assignedTask;
}

[Serializable]
public class WebSocketMessage
{
    public string type;
    public string from;
    public string to;
    public WebSocketProduct product;
}

[Serializable]
public class WebSocketProduct
{
    public int id;
    public string name;
    public string icon;
}

[Serializable]
public class AssignedTaskData
{
    public string taskName;
    public CookData cook;
}

[Serializable]
public class CookData
{
    public string name;
    public string deviceId;
    public string avatar;
    public string color;
}