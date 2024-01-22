using System.Collections.Generic;
using UnityEngine;

public class Helpers {
    public static float GetTerrainHeight(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Terrain"));
        if (hit.collider != null)
        {
            return hit.point.y;
        }
        return -float.NegativeInfinity;
    }

    public static bool IsStepping(List<LegMotor> legMotors)
    {
        for (int i = 0; i < legMotors.Count; i++)
        {
            if (legMotors[i].Stepping)
            {
                return true;
            }
        }
        return false;
    }

    public static LegMotor SelectNextLegForWalking(List<LegMotor> legs, float direction)
    {
        // direction: -1 means left, 1 means right
        // for each leg motor, we need to subtract FootPosition.x - LegAnchor.x
        // if direction is -1, we want the leg with the highest value
        // if direction is 1, we want the leg with the lowest value
        float maxValue = -100;
        LegMotor result = null;
        foreach (LegMotor leg in legs)
        {
            float value = -direction * (leg.FootPosition.x - leg.LegAnchor.x);
            if (value > maxValue)
            {
                maxValue = value;
                result = leg;
            }
        }
        return result;
    }

    public static LegMotor SelectNextLegForIdle(List<LegMotor> legs)
    {
        // get the distance of each LegMotor's foot from the LegAnchor
        // if any are > 0.1f, return that leg
        foreach (LegMotor leg in legs)
        {
            float distance = Mathf.Abs(leg.FootPosition.x - leg.LegAnchor.x);
            if (distance > 1f)
            {
                return leg;
            }
        }
        return null;
    }
}