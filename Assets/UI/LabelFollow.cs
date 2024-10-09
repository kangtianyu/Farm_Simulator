using System;
using UnityEngine;

public class LabelFollow : MonoBehaviour
{
	public Transform targetModel; // The model this label follows
	public Vector3 offset; // Offset above the model
	public Camera mainCamera; // The camera that the label should face
	public int idx;
	public string type;
	public bool abandoned;

	private RectTransform rectTransform;
	private TMPro.TextMeshProUGUI text;
	private Action<LabelFollow> UpdateAction;

    public void init(Transform targetModel, Vector3 offset, string type, Action<LabelFollow> UpdateAction, Camera mainCamera = null)
	{
		this.targetModel = targetModel;
		this.offset = offset;
		this.type = type;
		this.UpdateAction = UpdateAction;

        rectTransform = GetComponent<RectTransform>();
		text = GetComponent<TMPro.TextMeshProUGUI>();
		idx = targetModel.GetSiblingIndex();
		this.abandoned = false;


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
		// Update text
		UpdateAction(this);

        // Convert the world position of the model to screen space
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetModel.position + offset);

		// Position the label at the screen position
		rectTransform.position = screenPosition;
	}

}
