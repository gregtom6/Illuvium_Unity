using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UnitController : MonoBehaviour
{
    [SerializeField] ColorCode colorCode;
    [SerializeField] MeshRenderer renderer;
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
        LifeSetting();
    }

    void LifeSetting()
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
            locationUsedForTarget = CalculateNextStepWithUnitSpeed();
        }
    }

    public void StateCheckingAndSwitchingIfNeeded()
    {
        int dist = GetHexDistanceBetweenEnemyAndParameterPos(actualPosition);
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

    public Vector3 CalculateNextStepWithUnitSpeed()
    {
        Vector3 tempPos = transform.position;

        int stepCount = 0;

        while (stepCount < hexesPerTimeStep)
        {
            tempPos = DecideBestNextPosition(tempPos);

            stepCount += 1;

            if (stepCount > 10000)
            {
                Debug.LogWarning("too much steps");
                break;
            }

            if (GetHexDistanceBetweenEnemyAndParameterPos(tempPos) <= attackRangeInHexTiles)
            {
                return tempPos;
            }
        }

        return tempPos;
    }

    void GetBackMinDotProduct(out float minDotProduct, out Vector3 minDistanceDirectionVector, Vector3 fromVector, Vector3 toVector)
    {
        minDotProduct = 100f;
        minDistanceDirectionVector = Vector3.zero;

        for (int i = 0; i < 6; i++)
        {
            float dotProduct = Vector3.Dot(fromVector, -toVector);
            if (dotProduct < minDotProduct)
            {
                minDotProduct = dotProduct;
                minDistanceDirectionVector = toVector;
            }

            toVector = Quaternion.AngleAxis(60, Vector3.up) * toVector;
        }
    }

    void Step()
    {
        actualPosition = locationUsedForTarget;
        transform.position = actualPosition;
        SimulationManager.UpdatePosition(colorCode, actualPosition);

        locationUsedForTarget = CalculateNextStepWithUnitSpeed();
    }

    int GetHexDistanceBetweenEnemyAndParameterPos(Vector3 positionCompareTo)
    {
        Vector3 tempPos = positionCompareTo;
        Vector3 target = SimulationManager.GetLocationOfUnit(colorCode == ColorCode.Red ? ColorCode.Blue : ColorCode.Red);
        int stepCount = 0;

        while (tempPos != target)
        {
            tempPos = DecideBestNextPosition(tempPos);

            stepCount += 1;

            if (stepCount > 10000)
            {
                Debug.LogWarning("too much steps");
                break;
            }
        }

        return stepCount;
    }

    Vector3 DecideBestNextPosition(Vector3 pooo)
    {
        Vector3 hexSideDirection = Vector3.forward;

        Vector3 minDistanceDirectionVector = Vector3.zero;

        Vector3 target = SimulationManager.GetLocationOfUnit(colorCode == ColorCode.Red ? ColorCode.Blue : ColorCode.Red);

        float minDotProduct = 100f;

        GetBackMinDotProduct(out minDotProduct, out minDistanceDirectionVector, target - pooo, hexSideDirection);

        RaycastHit hit;
        if (Physics.Raycast(pooo + minDistanceDirectionVector * 10f, -Vector3.up, out hit, 100f))
        {
            GridElement gridElement = hit.transform.GetComponentInParent<GridElement>();

            if (gridElement != null)
            {
                Vector3 pos = gridElement.GetBackPosition();
                pooo = pos;
                return pooo;
            }
        }

        return Vector3.zero;
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
        Color currentColor = renderer.material.color;
        Color.RGBToHSV(currentColor, out float h, out float s, out float v);

        s = (float)currentLifePoint / (float)maxLifePoint;

        Color newColor = Color.HSVToRGB(h, s, v);

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