using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegMotor : MonoBehaviour
{
    public GameObject head;
    private LegData legData;
    private Vector2 footPosition;
    private Vector2 desiredFootPosition;
    private KneeDirection kneeDirection = KneeDirection.Left;
    private bool stepping = false;
    private float stepInterval = 0.02f;
    const float stepHeight = 2f;

    public KneeDirection KneeDirection
    {
        get { return kneeDirection; }
        set { kneeDirection = value; }
    }

    // get foot position
    public Vector2 FootPosition
    {
        get { return footPosition; }
    }

    public Vector2 LegAnchor
    {
        get { return legData.upperLegAnchor + (Vector2)head.transform.position; }
    }

    public float Speed {
        get { return 1f / stepInterval; }
        set { stepInterval = 1f / value; }
    }

    public bool Stepping {
        get { return stepping; }
    }

    void Start()
    {
        legData = buildLegData(gameObject);
    }

    private LegData buildLegData(GameObject leg)
    {
        LegData legData = new();
        legData.lowerLeg = leg.transform.Find("Lower").gameObject;
        legData.upperLeg = leg.transform.Find("Upper").gameObject;
        legData.lowerLegLength = legData.lowerLeg.GetComponent<SpriteRenderer>().bounds.size.y;
        legData.upperLegLength = legData.upperLeg.GetComponent<SpriteRenderer>().bounds.size.y;
        // get relative position of upper leg anchor relative to head
        Vector2 topOfUpperLeg = legData.upperLeg.transform.position + new Vector3(0, legData.upperLegLength / 2, 0);
        Vector2 headPos = head.transform.position;
        legData.upperLegAnchor = topOfUpperLeg - headPos;
        return legData;
    }

    public void Step(Vector2 desiredFootPosition)
    {
        if (stepping)
        {
            Debug.Log("Already stepping");
            return;
        }
        stepping = true;
        this.desiredFootPosition = desiredFootPosition;
        StartCoroutine(StepCoroutine());
    }

    public void InitFootPosition(Vector2 footPosition)
    {
        this.footPosition = footPosition;
        FlexLeg(footPosition, ref legData, kneeDirection);
    }

    IEnumerator StepCoroutine()
    {
        int numberOfFrames = UnityEngine.Random.Range(20, 30);
        float stepDistance = (desiredFootPosition - footPosition).magnitude;
        float stepDistancePerFrame = stepDistance / numberOfFrames;

        float stepHeightPerFrame = stepHeight / numberOfFrames;
        Vector2 stepDirection = (desiredFootPosition - footPosition).normalized;
        int frame = 0;
        // move foot upward for the first half of the frames
        while (stepping && frame < numberOfFrames / 2)
        {
            footPosition += stepDirection * stepDistancePerFrame;
            footPosition += Vector2.up * stepHeightPerFrame;
            // FlexLeg(footPosition, ref legData, kneeDirection);
            frame++;
            yield return new WaitForSeconds(stepInterval);
        }
        // move foot downward for the second half of the frames
        while (stepping && frame < numberOfFrames)
        {
            footPosition += stepDirection * stepDistancePerFrame;
            footPosition -= Vector2.up * stepHeightPerFrame;
            // FlexLeg(footPosition, ref legData, kneeDirection);
            frame++;
            yield return new WaitForSeconds(stepInterval);
        }
        stepping = false;
    }

    void FixedUpdate()
    {
        FlexLeg(footPosition, ref legData, kneeDirection);
    }

    private void PlacePart(GameObject part, Vector2 point1, Vector2 point2, ref LegData legData)
    {
        // get angle of point2 relative to point1
        Vector2 point1ToPoint2 = point2 - point1;
        // Debug.Log(part.name + " - point1ToPoint2: " + point1ToPoint2 + " point1: " + point1 + " point2: " + point2);
        float angle = Mathf.Atan2(point1ToPoint2.y, point1ToPoint2.x) * Mathf.Rad2Deg - 90f;
        // set part rotation
        part.transform.rotation = Quaternion.Euler(0, 0, angle + 180f);
        // set part position based on point1(anchor) and point2
        float midPointX = Mathf.Sin(-angle * Mathf.Deg2Rad) * legData.upperLegLength / 2;
        float midPointY = Mathf.Cos(-angle * Mathf.Deg2Rad) * legData.upperLegLength / 2;
        Vector2 midPoint = new Vector2(midPointX, midPointY);
        part.transform.position = point1 + midPoint;
    }

    void FlexLeg(Vector2 desiredFootPosition, ref LegData legData, KneeDirection kneeDirection = KneeDirection.Left)
    {
        Vector2 upperLegAnchorPosition = (Vector2)head.transform.position + legData.upperLegAnchor;
        Vector2 fromUpperLegAnchor = desiredFootPosition - upperLegAnchorPosition;
        float angle = Mathf.Atan2(fromUpperLegAnchor.y, fromUpperLegAnchor.x) * Mathf.Rad2Deg - 90f;
        float maxFlex = (legData.upperLegLength + legData.lowerLegLength) * 0.9f;
        float targetFlex = Mathf.Min(fromUpperLegAnchor.magnitude, maxFlex);
        // Debug.Log("maxFlex: " + maxFlex + " targetFlex: " + targetFlex + " angle: " + angle);
        float footX = Mathf.Sin(-angle * Mathf.Deg2Rad) * targetFlex + upperLegAnchorPosition.x;
        float footY = Mathf.Cos(-angle * Mathf.Deg2Rad) * targetFlex + upperLegAnchorPosition.y;
        Vector2 footPosition = new Vector2(footX, footY);

        float halfFlex = targetFlex / 2;
        float halfFlexX = Mathf.Sin(-angle * Mathf.Deg2Rad) * halfFlex + upperLegAnchorPosition.x;
        float halfFlexY = Mathf.Cos(-angle * Mathf.Deg2Rad) * halfFlex + upperLegAnchorPosition.y;
        Vector2 halfFlexPosition = new Vector2(halfFlexX, halfFlexY);
        float distanceToKnee = Mathf.Sqrt(Mathf.Max(legData.upperLegLength * legData.upperLegLength - halfFlex * halfFlex, 1f));
        float angleToKnee = angle - 90f;
        if (kneeDirection == KneeDirection.Right)
        {
            angleToKnee = angle + 90f;
        }
        float kneeX = Mathf.Sin(-angleToKnee * Mathf.Deg2Rad) * distanceToKnee + halfFlexX;
        float kneeY = Mathf.Cos(-angleToKnee * Mathf.Deg2Rad) * distanceToKnee + halfFlexY;
        Vector2 kneePosition = new Vector2(kneeX, kneeY);
        // place upper leg
        PlacePart(legData.upperLeg, (Vector2)head.transform.position + legData.upperLegAnchor, kneePosition, ref legData);
        // place lower leg
        PlacePart(legData.lowerLeg, kneePosition, footPosition, ref legData);
    }
}
