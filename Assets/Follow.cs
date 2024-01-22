using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public GameObject target;
    // Start is called before the first frame update
    
    void FixedUpdate()
    {
       
        transform.position = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);
    }
}
