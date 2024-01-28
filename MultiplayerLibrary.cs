using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class MultiplayerLibrary
{
    public class MultiplayerServer
    {
        private string serverURL;
        private string token;

        public MultiplayerServer(string ip, string token, int port)
        {
            this.serverURL = $"http://{ip}:{port}/";
            this.token = token;
        }

        public MultiplayerSession ConnectToSession(string sessionName, string password, Action<bool> callback)
        {
            UnityWebRequest request = UnityWebRequest.Get(serverURL);
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            request.SetRequestHeader("Session-Name", sessionName);
            request.SetRequestHeader("Password", password);

            request.SendWebRequest();
            while (!request.isDone) { }

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true);
                return new MultiplayerSession(sessionName, token, serverURL);
            }
            else
            {
                Debug.Log("Failed to connect to the session. Error: " + request.error);
                callback(false);
                return null;
            }
        }

        public MultiplayerSession CreateSession(string sessionName, string password, Action<bool> callback)
        {
            UnityWebRequest request = UnityWebRequest.PostWwwForm(serverURL + "create-session", "");
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            request.SetRequestHeader("Session-Name", sessionName);
            request.SetRequestHeader("Password", password);

            request.SendWebRequest();
            while (!request.isDone) { }

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(true);
                return new MultiplayerSession(sessionName, token, serverURL);
            }
            else
            {
                Debug.Log("Failed to create the session. Error: " + request.error);
                callback(false);
                return null;
            }
        }
    }

    public class MultiplayerSession
    {
        private string sessionName;
        private string token;
        private string serverURL;

        public MultiplayerSession(string sessionName, string token, string serverURL)
        {
            this.sessionName = sessionName;
            this.token = token;
            this.serverURL = serverURL;
        }

        public void StoreData(string key, string value, Action<bool> callback)
        {
            UnityWebRequest request = UnityWebRequest.PostWwwForm(serverURL + "store-data", "");
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            request.SetRequestHeader("Session-Name", sessionName);
            request.SetRequestHeader("Key", key);
            request.SetRequestHeader("Value", value);

            request.SendWebRequest();
            while (!request.isDone) { }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Data stored successfully!");
                callback(true);
            }
            else
            {
                Debug.Log("Failed to store data. Error: " + request.error);
                callback(false);
            }
        }

        public void ReadData(string key, Action<string> callback)
        {
            UnityWebRequest request = UnityWebRequest.Get(serverURL + $"read-data?key={key}");
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            request.SetRequestHeader("Session-Name", sessionName);

            request.SendWebRequest();
            while (!request.isDone) { }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string data = request.downloadHandler.text;
                callback(data);
            }
            else
            {
                Debug.Log("Failed to read data. Error: " + request.error);
                callback(null);
            }
        }
    }
}
