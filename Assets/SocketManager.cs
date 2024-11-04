using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class SocketManager : MonoBehaviour
{
    public static SocketManager Instance { get; private set; }

    private TcpClient client;
    private NetworkStream stream;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make this persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ConnectToSocket(string host, int port)
    {
        try
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();
            Debug.Log("Connected to socket server at " + host);
        }
        catch (SocketException e)
        {
            Debug.LogError("Socket connection failed: " + e.Message);
        }
    }

    public void SendMessage(string message)
    {
        if (stream != null && stream.CanWrite)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            stream.Write(msg, 0, msg.Length);
            Debug.Log("Message sent: " + message);
        }
    }

    public string ReceiveMessage()
    {
        try
        {
            if (stream != null && stream.DataAvailable)
            {
                byte[] receiveBuffer = new byte[1024];
                int bytesReceived = stream.Read(receiveBuffer, 0, receiveBuffer.Length);
                return Encoding.UTF8.GetString(receiveBuffer, 0, bytesReceived);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Socket receive failed: " + e.Message);
        }
        return null;
    }

    private void OnApplicationQuit()
    {
        if (stream != null) stream.Close();
        if (client != null) client.Close();
    }
}
