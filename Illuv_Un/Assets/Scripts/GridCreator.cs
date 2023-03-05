using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCreator : MonoBehaviour
{
    [SerializeField] GameObject gridElement;
    [SerializeField] int horizontalCount;
    [SerializeField] int verticalCount;
    [SerializeField] float gridNewElementOffset = 10f;
    [SerializeField] float gridNewDistanceFromPreviousLine = 8.5f;
    [SerializeField] float gridNewLineIntendation = -5f;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 position = Vector3.zero;

        for (int i = 0; i < horizontalCount; i++)
        {
            for (int j = 0; j < verticalCount; j++)
            {
                Instantiate(gridElement, position, Quaternion.identity, transform);

                position = new Vector3(position.x, position.y, position.z + gridNewElementOffset);
            }

            position = new Vector3(position.x + gridNewDistanceFromPreviousLine, position.y, i % 2 == 0 ? gridNewLineIntendation : 0f);
        }
    }
}
