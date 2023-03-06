using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UnitController : MonoBehaviour
{
    [SerializeField] ColorCode colorCode;
    [SerializeField] MeshRenderer renderer;
    [SerializeField] Transform checker;
    [SerializeField] GameObject attackParentGameObject;
    [SerializeField] GameObject dieParentGameObject;
    [SerializeField] Transform continuousVisualParent;
    [SerializeField] float hexesPerTimeStep;
    [SerializeField] float attackRangeInHexTiles;
    [SerializeField] int hitPointMinBorder;
    [SerializeField] int hitPointMaxBorder;
    [SerializeField] int timeStepsPerAttack;

    int maxLifePoint;
    int currentLifePoint;

    bool isInitialized = false;

    Vector3 actualPosition;

    State state = State.None;

    int timeStepCount;

    float actualTime;

    Vector3 locationUsedForTarget;

    private void Start()
    {
        maxLifePoint = Random.Range(hitPointMinBorder, hitPointMaxBorder);

        if (maxLifePoint == 0)
            maxLifePoint = 1;

        currentLifePoint = maxLifePoint;
    }

    public void Initialize()
    {
        isInitialized = true;

        actualPosition = SimulationManager.GetLocationOfUnit(colorCode);
        transform.position = actualPosition;

        state = State.Movement;

        StateCheckingAndSwitchingIfNeeded();

        if (state == State.Movement)
        {
            locationUsedForTarget = CalculateNextStep();
        }
    }

    public void StateCheckingAndSwitchingIfNeeded()
    {
        int dist = GetHexDistanceBetweenEnemyAndMe(actualPosition);
        if (dist > attackRangeInHexTiles)
        {
            state = State.Movement;
        }
        else
        {
            state = State.Attack;
            timeStepCount = 0;
        }

    }

    public void DamageMe()
    {
        if (state == State.Death) { return; }

        currentLifePoint -= 1;

        if (currentLifePoint <= 0)
        {
            state = State.Death;
        }
    }

    public Vector3 CalculateNextStep()
    {
        Vector3 posForContinuousViz = transform.position;

        int stepCount = 0;

        while (stepCount < hexesPerTimeStep)
        {
            Vector3 hexSideDirection = Vector3.forward;

            Vector3 minDistanceDirectionVector = Vector3.zero;

            Vector3 target = SimulationManager.GetLocationOfUnit(colorCode == ColorCode.Red ? ColorCode.Blue : ColorCode.Red);

            float minDotProduct = 100f;

            for (int i = 0; i < 6; i++)
            {
                float dotProduct = Vector3.Dot(target - posForContinuousViz, -hexSideDirection);
                if (dotProduct < minDotProduct)
                {
                    minDotProduct = dotProduct;
                    minDistanceDirectionVector = hexSideDirection;
                }

                hexSideDirection = Quaternion.AngleAxis(60, Vector3.up) * hexSideDirection;
            }

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(posForContinuousViz + minDistanceDirectionVector * 10f, -Vector3.up, out hit, 100f))
            {
                GridElement gridElement = hit.transform.GetComponentInParent<GridElement>();

                if (gridElement != null)
                {
                    Vector3 pos = gridElement.GetBackPosition();
                    posForContinuousViz = pos;
                }
            }

            stepCount += 1;

            if (stepCount > 10000)
            {
                Debug.LogWarning("too much steps");
                break;
            }

            if (GetHexDistanceBetweenEnemyAndMe(posForContinuousViz) <= attackRangeInHexTiles)
            {
                return posForContinuousViz;
            }
        }

        return posForContinuousViz;
    }

    void Step()
    {
        actualPosition = locationUsedForTarget;
        transform.position = actualPosition;
        SimulationManager.UpdatePosition(colorCode, actualPosition);

        locationUsedForTarget = CalculateNextStep();
    }

    int GetHexDistanceBetweenEnemyAndMe(Vector3 positionCompareTo)
    {
        Vector3 posForDistanceChecking = positionCompareTo;
        Vector3 target = SimulationManager.GetLocationOfUnit(colorCode == ColorCode.Red ? ColorCode.Blue : ColorCode.Red);
        int stepCount = 0;

        checker.position = positionCompareTo;

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

            if (stepCount > 10000)
            {
                Debug.LogWarning("too much steps");
                break;
            }
        }

        return stepCount;
    }

    private void FixedUpdate()
    {
        if (!isInitialized) { return; }

        if (state == State.Movement)
        {
            Step();

            StateCheckingAndSwitchingIfNeeded();

        }
        else if (state == State.Attack)
        {
            timeStepCount += 1;

            if (timeStepCount == timeStepsPerAttack)
            {
                UnitController opponentUnit = SimulationManager.GetOpponent(colorCode == ColorCode.Blue ? ColorCode.Red : ColorCode.Blue);

                opponentUnit.DamageMe();

                timeStepCount = 0;
            }
        }

        actualTime = Time.time;
    }

    private void Update()
    {
        VisualizeDamage();

        VisualizeState();

        MoveContinuousVisualParent();
    }

    void VisualizeDamage()
    {
        // Get the current material color
        Color currentColor = renderer.material.color;

        // Convert the color to HSV
        Color.RGBToHSV(currentColor, out float h, out float s, out float v);

        // Modify the S value
        s = (float)currentLifePoint / (float)maxLifePoint;

        // Convert the modified HSV color back to RGB
        Color newColor = Color.HSVToRGB(h, s, v);

        // Set the material color to the new color
        renderer.material.color = newColor;
    }

    void VisualizeState()
    {
        attackParentGameObject.SetActive(state == State.Attack);
        dieParentGameObject.SetActive(state == State.Death);
        continuousVisualParent.gameObject.SetActive(state == State.Movement);
    }

    void MoveContinuousVisualParent()
    {
        if (state == State.Movement)
        {
            float currentTime = Time.time - actualTime;
            continuousVisualParent.position = Vector3.Lerp(transform.position, locationUsedForTarget, currentTime / Time.fixedDeltaTime);
        }

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
    Death,
}