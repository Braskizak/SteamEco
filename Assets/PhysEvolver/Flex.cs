using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flex : MonoBehaviour
{

    public List<GameObject> bomes = new List<GameObject>();
    private List<SpringJoint2D> springs = new List<SpringJoint2D>();
    private List<float> distances = new List<float>();

    public KeyCode triggerKey = KeyCode.A;
    public KeyCode triggerKey2 = KeyCode.S;

    public float innerFlex = 0.7f;
    public float defaultFlex = 1f;
    public float outerFlex = 1.2f;

    // Start is called before the first frame update
    void Start()
    {
        // each bone may have more then one spring joint 2d
        foreach (GameObject bone in bomes)
        {
            SpringJoint2D[] springJoints = bone.GetComponents<SpringJoint2D>();
            foreach (SpringJoint2D spring in springJoints)
            {
                springs.Add(spring);
                distances.Add(spring.distance);
            }
        }
        
    }

    // Apply muscle force by setting each spring to 1.5x its original length when the spacebar is pressed and held
    void FixedUpdate()
    {
        if (Input.GetKey(triggerKey))
        {
            for (int i = 0; i < springs.Count; i++)
            {
                springs[i].distance = distances[i] * innerFlex;
            }
        }
        else if (Input.GetKey(triggerKey2))
        {
            for (int i = 0; i < springs.Count; i++)
            {
                springs[i].distance = distances[i] * outerFlex;
            }
        }
        else
        {
            for (int i = 0; i < springs.Count; i++)
            {
                springs[i].distance = distances[i] * defaultFlex;
            }
        }
    }

  
}
