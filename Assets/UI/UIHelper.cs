using System.IO;
using UnityEngine;
using UnityEngine.UI;

public static class UIHelper
{
	public static void SetupImageButton(Button imageButton, System.Action onClickAction)
	{
		// Ensure the Button component is assigned
		if (imageButton != null)
		{
			// Remove all listeners to avoid multiple assignments
			imageButton.onClick.RemoveAllListeners();

			// Add the listener to the button
			imageButton.onClick.AddListener(() => onClickAction.Invoke());
		}
		else
		{
			Debug.LogError("Button component is not assigned.");
		}
	}

	/// <summary>
	/// Loads a Texture2D from the given file path.
	/// </summary>
	/// <param name="filePath">The file path to load the texture from.</param>
	/// <returns>The loaded Texture2D, or null if the loading fails.</returns>
	public static Texture2D LoadTexture(string filePath)
	{
		if (File.Exists(filePath))
		{
			byte[] fileData = File.ReadAllBytes(filePath);
			Texture2D texture = new Texture2D(2, 2);
			if (texture.LoadImage(fileData)) // Load the image data into the texture
			{
				return texture;
			}
		}
		return null;
	}

	/// <summary>
	/// Converts a Texture2D to a Sprite.
	/// </summary>
	/// <param name="texture">The Texture2D to convert.</param>
	/// <returns>The created Sprite.</returns>
	public static Sprite ConvertTextureToSprite(Texture2D texture)
	{
		return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
	}
}
