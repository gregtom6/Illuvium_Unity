using System;
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

    static List<Vector3> gridElementPositions = new List<Vector3>();

    static GridCreator instance;

    public static Action gridCreationFinished;

    void Start()
    {
        instance = this;

        CreateGrid();
    }

    public void CreateGrid()
    {
        if (instance == null) { return; }

        Vector3 position = Vector3.zero;

        gridElementPositions.Clear();

        for (int i = 0; i < instance.horizontalCount; i++)
        {
            for (int j = 0; j < instance.verticalCount; j++)
            {
                Instantiate(instance.gridElement, position, Quaternion.identity, instance.transform);

                gridElementPositions.Add(position + new Vector3(5f, 0f, 5f));

                position = new Vector3(position.x, position.y, position.z + instance.gridNewElementOffset);
            }

            position = new Vector3(position.x + instance.gridNewDistanceFromPreviousLine, position.y, i % 2 == 0 ? instance.gridNewLineIntendation : 0f);
        }

        gridCreationFinished?.Invoke();
    }

    public static Vector3 GetRandomPosition()
    {
        if (gridElementPositions.Count == 0) { return Vector3.zero; }

        return gridElementPositions[UnityEngine.Random.Range(0, gridElementPositions.Count)];
    }
}
