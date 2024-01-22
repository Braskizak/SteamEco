using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class WalkingState : MonoBehaviour
{
    public List<LegMotor> legMotors = new();
    public GameObject head;
    public GameObject pointOfInterest;

    // threshold from which this state will transition to stationary
    private const float poiDistanceThreshold = 4f;

    public void Execute()
    {
        StartCoroutine(EnterState());
    }

    IEnumerator EnterState()
    {
        Debug.Log("WalkingState.EnterState()");
        // bool speedingUp = true;
        // bool slowingDown = false;
        float currentSpeed = 150f;
        foreach (LegMotor legMotor in legMotors)
        {
            legMotor.Speed = currentSpeed;
        }
        // target stepInterval is 0.02f
        while (true)
        {
            // wait until stepping has stopped
            var stepping = Helpers.IsStepping(legMotors);
            yield return new WaitForSeconds(0.01f);
            if (pointOfInterest == null)
            {
                Debug.Log("No point of interest");
                gameObject.GetComponent<StationaryState>().Execute();
                yield break;
            }
            Vector2 targetPosition = pointOfInterest.transform.position;
            Vector2 headPosition = head.transform.position;
            if (Mathf.Abs(targetPosition.x - headPosition.x) < poiDistanceThreshold)
            {
                Debug.Log("Reached point of interest");
                gameObject.GetComponent<StationaryState>().Execute();
                yield break;
            }
            if (stepping)
            {
                continue;
            }

            float direction = targetPosition.x < headPosition.x ? -1f : 1f;
            LegMotor legMotor = Helpers.SelectNextLegForWalking(legMotors, direction);

            float stepDistance = Random.Range(3f, 5f);

            // check terrain height at new foot position
            float terrainHeight = Helpers.GetTerrainHeight(new Vector2(legMotor.FootPosition.x + direction * stepDistance, headPosition.y));
            // TODO: here is where we might want to jump if there is a gap
            // TODO: maybe use fall height threshold instead of negative infinity
            // TODO: falling state?
            if (terrainHeight == -float.NegativeInfinity)
            {
                float targetTerrainHeight = Helpers.GetTerrainHeight(targetPosition);
                if (targetTerrainHeight == -float.NegativeInfinity)
                {
                    gameObject.GetComponent<StationaryState>().Execute();
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }
            legMotor.Step(new Vector2(legMotor.FootPosition.x + direction * stepDistance, terrainHeight));
        }
    }
}
