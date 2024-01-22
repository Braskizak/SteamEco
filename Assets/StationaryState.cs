using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationaryState : MonoBehaviour
{
    public List<LegMotor> legMotors = new();
    public GameObject head;
    public GameObject pointOfInterest;

    // threshold from which this state will transition to walking
    private const float poiDistanceThreshold = 4f;

    public void Execute()
    {
        StartCoroutine(EnterState());
    }

    // Execute coroutine
    IEnumerator EnterState()
    {
        Debug.Log("StationaryState.Execute()");
        while (true)
        {
            // wait until stepping has stopped
            var stepping = Helpers.IsStepping(legMotors);
            yield return new WaitForSeconds(0.01f);
            if (stepping)
            {
                continue;
            }

            LegMotor stretched = Helpers.SelectNextLegForIdle(legMotors);
            if (stretched != null)
            {
                Debug.Log("Resetting leg");
                float terrainHeightAtTarget = Helpers.GetTerrainHeight(stretched.LegAnchor);
                stretched.Step(new Vector2(stretched.LegAnchor.x, terrainHeightAtTarget));
                continue;
            }
            if (pointOfInterest != null)
            {
                Vector2 targetPosition = pointOfInterest.transform.position;
                float targetTerrainHeight = Helpers.GetTerrainHeight(targetPosition);
                if (targetTerrainHeight == -float.NegativeInfinity)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }
                Vector2 headPosition = head.transform.position;
                if (Mathf.Abs(targetPosition.x - headPosition.x) > poiDistanceThreshold)
                {
                    Debug.Log("Distance to POI is greater than threshold, walking");
                    gameObject.GetComponent<WalkingState>().Execute();
                    yield break;
                }
            }

        }

    }
}
