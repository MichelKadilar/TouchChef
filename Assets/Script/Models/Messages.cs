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
    public string taskId;
    public string quantity;
    public string workstation;
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

[Serializable]
public class TaskProgressMessage
{
    public string type = "taskProgress";
    public string from = "unity";
    public string to = "angular";
    public TaskProgressData progressData;
}

[Serializable]
public class TaskProgressData
{
    public string playerId;
    public string taskId;
    public int currentProgress;
    public int targetProgress;
}

[Serializable]
public class TaskCompletionMessage
{
    public string type = "taskComplete";
    public string from = "unity";
    public string to = "angular";
    public TaskProgressData progressData;
}

[System.Serializable]
public class DeliveryScoreMessage
{
    public string type = "updatescore";
    public string from = "table";
    public string to = "angular";
    public string ingredientState;
}