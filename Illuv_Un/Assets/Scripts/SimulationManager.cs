using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [SerializeField] Transform redUnit;
    [SerializeField] Transform blueUnit;

    Dictionary<UnitController, Vector3> unitsAndTheirActualLocations = new Dictionary<UnitController, Vector3>();

    // Start is called before the first frame update
    void OnEnable()
    {
        GridCreator.gridCreationFinished += OnGridCreationFinished;

    }

    private void OnDisable()
    {
        GridCreator.gridCreationFinished -= OnGridCreationFinished;
    }

    void OnGridCreationFinished()
    {
        Vector3 position = GridCreator.GetRandomPosition();
        redUnit.position = position;
        Vector3 newPosition = Vector3.zero;
        do
        {
            newPosition = GridCreator.GetRandomPosition();
        } while (newPosition == position);

        blueUnit.position = newPosition;
    }
}
