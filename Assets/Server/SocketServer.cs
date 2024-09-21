using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;

public class SocketServer : MonoBehaviour
{
    private Socket listenerSocket;
    private Thread serverThread;
    private bool isRunning = false;
    private List<Thread> clientThreadList = new List<Thread>();

    private static Dictionary<string, InventoryData> ServerInventoryData = new Dictionary<string, InventoryData>();
    private static Dictionary<string, FieldData> ServerFieldData = new Dictionary<string, FieldData>();
    private static Dictionary<string, List<string>> ServerFriendsData = new Dictionary<string, List<string>>();

    private static string SavePath = "Saves";
    private static BinaryFormatter formatter = new BinaryFormatter();


    void Start()
    {
        // Start the server in a separate thread
        serverThread = new Thread(StartServer);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    void StartServer()
    {
        isRunning = true;

        // Define the IP address and port for the server
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 33366;

        // Create a new socket
        listenerSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint
        listenerSocket.Bind(new IPEndPoint(ipAddress, port));

        // Start listening for incoming connections
        listenerSocket.Listen(10);

        Debug.Log($"Server started. Listening on {ipAddress}:{port}");

        while (isRunning)
        {
            try
            {
                // Accept an incoming connection
                Socket clientSocket = listenerSocket.Accept();
                Debug.Log("Server: Client connected.");

                // Handle client communication in a separate thread
                Thread clientThread = new Thread(() => HandleClient(clientSocket));
                clientThreadList.Add(clientThread);
                clientThread.IsBackground = true;
                clientThread.Start();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Server error: {ex.Message}");
            }
        }

        listenerSocket.Close();
    }

    private void HandleClient(Socket clientSocket)
    {
        byte commandType;
        string message;
        string playerHash = null;
        string farmHash = null;

        //try
        //{
            while (true)
            {
                (commandType, message) = SocketUtil.ReceiveMessage(clientSocket);
                Debug.Log($"Server received command {commandType}, message: {message}");

                if (commandType == 1)
                {
                    // Login and identify player
                    playerHash = PlayerHash(message);
                    farmHash = (string)playerHash.Clone();
                    LoadData(message);
                    SocketUtil.SendMessage(clientSocket, 1, "");
                    SynchronizeDataWithClient(clientSocket, playerHash);
                }
                else if (playerHash == null)
                {
                    Debug.LogError($"Server error. Can't identify player before processing command.");
                }
                else
                {
                    farmHash = ProcessCommand(clientSocket, playerHash, farmHash, commandType, message);
                }

            }
        //}
        //catch (Exception ex)
        //{
        //    Debug.LogError($"Client disconnected. {ex.Message}");
        //}
        //finally
        //{
        //    if (playerHash != null)
        //    {
        //        SaveData(playerHash);
        //    }
        //    // Close the connection
        //    clientSocket.Close();
        //}
    }

    private static void SynchronizeDataWithClient(Socket clientSocket, string playerHash)
    {
        SynchronizeInventoryDataWithClient(clientSocket, playerHash);
        SynchronizeFieldDataWithClient(clientSocket, playerHash);
        SynchronizeFriendsDataWithClient(clientSocket, playerHash);
    }

    private static void SynchronizeInventoryDataWithClient(Socket clientSocket, string playerHash)
    {
        try
        {
            string message = SocketUtil.SerializeObjectToString(ServerInventoryData[playerHash]);
            SocketUtil.SendMessage(clientSocket, 2, message);
            Debug.Log($"Server send Command 2: {message}");
        }
        catch (SocketException ex)
        {
            Debug.LogError($"Server failed to send message: {ex.Message}");
        }
    }

    private static void SynchronizeFieldDataWithClient(Socket clientSocket, string playerHash)
    {
        try
        {
            string message = SocketUtil.SerializeObjectToString(ServerFieldData[playerHash]);
            SocketUtil.SendMessage(clientSocket, 3, message);
            Debug.Log($"Server send Command 3: {message}");
        }
        catch (SocketException ex)
        {
            Debug.LogError($"Server failed to send message: {ex.Message}");
        }
    }

    private static void SynchronizeFriendsDataWithClient(Socket clientSocket, string playerHash)
    {
        try
        {
            string message = SocketUtil.SerializeObjectToString(ServerFriendsData[playerHash]);
            SocketUtil.SendMessage(clientSocket, 5, message);
            Debug.Log($"Server send Command 5: {message}");
        }
        catch (SocketException ex)
        {
            Debug.LogError($"Server failed to send message: {ex.Message}");
        }
    }

    private string ProcessCommand(Socket clientSocket, string playerHash, string farmHash, byte commandType, string message)
    {
        ShopItem item;
        string nameHash;
        switch (commandType)
        {
            case 0:
                // Logout
                SaveData(playerHash);
                if (playerHash != farmHash) SaveFieldData(farmHash);
                playerHash = null;
                break;
            case 1:
                // Login Info (Init login have processed)
                break;
            case 2:
                // Player's Inventory
                break;
            case 3:
                // Player's Field
                (int idx, FieldContain fieldContain) = SocketUtil.DeserializeStringToObject<(int, FieldContain)>(message);
                int stage = ServerFieldData[farmHash].fieldSlots[idx].stage;
                Debug.Log($"Plant field {idx} at stage {stage} with {fieldContain}");
                switch (stage)
                {
                    case -1:
                        if (playerHash == farmHash)
                        {
                            FieldInit(farmHash, idx, fieldContain);
                            item = GetItemByFieldContain(playerHash, fieldContain);
                            UpdateItem(playerHash, item, -1);
                        }
                        break;
                    default:
                        FieldNextStage(playerHash, farmHash, idx);
                        break;
                }
                SynchronizeFieldDataWithClient(clientSocket, farmHash);
                SynchronizeInventoryDataWithClient(clientSocket, playerHash);
                break;
            case 4:
                // Shop 
                item = SocketUtil.DeserializeStringToObject<ShopItem>(message);
                switch (item.type)
                {
                    case "Seeds":
                        UpdateItem(playerHash, item, 1);
                        SynchronizeInventoryDataWithClient(clientSocket, playerHash);
                        break;
                    case "Upgrade":
                        for (int i = 0; i < ShopManager.maxField; i++)
                        {
                            if (ServerFieldData[playerHash].fieldSlots[i].fieldContain == FieldContain.None)
                            {
                                ServerFieldData[playerHash].fieldSlots[i].fieldContain = FieldContain.Empty;
                                break;
                            }
                        }
                        SynchronizeFieldDataWithClient(clientSocket, playerHash);
                        break;
                    default:
                        break;
                }
                break;
            case 5:
                // Add new Friend
                nameHash = PlayerHash(message);
                if (!ServerFriendsData[playerHash].Contains(message) && nameHash != playerHash && ExistPlayer(nameHash))
                {
                    ServerFriendsData[playerHash].Add(message);
                    SynchronizeFriendsDataWithClient(clientSocket, playerHash);
                }
                break;
            case 6:
                // Go to other's farm
                nameHash = PlayerHash(message);
                FieldData fd;
                if (ServerFieldData.ContainsKey(nameHash))
                {
                    fd = ServerFieldData[nameHash];
                }
                else
                {
                    fd = LoadFieldData(nameHash);
                }
                if (fd != null)
                {
                    try
                    {
                        string newMessage = SocketUtil.SerializeObjectToString(fd);
                        SocketUtil.SendMessage(clientSocket, 6, newMessage);
                        SaveFieldData(farmHash);
                        farmHash = (string)nameHash.Clone();
                        Debug.Log($"Server send Command 6: {newMessage}");
                    }
                    catch (SocketException ex)
                    {
                        Debug.LogError($"Server failed to send message: {ex.Message}");
                    }
                }
                break;
            default:
                break;
        }
        return farmHash;
    }

    private static void FieldInit(string farmHash, int idx, FieldContain type)
    {
        ServerFieldData[farmHash].fieldSlots[idx].fieldContain = type;
        ServerFieldData[farmHash].fieldSlots[idx].startTime = System.DateTime.UtcNow;
        ServerFieldData[farmHash].fieldSlots[idx].stage = 0;
    }

    private static void FieldNextStage(string playerHash, string farmHash, int idx)
    {
        if (ServerFieldData[farmHash].fieldSlots[idx].stage + 1 < ShopManager.plantStages[ServerFieldData[farmHash].fieldSlots[idx].fieldContain].Count)
        {
            if (playerHash == farmHash)
            {
                ServerFieldData[farmHash].fieldSlots[idx].startTime = System.DateTime.UtcNow;
                ServerFieldData[farmHash].fieldSlots[idx].stage += 1;
            }
        }
        else
        {
            ServerFieldData[farmHash].fieldSlots[idx].stage = -1;
            if(playerHash == farmHash)
            {
                ServerInventoryData[playerHash].coins += ShopManager.plantPrices[ServerFieldData[farmHash].fieldSlots[idx].fieldContain];
            }
            else
            {
                AddCoins(farmHash, ShopManager.plantPrices[ServerFieldData[farmHash].fieldSlots[idx].fieldContain]);
                ServerInventoryData[playerHash].coins += ShopManager.plantPrices[ServerFieldData[farmHash].fieldSlots[idx].fieldContain] / 10;
            }
            ServerFieldData[farmHash].fieldSlots[idx].fieldContain = FieldContain.Empty;

        }
    }

    private static ShopItem GetItemByFieldContain(string playerHash, FieldContain fieldContain)
    {
        string name = "";
        switch (fieldContain)
        {
            case FieldContain.Tomato:
                name = "Tomato Seeds";
                break;
            case FieldContain.Cabbage:
                name = "Cabbage Seeds";
                break;
            default:
                break;
        }

        if (ServerInventoryData[playerHash] != null)
        {
            List<GameItem> gameItems = ServerInventoryData[playerHash].gameItems;

            GameItem itm = gameItems.Find(x => x.item.name == name);
            if (itm != null)
            {
                return itm.item;
            }
        }
        return null;
    }

    private static void UpdateItem(string playerHash, ShopItem item, int num)
    {
        if (ServerInventoryData[playerHash] != null)
        {
            List<GameItem> gameItems = ServerInventoryData[playerHash].gameItems;

            GameItem itm = gameItems.Find(x => x.item.name == item.name);
            if (itm == null)
            {
                if (num > 0)
                {
                    itm = new GameItem(item, num);
                    gameItems.Add(itm);
                }
            }
            else
            {
                itm.num += num;
                if (itm.num <= 0)
                {
                    gameItems.Remove(itm);
                }
            }
        }
    }

    private static string PlayerHash(string playerName)
    {
        // Generate a SHA256 hash from the string
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(playerName));

            // Convert the byte array to a hexadecimal string
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("X2"));
            }

            return builder.ToString();
        }
    }

    private bool ExistPlayer(string nameHash)
    {
        string directoryPath = Path.Combine(SavePath, "Inventories");

        if (!Directory.Exists(directoryPath))
        {
            Debug.LogError("Directory does not exist: " + directoryPath);
            return false;
        }

        IEnumerable<string> files = Directory.EnumerateFiles(directoryPath);
        foreach (string file in files)
        {
            if (nameHash.Equals(Path.GetFileName(file))) return true;
        }

        return false;
    }

    private void SaveData(string playerHash)
    {
        string inventoryPath = Path.Combine(SavePath, $"Inventories/{playerHash}");
        string fieldPath = Path.Combine(SavePath, $"Fields/{playerHash}");
        string friendsPath = Path.Combine(SavePath, $"Friends/{playerHash}");

        using (FileStream stream = new FileStream(inventoryPath, FileMode.Create))
        {
            formatter.Serialize(stream, ServerInventoryData[playerHash]);
        }
        using (FileStream stream = new FileStream(fieldPath, FileMode.Create))
        {
            formatter.Serialize(stream, ServerFieldData[playerHash]);
        }
        using (FileStream stream = new FileStream(friendsPath, FileMode.Create))
        {
            formatter.Serialize(stream, ServerFriendsData[playerHash]);
        }

        Debug.Log($"{playerHash}'s Data Saved.");
    }

    private static void AddCoins(string playerHash, int num)
    {
        string inventoryPath = Path.Combine(SavePath, $"Inventories/{playerHash}");
        if (!ServerInventoryData.ContainsKey(playerHash))
        {
            if (File.Exists(inventoryPath))
            {
                using (FileStream stream = new FileStream(inventoryPath, FileMode.Open))
                {
                    ServerInventoryData[playerHash] = (InventoryData)formatter.Deserialize(stream);
                }
            }
            else
            {
                Debug.Log($"Server: Creating inventory data for new player {playerHash}");
                ServerInventoryData[playerHash] = new InventoryData();
            }
        }
        ServerInventoryData[playerHash].coins += num;
        using (FileStream stream = new FileStream(inventoryPath, FileMode.Create))
        {
            formatter.Serialize(stream, ServerInventoryData[playerHash]);
        }
    }

    private void LoadData(string playerName)
    {
        string playerHash = PlayerHash(playerName);
        string inventoryPath = Path.Combine(SavePath, $"Inventories/{playerHash}");
        string fieldPath = Path.Combine(SavePath, $"Fields/{playerHash}");
        string friendsPath = Path.Combine(SavePath, $"Friends/{playerHash}");

        if (File.Exists(inventoryPath))
        {
            using (FileStream stream = new FileStream(inventoryPath, FileMode.Open))
            {
                ServerInventoryData[playerHash] = (InventoryData)formatter.Deserialize(stream);
            }
        }
        else
        {
            Debug.Log($"Server: Creating inventory data for new player {playerName}");
            ServerInventoryData[playerHash] = new InventoryData();
        }

        if (File.Exists(fieldPath))
        {
            using (FileStream stream = new FileStream(fieldPath, FileMode.Open))
            {
                ServerFieldData[playerHash] = (FieldData)formatter.Deserialize(stream);
            }
        }
        else
        {
            Debug.Log($"Server: Creating field data for new player {playerName}");
            ServerFieldData[playerHash] = new FieldData(playerName);
            ServerFieldData[playerHash].seed = (new System.Random()).Next();
        }

        if (File.Exists(friendsPath))
        {
            using (FileStream stream = new FileStream(friendsPath, FileMode.Open))
            {
                ServerFriendsData[playerHash] = (List<string>)formatter.Deserialize(stream);
            }
        }
        else
        {
            Debug.Log($"Server: Creating friends data for new player {playerName}");
            ServerFriendsData[playerHash] = new List<string>();
        }
    }


    private FieldData LoadFieldData(string nameHash)
    {
        string fieldPath = Path.Combine(SavePath, $"Fields/{nameHash}");
        if (File.Exists(fieldPath))
        {
            using (FileStream stream = new FileStream(fieldPath, FileMode.Open))
            {
                ServerFieldData[nameHash] = (FieldData)formatter.Deserialize(stream);
                return ServerFieldData[nameHash];
            }
        }
        return null;
    }

    private void SaveFieldData(string nameHash)
    {
        string fieldPath = Path.Combine(SavePath, $"Fields/{nameHash}");
        using (FileStream stream = new FileStream(fieldPath, FileMode.Create))
        {
            formatter.Serialize(stream, ServerFieldData[nameHash]);
        }
    }

    void OnApplicationQuit()
    {
        // Stop the server when the application quits
        isRunning = false;
        listenerSocket.Close();

        foreach(Thread t in clientThreadList)
        {
            if (t != null && t.IsAlive)
            {
                t.Abort();
            }
        }

        if (serverThread != null && serverThread.IsAlive)
        {
            serverThread.Abort();
        }
    }
}
