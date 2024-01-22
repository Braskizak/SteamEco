using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

public struct LegData
{
    public float upperLegLength;
    public float lowerLegLength;
    public Vector2 upperLegAnchor;
    public GameObject upperLeg;
    public GameObject lowerLeg;
}

public enum KneeDirection
{
    Left,
    Right
}

public class MoveTest : MonoBehaviour
{
    public GameObject head;
    public GameObject leftLeg;
    public GameObject middleLeg;
    public GameObject rightLeg;

    private LegMotor leftLegMotor;
    private LegMotor middleLegMotor;
    private LegMotor rightLegMotor;

    // keep leg motors in a list so we can alternate
    private List<LegMotor> legMotors = new List<LegMotor>();
    private int selectedLegMotorIndex = 2;

    void Start()
    {
        leftLegMotor = leftLeg.GetComponent<LegMotor>();
        middleLegMotor = middleLeg.GetComponent<LegMotor>();
        rightLegMotor = rightLeg.GetComponent<LegMotor>();
        legMotors.Add(leftLegMotor);
        legMotors.Add(middleLegMotor);
        legMotors.Add(rightLegMotor);

        float terrainHeight = Helpers.GetTerrainHeight(head.transform.position);
        // set the head to terrain height + 2
        head.transform.position = new Vector2(head.transform.position.x, terrainHeight + 3f);
        leftLegMotor.InitFootPosition(new Vector2(leftLegMotor.gameObject.transform.position.x, terrainHeight));
        middleLegMotor.InitFootPosition(new Vector2(middleLegMotor.gameObject.transform.position.x, terrainHeight));
        rightLegMotor.KneeDirection = KneeDirection.Right;
        rightLegMotor.InitFootPosition(new Vector2(rightLegMotor.gameObject.transform.position.x, terrainHeight));

        GameObject pointOfInterest = GameObject.FindObjectOfType<MouseFollower>().gameObject;
        StationaryState stationaryState = gameObject.AddComponent<StationaryState>();
        stationaryState.legMotors.AddRange(legMotors);
        stationaryState.head = head;
        stationaryState.pointOfInterest = pointOfInterest;

        WalkingState walkingState = gameObject.AddComponent<WalkingState>();
        walkingState.legMotors.AddRange(legMotors);
        walkingState.head = head;
        walkingState.pointOfInterest = pointOfInterest;

        stationaryState.Execute();
    }

    void FixedUpdate()
    {
        // get the footPosition of all legs
        Vector2 leftFootPosition = leftLegMotor.FootPosition;
        Vector2 middleFootPosition = middleLegMotor.FootPosition;
        Vector2 rightFootPosition = rightLegMotor.FootPosition;

        // average the X position of all feet
        float averageX = (leftFootPosition.x + middleFootPosition.x + rightFootPosition.x) / 3f;
        float averageY = (leftFootPosition.y + middleFootPosition.y + rightFootPosition.y) / 3f;
        
        float heightDiff = (averageY + 3f) - head.transform.position.y;
        float xDiff = averageX - head.transform.position.x;

        // set the head to the average X position
        head.transform.position = new Vector2(head.transform.position.x + xDiff * 0.3f, head.transform.position.y + heightDiff * 0.05f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LegMotor legMotor = legMotors[selectedLegMotorIndex];
            Vector2 footPosition = legMotor.FootPosition;
            legMotor.Step(footPosition + new Vector2(4f, 0f));
            selectedLegMotorIndex = (selectedLegMotorIndex + 1) % legMotors.Count;
        }

    }
}

