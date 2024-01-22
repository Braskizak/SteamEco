using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stats : MonoBehaviour
{

    public int generation;
    public float best;
    public float record;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // get Text component from child named "Generation"
        Text generationText = transform.Find("Generation").GetComponent<Text>();
        generationText.text = "Generation: " + generation;
        // get Text component from child named "Best"
        Text bestText = transform.Find("Best").GetComponent<Text>();
        bestText.text = "Best: " + best;
        // get Text component from child named "Record"
        Text recordText = transform.Find("Record").GetComponent<Text>();
        recordText.text = "Record: " + record;
    }
}
