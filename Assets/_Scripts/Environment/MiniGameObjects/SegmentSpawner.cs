using System.Collections.Generic;
using StarWriter.Core;
using UnityEngine;


public enum PositioningScheme
{
    SphereUniform = 0,
    SphereSurface = 1,
    StraightLine = 2,
    KinkyLine = 3,
    CurvyLine = 4,
    ToroidSurface = 5,
    Cubic = 6,
}

public class SegmentSpawner : MonoBehaviour
{
    [SerializeField] TrailBlock trailBlock;
    [SerializeField] int numberOfSegments = 1;
    [SerializeField] List<SpawnableAbstractBase> spawnableSegments;
    [SerializeField] PositioningScheme positioningScheme = PositioningScheme.SphereUniform;
    [SerializeField] List<float> spawnSegmentWeights;
    [SerializeField] int Seed;
    List<Trail> trails = new List<Trail>();
    System.Random random = new System.Random();

    int index;
    //[SerializeField] float aa;
    //[SerializeField] float bb;

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (Seed != 0) random = new System.Random(Seed);

        // Clear out last run
        foreach (Trail trail in trails) { 
            foreach (var block in trail.TrailList)
            {
                Destroy(block);
            }    
        }
        trails.Clear();

        float totalWeight = 0;
        foreach (var weight in spawnSegmentWeights)
        {
            totalWeight+= weight;
        }
        for (int i=0; i<spawnSegmentWeights.Count; i++)
        {
            spawnSegmentWeights[i] = spawnSegmentWeights[i] * (1 / totalWeight);
        }

        
        for (int i=0; i < numberOfSegments; i++)
        {
            var spawned = SpawnRandom();
            PositionSpawnedObject(spawned, positioningScheme);
            index++;
        }

        //var spawned = SpawnRandom();
        //spawned.transform.position = new Vector3(90, 90, 0);
        //PositionSpawnedObject(spawned, 0);

        //spawned = SpawnRandom();
        //spawned.transform.position = new Vector3(-90, 90, 0);
        //PositionSpawnedObject(spawned, 0);

        //spawned = SpawnRandom();
        //spawned.transform.position = new Vector3(-90, -90, 0);
        //PositionSpawnedObject(spawned, 0);

        //spawned = SpawnRandom();
        //spawned.transform.position = new Vector3(90, -90, 0);
        //PositionSpawnedObject(spawned, 0);
    }

    float sphereRadius = 200f;

    void PositionSpawnedObject(GameObject spawned, PositioningScheme positioningScheme)
    {
        switch (positioningScheme)
        {
            case PositioningScheme.SphereUniform:
                spawned.transform.SetPositionAndRotation(Random.insideUnitSphere * sphereRadius, Random.rotation);
                return;
            case PositioningScheme.SphereSurface:
                
                int difficultyAngle = 90;
                spawned.transform.position = Quaternion.Euler(0, 0, random.Next(index * (360/ numberOfSegments), index * (360 / numberOfSegments) + 20)) *
                    (Quaternion.Euler(0, random.Next(Mathf.Max(difficultyAngle - 20, 40), Mathf.Max(difficultyAngle - 20, 40)), 0) *
                    (sphereRadius * Vector3.forward));
                spawned.transform.LookAt(Vector3.zero);
                return;
            case PositioningScheme.Cubic:
                // Volumetric Grid, looking at origin
                var volumeSideLength = 100;
                var voxelSideLength = 10;
                var x = random.Next(0, volumeSideLength/voxelSideLength) * voxelSideLength;
                var y = random.Next(0, volumeSideLength/voxelSideLength) * voxelSideLength;
                var z = random.Next(0, volumeSideLength/voxelSideLength) * voxelSideLength;
                spawned.transform.position = new Vector3(x, y, z);
                spawned.transform.LookAt(Vector3.zero, Vector3.up);
                break;
            //case 1:
            //    // Put it at 90, 90, 0
            //    spawned.transform.position = new Vector3(90, 90, 0);
            //    break;
            //case 2:
            //    break;
        }
    }

    GameObject SpawnRandom()
    {
        var spawnWeight = random.NextDouble();
        var spawnIndex = 0;
        var totalWeight = 0f;
        for (int i = 0; i < spawnSegmentWeights.Count && totalWeight < spawnWeight; i++)
        {
            spawnIndex = i;
            totalWeight += spawnSegmentWeights[i];
        }

        return spawnableSegments[spawnIndex].Spawn();
    }

    /*
    void CreateSpiralSegment()
    {
        GameObject container = new GameObject();
        container.name = "spiral";

        //x(t) = aa � exp(bb � t) � cos(t) y(t) = aa � exp(bb � t) � sin(t).
        var trail = new Trail();

        float blockCount = Mathf.PI * 8;
        for (float block = .1f; block < blockCount; block += .2f) //(1.2f-block/blockCount)
        {
            var t = ((float)block / (float)blockCount);
            // x = 16sin^3(t)
            var x = aa * Mathf.Exp(bb*t) * Mathf.Cos(t);
            var y = aa * Mathf.Exp(bb*t) * Mathf.Sin(t);
            var z = (block / blockCount) * 12;
            var position = new Vector3(x, y, z);
            CreateBlock(position, Vector3.zero, "SEGMENT::" + block, trail, Vector3.one, trailBlock, container);
        }

        trails.Add(trail);
    }

    void CreateBlock(Vector3 position, Vector3 lookPosition, string blockId, Trail trail, Vector3 scale, TrailBlock trailBlock, GameObject container)
    {
        var Block = Instantiate(trailBlock);
        Block.Team = Teams.None;
        Block.ownerId = "public";
        Block.PlayerName = "";
        Block.transform.SetPositionAndRotation(position, Quaternion.LookRotation(lookPosition - transform.position, transform.forward));
        Block.transform.SetParent(container.transform, false);
        Block.ID = blockId;
        Block.InnerDimensions = scale;
        Block.Trail = trail;
        trail.Add(Block);
    }
    */
}