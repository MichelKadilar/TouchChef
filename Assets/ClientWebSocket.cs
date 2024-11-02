using UnityEngine;

using NativeWebSocket;

public class ClientWebSocket : MonoBehaviour
{
  private WebSocket _websocket;

  // Start is called before the first frame update
  async void Start()
  {
    _websocket = new WebSocket("ws://localhost:8080");

    _websocket.OnOpen += () =>
    {
      Debug.Log("Connection open!");
      
    
      // Send JSON object
      SendWebSocketMessage("{\"test\":\"message\"}");
    };

    _websocket.OnError += (e) =>
    {
      Debug.Log("Error! " + e);
    };

    _websocket.OnClose += (e) =>
    {
      Debug.Log("Connection closed!");
    };

    _websocket.OnMessage += (bytes) =>
    {
      // Reading a plain text message
      var message = System.Text.Encoding.UTF8.GetString(bytes);
      Debug.Log("Received OnMessage! (" + bytes.Length + " bytes) " + message);
    };

    await _websocket.Connect();
  }

  void Update()
  {
    #if !UNITY_WEBGL || UNITY_EDITOR
      _websocket.DispatchMessageQueue();
    #endif
  }

  async void SendWebSocketMessage(string message)
  {
    if (_websocket.State == WebSocketState.Open)
    {
      await _websocket.SendText(message);
    }
  }

  private async void OnApplicationQuit()
  {
    await _websocket.Close();
  }
}
