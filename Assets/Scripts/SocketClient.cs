using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public static class SocketClient
{
    private static Socket clientSocket;
    private static Thread clientThread;
    private static bool isRunning = false;
    private static bool isConnected = false;

    public static string serverIP = "127.0.0.1";
    public static int port = 33366;
    public static float reconnectDelay = 5.0f; // Time in seconds to wait before reconnecting

    public static void ClientStart()
    {
        isRunning = true;
        // Start the client in a separate thread
        clientThread = new Thread(ClientLoop);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private static void ClientLoop()
    {
        byte commandType;
        string message;

        while (isRunning)
        {
            if (!isConnected)
            {
                TryConnect();
            }

            if (isConnected)
            {
                try
                {
                    // Handle communication with the server
                    (commandType, message) = SocketUtil.ReceiveMessage(clientSocket);
                   
                    Debug.Log($"Client: received from server command {commandType}: {message}");
                    ProcessCommand(commandType, message);
                }
                catch (SocketException ex)
                {
                    Debug.LogError($"Client: server connection lost: {ex.Message}");
                    isConnected = false;
                }
            }

            // Add a small delay to prevent tight looping
            Thread.Sleep(100);
        }
    }

    public static void ProcessCommand(byte commandType, string message)
    {
        switch (commandType)
        {
            case 0:
                // Empty Command
                break;
            case 1:
                // Login Info
                GameData.gameInstance.LoginSuccess();
                break;
            case 2:
                // Player's Inventory
                GameData.inventoryData = SocketUtil.DeserializeStringToObject<InventoryData>(message);
                GameData.gameInstance.ui.dataLoaded();
                GameData.gameInstance.ui.UpdateInventory();
                break;
            case 3:
                // Player's Field
                GameData.fieldData = SocketUtil.DeserializeStringToObject<FieldData>(message);
                GameData.gameInstance.fields.dataLoaded();
                GameData.gameInstance.fields.FieldUpdated();
                break;
            case 4:
                // Shop
                break;
            case 5:
                // Friends' name list
                GameData.friendsData = SocketUtil.DeserializeStringToObject<List<string>>(message);
                GameData.gameInstance.ui.UpdateFriends();
                break;
            case 6:
                // Other's fields
                GameData.gameInstance.fields.ClearFields();
                GameData.fieldData = SocketUtil.DeserializeStringToObject<FieldData>(message);
                break;
            default:
                break;
        }
    }

    private static void TryConnect()
    {
        try
        {
            // Initialize the socket
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse(serverIP), port));
            isConnected = true;
            Debug.Log("Client connected to the server.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Client connection failed: {ex.Message}");
            isConnected = false;

            // Wait before trying to reconnect
            Thread.Sleep((int)(reconnectDelay * 1000));
        }
    }

    public static void SendMessageToServer(byte commandType, string message)
    {
        if (isConnected)
        {
            try
            {
                SocketUtil.SendMessage(clientSocket, commandType, message);
                Debug.Log($"Client send Command {commandType}: {message}");
            }
            catch (SocketException ex)
            {
                Debug.LogError($"Client failed to send message: {ex.Message}");
                isConnected = false;
            }
        }
        else
        {
            Debug.LogError("Client cannot send message. Not connected to the server.");
        }
    }

    public static void Login(string name)
    {
        SendMessageToServer(1, name);
    }

    public static void OnApplicationQuit()
    {
        isRunning = false;
        if (clientSocket != null && clientSocket.Connected)
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        if (clientThread != null && clientThread.IsAlive)
        {
            clientThread.Abort();
        }
    }
}