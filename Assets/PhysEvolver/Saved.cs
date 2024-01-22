using System;
using System.Collections.Generic;

[Serializable]
public class SavedGeneration
{
    public List<SavedOrganism> organisms = new List<SavedOrganism>();
    public int generation;
    // Class members go here
}

[Serializable]
public class SavedOrganism
{
    public float flexStep = 0.05f;
    public List<SavedSpringData> distances = new();
    public float score;
    // Class members go here
}

[Serializable]
public class SavedSpringData {
    public List<float> frames = new();
}