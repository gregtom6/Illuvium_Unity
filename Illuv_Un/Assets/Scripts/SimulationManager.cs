using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [SerializeField] Transform redUnit;
    [SerializeField] Transform blueUnit;
    [SerializeField] UnitController redUnitController;
    [SerializeField] UnitController blueUnitController;

    Dictionary<ColorCode, Vector3> unitsAndTheirActualLocations = new Dictionary<ColorCode, Vector3>();

    static SimulationManager instance;

    // Start is called before the first frame update
    void OnEnable()
    {
        GridCreator.gridCreationFinished += OnGridCreationFinished;

    }

    private void OnDisable()
    {
        GridCreator.gridCreationFinished -= OnGridCreationFinished;
    }

    private void Start()
    {
        instance = this;
    }

    void OnGridCreationFinished()
    {
        Vector3 position = GridCreator.GetRandomPosition();
        redUnit.position = position;

        unitsAndTheirActualLocations.Add(ColorCode.Red, position);

        Vector3 newPosition = Vector3.zero;
        do
        {
            newPosition = GridCreator.GetRandomPosition();
        } while (newPosition == position);

        blueUnit.position = newPosition;

        unitsAndTheirActualLocations.Add(ColorCode.Blue, newPosition);

        redUnitController.Initialize();
        blueUnitController.Initialize();
    }

    public static Vector3 GetLocationOfUnit(ColorCode colorCode)
    {
        if(instance== null) { return Vector3.zero; }
        return instance.unitsAndTheirActualLocations[colorCode];
    }
}
