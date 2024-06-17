using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnInput : MonoBehaviour
{
    [SerializeField] private int row;
    [SerializeField] private int column;
    
    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {

            Debug.Log("Column Presses:" + row + "," + column);
            GameManager.instance.ColumnPressed(row, column);

            // Access the BoxCollider component
            BoxCollider boxCollider = GetComponent<BoxCollider>();

            // Increase the size of the collider on each click
            Vector3 newSize = boxCollider.size;
            // newSize.y += 4;
            // boxCollider.size = newSize;

            Vector3 newCenter = boxCollider.center;
            // newCenter.y += 3;
            // boxCollider.center = newCenter;


        }
        
    }
}