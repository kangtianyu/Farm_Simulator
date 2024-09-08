using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum FieldContain {None, Empty, Tomato, Cabbage};
public enum ItemType {Empty, Tomato, Cabbage};

public class GameInstance : MonoBehaviour
{
	public Fields fields;
	public UIManagement ui;
	public GameObject meshCollider;
	public MapGenerator mapGen;
	public GameObject loginUICanvas;
	public TMP_InputField playerName;
	public TMP_InputField playerPassword;
	public Button loginButton;

    private bool loginSuccess = false;
    private bool initalized = false;

    //private SocketClient socketClient;

    // Start is called before the first frame update
    void Start()
	{
		GameData.inventoryData = null;
		GameData.fieldData = null;
		GameData.currentItem = ItemType.Empty;
		GameData.gameInstance = this;
        GameData.playerName = "Player";

		loginUICanvas.SetActive(true);
        ui.gameObject.SetActive(false);
        fields.gameObject.SetActive(false);

        //socketClient = new SocketClient();
        SocketClient.ClientStart();

		playerName.text = "Player";
    }

	// Update is called once per frame
	void Update()
	{
		if (loginSuccess)
        {
            if (!initalized)
			{
				loginUICanvas.SetActive(false);
				ui.gameObject.SetActive(true);
				fields.gameObject.SetActive(true);
				initalized = true;
            }
			ui.UpdateUI();
			fields.UpdateFields();
        }
    }

    public void ClientLogin()
	{
		GameData.playerName = playerName.text;
		Debug.Log($"Client: Login as {playerName.text} with password {playerPassword.text}");
        loginButton.interactable = false;
		Invoke("CheckLogin", 1f);
        SocketClient.Login(GameData.playerName);
    }


    public void Logout()
    {
        loginUICanvas.SetActive(true);
        GameData.inventoryData = null;
        GameData.fieldData = null;
        GameData.currentItem = ItemType.Empty;
        ui.ClearUI();
		fields.ClearFields();
        ui.gameObject.SetActive(false);
		fields.gameObject.SetActive(false);
        ui.UpdateUI();
        fields.UpdateFields();
        loginButton.interactable = true;
        loginSuccess = false;
        initalized = false;
        SocketClient.SendMessageToServer(0, "");
    }

    private void CheckLogin()
	{
		if (!loginSuccess) loginButton.interactable = true;
    }

	public void LoginSuccess()
	{
		loginSuccess = true;
    }

	public void AddNewFriend(TMP_InputField nameInputField)
	{
        SocketClient.SendMessageToServer(5, nameInputField.text);
    }

	void OnApplicationQuit()
	{
		SocketClient.OnApplicationQuit();
	}
}

public static class GameData
{
	public static FieldData fieldData;
	public static InventoryData inventoryData;
	public static List<string> friendsData;
    public static ItemType currentItem;
	public static GameInstance gameInstance;
    public static string playerName;

    public static int FieldGetStage(int idx)
	{
		return GameData.fieldData.fieldSlots[idx].stage;
    }
}