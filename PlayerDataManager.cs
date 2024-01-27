using UnityEngine;
using System;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerDataManager
{
    private TcpClient client;
    private NetworkStream stream;
    private BinaryFormatter formatter = new BinaryFormatter();

    private string serverAddress;
    private int serverPort;
    private string serverPassword;

    public string ConnectionError { get; private set; }

    public bool Connect(string hostname, int port = 6060, string password = "")
    {
        try
        {
            client = new TcpClient(hostname, port);
            stream = client.GetStream();
            serverAddress = hostname;
            serverPort = port;
            serverPassword = password;
            return true;
        }
        catch (Exception e)
        {
            ConnectionError = $"Error connecting to server: {e.Message}";
            return false;
        }
    }

    public void Disconnect()
    {
        stream.Close();
        client.Close();
    }

    public void StoreData(string name, object value)
    {
        SendCommand("store", name, value);
    }

    public T ReadData<T>(string name)
    {
        return SendCommandWithResult<T>("read", name);
    }

    public void RemoveData(string name)
    {
        SendCommand("remove", name);
    }

    public void UpdateData(string name, object newValue)
    {
        SendCommand("update", name, newValue);
    }

    private void SendCommand(string command, string name, object value = null)
    {
        try
        {
            var data = new
            {
                command,
                name,
                value,
                password = serverPassword
            };

            MemoryStream memoryStream = new MemoryStream();
            formatter.Serialize(memoryStream, data);

            byte[] byteData = memoryStream.ToArray();
            stream.Write(byteData, 0, byteData.Length);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending command to server: {e.Message}");
        }
    }

    private T SendCommandWithResult<T>(string command, string name)
    {
        try
        {
            var data = new
            {
                command,
                name,
                password = serverPassword
            };

            MemoryStream memoryStream = new MemoryStream();
            formatter.Serialize(memoryStream, data);

            byte[] byteData = memoryStream.ToArray();
            stream.Write(byteData, 0, byteData.Length);

            byte[] resultData = new byte[1024];
            int bytesRead = stream.Read(resultData, 0, resultData.Length);
            MemoryStream resultStream = new MemoryStream(resultData, 0, bytesRead);
            return (T)formatter.Deserialize(resultStream);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending command to server: {e.Message}");
            return default;
        }
    }
}
