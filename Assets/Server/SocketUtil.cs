using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public static class SocketUtil
{
    public static void SendMessage(Socket socket, byte commandType, string message)
    {
            // Convert the message to bytes
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // Prefix the message with its length (4 bytes for an int)
            byte[] lengthPrefix = BitConverter.GetBytes(messageBytes.Length);

            // Concatenate length prefix and message bytes
            byte[] dataToSend = new byte[lengthPrefix.Length + 1 + messageBytes.Length];
            Array.Copy(lengthPrefix, dataToSend, lengthPrefix.Length);
            dataToSend[lengthPrefix.Length] = commandType;
            Array.Copy(messageBytes, 0, dataToSend, lengthPrefix.Length + 1, messageBytes.Length);

            // Send the data
            socket.Send(dataToSend);
    }

    public static (byte commandType, string message) ReceiveMessage(Socket socket)
    {
        byte[] lengthBuffer = new byte[4];
        byte[] commandTypeBuffer = new byte[1];
        byte[] messageBuffer;
        while (true)
        {
            // Read the length prefix (blocking call until 4 bytes are received)
            int receivedBytes = socket.Receive(lengthBuffer, 0, 4, SocketFlags.None);
            if (receivedBytes == 4)
            {
                // Convert length prefix to int
                int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                while (true)
                {
                    // Read the command type code
                    receivedBytes = socket.Receive(commandTypeBuffer, 0, 1, SocketFlags.None);
                    if (receivedBytes == 1)
                    {
                        // Read the actual message bytes
                        messageBuffer = new byte[messageLength];
                        receivedBytes = 0;
                        while (receivedBytes < messageLength)
                        {
                            receivedBytes += socket.Receive(messageBuffer, receivedBytes, messageLength - receivedBytes, SocketFlags.None);
                        }
                        // Convert the message bytes back to a string
                        string message = Encoding.UTF8.GetString(messageBuffer);

                        return (commandTypeBuffer[0], message);
                    }
                }
            }
        }
    }

    public static string SerializeObjectToString(object obj)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            // Create a BinaryFormatter to serialize the object
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, obj);

            // Convert the serialized object (as bytes) into a Base64 string
            return Convert.ToBase64String(memoryStream.ToArray());
        }
    }

    public static T DeserializeStringToObject<T>(string base64String)
    {
        byte[] bytes = Convert.FromBase64String(base64String);
        using (MemoryStream memoryStream = new MemoryStream(bytes))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            return (T)binaryFormatter.Deserialize(memoryStream);
        }
    }


    public static T SerializeObjectCopy<T>(T obj)
    {
        Debug.Log($"Client {obj} copied");
        MemoryStream memoryStream = new MemoryStream();

        // Create a BinaryFormatter to serialize the object
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(memoryStream, obj);

        memoryStream.Seek(0, SeekOrigin.Begin);
        T newObj = (T)binaryFormatter.Deserialize(memoryStream);

        return newObj;

    }
}
