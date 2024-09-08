using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class FriendsManager
{
    private static GameObject FriendsPanel;
    private static RectTransform FriendsScrollContent;
    private static GameObject NamePlatePrefab;
    private static TMPro.TMP_InputField newFriendName;
    public static bool initialized = false;

    public static void init(GameObject FriendsP, RectTransform FriendsScrollC, GameObject NamePlateP)
    {
        FriendsPanel = FriendsP;
        FriendsScrollContent = FriendsScrollC;
        NamePlatePrefab = NamePlateP;
        GameData.friendsData = new List<string>();
        initialized = true;
    }

    public static void DisplayFriendsList()
    {
        if (initialized)
        {
            ClearContent();
            AddSelf();
            AddFriends();
        }
        else
        {
            Debug.LogError("Friends System Not Initialized!");
        }
    }

    private static void ClearContent()
    {
        foreach (Transform child in FriendsScrollContent)
        {
            UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    private static void AddSelf()
    {
        Transform selfNamePlate = FriendsPanel.transform.GetChild(0).GetChild(0);
        selfNamePlate.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = $"<{GameData.playerName}>";
        
        if(selfNamePlate.gameObject.GetComponent<Button>() == null)
        {
            AddTravelButton(selfNamePlate.gameObject, GameData.playerName);
        }

        newFriendName = FriendsPanel.transform.GetChild(0).GetChild(1).GetComponent<TMPro.TMP_InputField>();
        newFriendName.text = "";
    }


    private static void AddFriends()
    {
        GameObject namePlate;
        foreach (string name in GameData.friendsData)
        {
            namePlate = UnityEngine.Object.Instantiate(NamePlatePrefab, FriendsScrollContent);
            namePlate.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = name;
            AddTravelButton(namePlate,name);
        }
    }

    private static void AddTravelButton(GameObject namePlate,string name)
    {
        Button button = namePlate.AddComponent<Button>();
        System.Action action = new System.Action(() => TravelFriend(name));
        button.onClick.AddListener(() => action.Invoke());
    }

    private static void TravelFriend(string friendName)
    {
        if(GameData.fieldData.owner != friendName)
        {
            SocketClient.SendMessageToServer(6, friendName);
        }
    }
}