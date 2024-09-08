using UnityEngine;

public class LabelFollow : MonoBehaviour
{
	public Transform targetModel; // The model this label follows
	public Vector3 offset; // Offset above the model
	public Camera mainCamera; // The camera that the label should face
	public int idx;
	public string type;

	private RectTransform rectTransform;
	private TMPro.TextMeshProUGUI text;

	public void init(Transform targetModel, Vector3 offset, string type, Camera mainCamera = null)
	{
		this.targetModel = targetModel;
		this.offset = offset;
		this.type = type;
		rectTransform = GetComponent<RectTransform>();
		text = GetComponent<TMPro.TextMeshProUGUI>();
		idx = targetModel.GetSiblingIndex();

		if (mainCamera == null)
		{
			mainCamera = Camera.main; // Get the main camera if not assigned
		}
		this.mainCamera = mainCamera;

	}

	public void SetText(string newText)
	{
		text.text = newText;
	}

	public void UpdateLabel()
	{
		// Convert the world position of the model to screen space
		Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetModel.position + offset);

		// Position the label at the screen position
		rectTransform.position = screenPosition;
	}

}
