//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureFloatMenuInfo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => HouseObjectsManager.SelectedFurniture.RotateClockwise());
        transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => HouseObjectsManager.SelectedFurniture.RotateAnticlockwise());
        transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => HouseObjectsManager.SelectedFurniture.RemoveFurniture());
        transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => HouseObjectsManager.SelectedFurniture.HorizontalFlip());
    }
}
