using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    [SerializeField] float hexesPerTimeStep;
    [SerializeField] ColorCode colorCode;

    bool isInitialized = false;

    Vector3 actualPosition;

    public void Initialize()
    {
        isInitialized = true;

        actualPosition = transform.position;

        Step();
    }

    void Step()
    {
        Vector3 target = SimulationManager.GetLocationOfUnit(colorCode == ColorCode.Red ? ColorCode.Blue : ColorCode.Red);

        Vector3 hexSideDirection = transform.forward;

        Vector3 minDistanceDirectionVector = Vector3.zero;
        float minDotProduct = 100f;

        Debug.DrawLine(Vector3.zero, new Vector3(5, 0, 0), Color.white, 100f);


        for (int i = 0; i < 6; i++)
        {
            Debug.DrawLine(actualPosition, actualPosition + hexSideDirection * 10f, Color.red, 5000f);
            float dotProduct = Vector3.Dot(target - actualPosition, -hexSideDirection);
            if (dotProduct < minDotProduct)
            {
                minDotProduct = dotProduct;
                minDistanceDirectionVector = hexSideDirection;
            }

            hexSideDirection = Quaternion.AngleAxis(60, Vector3.up) * hexSideDirection;
        }



        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(actualPosition + minDistanceDirectionVector * 10f, -Vector3.up, out hit, 100f))
        {
            GridElement gridElement = hit.transform.GetComponentInParent<GridElement>();

            if (gridElement != null)
            {
                Vector3 pos = gridElement.GetBackPosition();
                actualPosition = pos;
                SimulationManager.UpdatePosition(colorCode, actualPosition);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isInitialized) { return; }

        Step();
    }

    private void Update()
    {
        Visualize();
    }

    void Visualize()
    {
        transform.position = actualPosition;
    }
}

public enum ColorCode
{
    Red,
    Blue,
    None,
}