using System;
using System.Net.Http;
using System.Threading.Tasks;

public class MultiplayerServer
{
    private string serverIp;
    private string token;
    private int port;

    public MultiplayerServer(string ip = "localhost", string token = "", int port = 6060)
    {
        this.serverIp = ip;
        this.token = token;
        this.port = port;
    }

    public async Task<bool> Connect()
    {
        try
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"http://{serverIp}:{port}/connect?token={token}");
                response.EnsureSuccessStatusCode();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to server: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CreateSession(string sessionName, string sessionPassword)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"http://{serverIp}:{port}/createSession?name={sessionName}&password={sessionPassword}", null);
                response.EnsureSuccessStatusCode();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating session: {ex.Message}");
            return false;
        }
    }

    public async Task<Session> LoadSession(string sessionName, string sessionPassword)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"http://{serverIp}:{port}/loadSession?name={sessionName}&password={sessionPassword}");
                response.EnsureSuccessStatusCode();
                var sessionData = await response.Content.ReadAsStringAsync();
                return new Session(sessionData);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading session: {ex.Message}");
            return null;
        }
    }
}

public class Session
{
    private string sessionData;

    public Session(string data)
    {
        this.sessionData = data;
    }

    public async Task<bool> Store(string name, object data)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"http://{serverIp}:{port}/store?sessionData={sessionData}&name={name}&data={data}", null);
                response.EnsureSuccessStatusCode();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error storing data: {ex.Message}");
            return false;
        }
    }

    public async Task<object> Read(string name)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"http://{serverIp}:{port}/read?sessionData={sessionData}&name={name}");
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading data: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> Delete(string name)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var response = await client.DeleteAsync($"http://{serverIp}:{port}/delete?sessionData={sessionData}&name={name}");
                response.EnsureSuccessStatusCode();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting data: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Update(string name, object newData)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var response = await client.PutAsync($"http://{serverIp}:{port}/update?sessionData={sessionData}&name={name}&newData={newData}", null);
                response.EnsureSuccessStatusCode();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating data: {ex.Message}");
            return false;
        }
    }
}
