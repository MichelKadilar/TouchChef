using UnityEngine;

using NativeWebSocket;
using UnityEngine.SceneManagement;

public class ClientWebSocket : MonoBehaviour
{
  private WebSocket _websocket;

  // Start is called before the first frame update
  async void Start()
  {
    _websocket = new WebSocket("ws://websocket.chhilif.com/ws");

    _websocket.OnOpen += () =>
    {
      Debug.Log("Connection open!");
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
      if (message.Contains("startGame"))
      {
        // Change scene to cuisine
        SceneManager.LoadScene("Cuisine");
      }
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
