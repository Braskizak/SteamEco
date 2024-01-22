using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Evolver : MonoBehaviour
{
    public GameObject organismPrefab;
    public Stats stats;
    public new GameObject camera;

    private List<GameObject> organisms = new List<GameObject>();
    private int generation = 1;
    public int generationSize = 50;
    public int keep = 5;
    public int maxAge = 10;
    public float generationLength = 15;

    // Start is called before the first frame update
    void Start()
    {
        
        // StartCoroutine(Evolve());
        StartCoroutine(Camera());
    }

    IEnumerator Camera()
    {
        while (true)
        {
            if (organisms.Count == 0)
            {
                yield return new WaitForSeconds(0.02f);
                continue;
            }
            organisms.Sort((x, y) => x.transform.Find("Head").transform.position.x.CompareTo(y.transform.Find("Head").transform.position.x));
            Vector3 bestPosition = organisms[organisms.Count - 1].transform.Find("Head").transform.position;
            if (camera)
            {
                camera.transform.position = new Vector3(bestPosition.x, camera.transform.position.y, camera.transform.position.z);
            }
            yield return new WaitForSeconds(0.02f);
        }

    }

    private void SortOrganisms()
    {
        organisms.Sort((x, y) => x.GetComponent<Organism>().Score().CompareTo(y.GetComponent<Organism>().Score()));
    }

    public void StartEvolving()
    {
        for (int i = 0; i < generationSize; i++)
        {
            GameObject organism = Instantiate(organismPrefab);
            organism.transform.position = transform.position;
            Organism org = organism.GetComponent<Organism>();
            org.Randomize();
            organisms.Add(organism);
        }
        StartCoroutine(Evolve());
    }

    // evolver coroutine
    IEnumerator Evolve()
    {
        Debug.Log("Generation: " + generation);

        // wait for 10 seconds
        yield return new WaitForSeconds(generationLength);
        // organisms.Sort((x, y) => x.transform.position.x.CompareTo(y.transform.position.x));
        // sort organisms by the X position of their "Head" component instead
        organisms.Sort((x, y) => x.GetComponent<Organism>().Score().CompareTo(y.GetComponent<Organism>().Score()));

        // capture the dna from the top 3
        List<List<List<float>>> distances = new List<List<List<float>>>();
        List<float> flexSteps = new List<float>();
        for (int i = 0; i < keep; i++)
        {
            Organism org = organisms[organisms.Count - 1 - i].GetComponent<Organism>();
            distances.Add(org.distances);
            flexSteps.Add(org.flexStep);
        }

        Vector3 bestPosition = organisms[organisms.Count - 1].transform.Find("Head").transform.position;
        float best = bestPosition.x;
        stats.best = best;
        if (best > stats.record)
        {
            stats.record = best;
        }

        if (camera != null)
        {
            camera.transform.position = new Vector3(bestPosition.x, bestPosition.y, camera.transform.position.z);
        }

        // destroy all organisms
        // for (int i = 0; i < generationSize; i++)
        // {
        //     Destroy(organisms[i]);
        // }
        // organisms.Clear();

        List<GameObject> toRemove = new List<GameObject>();
        // increase age of all organisms. destroy those that are too old

        for (int i = 0; i < organisms.Count; i++)
        {
            Organism org = organisms[i].GetComponent<Organism>();
            org.age++;
            org.alive = false;
            

            if (org.age > maxAge)
            {
                toRemove.Add(organisms[i]);
            }
        }
        foreach (GameObject organism in toRemove)
        {
            organisms.Remove(organism);
            Destroy(organism);
        }

        // destroy all but the top organisms (keep)
        while (organisms.Count > keep)
        {
            GameObject organism = organisms[0];
            organisms.Remove(organism);
            Destroy(organism);
        }

        // reset the top organisms by re-instantiating and calling Reset
        for (int i = 0; i < keep; i++)
        {
            GameObject oldOrganism = organisms[i];
            Organism oldOrg = oldOrganism.GetComponent<Organism>();
            GameObject organism = Instantiate(organismPrefab);
            organism.transform.position = transform.position;
            Organism org = organism.GetComponent<Organism>();
            org.Reset(oldOrg.distances, oldOrg.age, oldOrg.flexStep);
            organisms[i] = organism;
            Destroy(oldOrganism);
        }

        // create generationSize new organisms with mutated dna from the top 3
        while (organisms.Count < generationSize)
        {
            GameObject organism = Instantiate(organismPrefab);
            organism.transform.position = transform.position;
            Organism org = organism.GetComponent<Organism>();
            int which = Random.Range(0, keep);
            org.Mutate(distances[which], flexSteps[which]);
            organisms.Add(organism);
        }
        generation++;
        stats.generation = generation;
        // repeat
        StartCoroutine(Evolve());
    }

    public void SaveGeneration()
    {
        SortOrganisms();
        // grab the top (last) 5 organisms
        List<GameObject> topOrganisms = new List<GameObject>();
        for (int i = 0; i < 5; i++)
        {
            topOrganisms.Add(organisms[organisms.Count - 1 - i]);
        }
        SavedGeneration savedGeneration = new SavedGeneration();
        savedGeneration.generation = generation;
        foreach (GameObject organism in topOrganisms)
        {
            Organism org = organism.GetComponent<Organism>();
            SavedOrganism savedOrganism = new SavedOrganism();
            savedOrganism.score = org.Score();
            savedOrganism.flexStep = org.flexStep;
            foreach (List<float> frames in org.distances)
            {
                savedOrganism.distances.Add(new SavedSpringData{
                    frames = frames
                });
            }
            savedGeneration.organisms.Add(savedOrganism);
        }
        string json = JsonUtility.ToJson(savedGeneration);
        Debug.Log("Path is: " + Application.persistentDataPath + "/generation.json");
        File.WriteAllText(Application.persistentDataPath + "/generation.json", json);
    }

    public void LoadGeneration()
    {
        string json = File.ReadAllText(Application.persistentDataPath + "/generation.json");
        SavedGeneration savedGeneration = JsonUtility.FromJson<SavedGeneration>(json);
        generation = savedGeneration.generation;
        stats.generation = generation;
        foreach (SavedOrganism savedOrganism in savedGeneration.organisms)
        {
            GameObject organism = Instantiate(organismPrefab);
            organism.transform.position = transform.position;
            Organism org = organism.GetComponent<Organism>();
            List<List<float>> distances = new();
            foreach (SavedSpringData savedSpringData in savedOrganism.distances)
            {
                distances.Add(savedSpringData.frames);
            }
            org.Reset(distances, 0, savedOrganism.flexStep);
            organisms.Add(organism);
        }
        while (organisms.Count < generationSize)
        {
            GameObject organism = Instantiate(organismPrefab);
            organism.transform.position = transform.position;
            Organism org = organism.GetComponent<Organism>();
            int which = Random.Range(0, keep);
            org.Mutate(organisms[which].GetComponent<Organism>());
            organisms.Add(organism);
        }
        StartCoroutine(Evolve());
    }

}
