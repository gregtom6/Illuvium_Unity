using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UnitController : MonoBehaviour
{
    [SerializeField] float hexesPerTimeStep;
    [SerializeField] float attackRangeInHexTiles;
    [SerializeField] int hitPointMinBorder;
    [SerializeField] int hitPointMaxBorder;
    [SerializeField] int timeStepsPerAttack;
    [SerializeField] ColorCode colorCode;
    [SerializeField] Transform checker;

    bool isInitialized = false;

    Vector3 actualPosition;

    State state = State.None;

    public void Initialize()
    {
        isInitialized = true;

        actualPosition = transform.position;
        SimulationManager.UpdatePosition(colorCode, actualPosition);

        state = State.Movement;

        StateCheckingAndSwitchingIfNeeded();
    }

    public void StateCheckingAndSwitchingIfNeeded()
    {
        int dist = GetHexDistanceBetweenEnemyAndMe();
        if (dist > attackRangeInHexTiles)
        {
            state = State.Movement;
        }
        else
        {
            state = State.Attack;
        }

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

    int GetHexDistanceBetweenEnemyAndMe()
    {
        Vector3 posForDistanceChecking = actualPosition;
        Vector3 target = SimulationManager.GetLocationOfUnit(colorCode == ColorCode.Red ? ColorCode.Blue : ColorCode.Red);
        int stepCount = 0;

        checker.position = actualPosition;

        while (posForDistanceChecking != target)
        {
            Vector3 hexSideDirection = checker.forward;

            Vector3 minDistanceDirectionVector = Vector3.zero;
            float minDotProduct = 100f;

            for (int i = 0; i < 6; i++)
            {
                float dotProduct = Vector3.Dot(target - posForDistanceChecking, -hexSideDirection);
                if (dotProduct < minDotProduct)
                {
                    minDotProduct = dotProduct;
                    minDistanceDirectionVector = hexSideDirection;
                }

                hexSideDirection = Quaternion.AngleAxis(60, Vector3.up) * hexSideDirection;
            }



            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(posForDistanceChecking + minDistanceDirectionVector * 10f, -Vector3.up, out hit, 100f))
            {
                GridElement gridElement = hit.transform.GetComponentInParent<GridElement>();

                if (gridElement != null)
                {
                    Vector3 pos = gridElement.GetBackPosition();
                    posForDistanceChecking = pos;
                    checker.position = pos;
                }
            }

            stepCount += 1;
        }

        return stepCount;
    }

    private void FixedUpdate()
    {
        if (!isInitialized) { return; }

        if (state == State.Movement)
        {
            for (int i = 0; i < hexesPerTimeStep; i++)
            {
                Step();

                StateCheckingAndSwitchingIfNeeded();

                if (state == State.Attack)
                {
                    break;
                }
            }
        }
        else if (state == State.Attack)
        {

        }
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

public enum State
{
    None,
    Movement,
    Attack,
}