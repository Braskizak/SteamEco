using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Organism : MonoBehaviour
{
    public List<GameObject> bomes = new List<GameObject>();
    private List<SpringJoint2D> springs = new List<SpringJoint2D>();
    private List<float> initialDistances = new List<float>();
    public List<List<float>> distances = new List<List<float>>();
    public int age = 0;

    const float minFlex = 0.7f;
    const float maxFlex = 1.2f;
    const int numFlexes = 50;
    public float flexStep = 0.05f;
    public bool alive = false;

    public void Randomize()
    {
        Init();
        for (int j = 0; j < springs.Count; j++)
        {
            for (int i = 0; i < numFlexes; i++)
            {
                float randomDistance = Random.Range(minFlex, maxFlex);
                distances[j][i] = randomDistance;
            }
        }
        StartCoroutine(Flex());
    }

    public void Mutate(Organism parent)
    {
        Mutate(parent.distances, parent.flexStep);
    }

    public void Mutate(List<List<float>> distances, float flexStep)
    {
        Init();
        // randomly mutate
        for (int i = 0; i < this.distances.Count; i++)
        {
            for (int j = 0; j < this.distances[i].Count; j++)
            {
                this.distances[i][j] = distances[i][j];
                if (Random.Range(0f, 1f) < 0.1f)
                {
                    this.distances[i][j] = distances[i][j] * Random.Range(0.95f, 1.05f);
                    if (this.distances[i][j] < minFlex)
                    {
                        this.distances[i][j] = minFlex;
                    }
                    else if (this.distances[i][j] > maxFlex)
                    {
                        this.distances[i][j] = maxFlex;
                    }
                }
            }
        }
        this.flexStep = flexStep;
        if (Random.Range(0f, 1f) < 0.1f)
        {
            this.flexStep = flexStep * Random.Range(0.95f, 1.05f);
        }
        StartCoroutine(Flex());
    }

    public void Reset(List<List<float>> distances, int age, float flexStep)
    {
        Init();
        // inherit from the parent
        for (int i = 0; i < this.distances.Count; i++)
        {
            for (int j = 0; j < this.distances[i].Count; j++)
            {
                this.distances[i][j] = distances[i][j];
            }
        }
        this.age = age;
        this.flexStep = flexStep;
        StartCoroutine(Flex());
    }

    public void Init()
    {
// each bone may have more then one spring joint 2d
        foreach (GameObject bone in bomes)
        {
            SpringJoint2D[] springJoints = bone.GetComponents<SpringJoint2D>();
            foreach (SpringJoint2D spring in springJoints)
            {
                springs.Add(spring);
                initialDistances.Add(spring.distance);
                this.distances.Add(new List<float>());
                for (int i = 0; i < numFlexes; i++)
                {
                    distances[^1].Add(1);
                }
            }
        }
    }

    public float Score()
    {
        GameObject head = gameObject.transform.Find("Head").gameObject;
        float baseScore = head.transform.position.x;
        return baseScore;
    }

    IEnumerator Flex()
    {
        if (alive)
        {
            yield break;
        }
        alive = true;
        // wait for 1 second
        yield return new WaitForSeconds(2);
        while (alive)
        {
            for (int i = 0; i < numFlexes; i++)
            {
                for (int j = 0; j < springs.Count; j++)
                {
                    if (!alive)
                    {
                        yield break;
                    }
                    springs[j].distance = distances[j][i] * initialDistances[j];
                }
                yield return new WaitForSeconds(flexStep);
            }
            
        }
    }

    // // Update is called once per frame
    // void FixedUpdate()
    // {

    // }
}
