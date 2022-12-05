﻿using System.Collections;
using UnityEngine;
using System.Linq;

public class Trail : MonoBehaviour, IEntity
{
    [SerializeField] float fuelChange = -3f;
    [SerializeField] GameObject FossilBlock;
    [SerializeField] GameObject ParticleEffect;
    [SerializeField] Material material;
    [SerializeField] TrailBlockProperties trailBlockProperties;

    public string ownerId;
    public float waitTime = .6f;
    public bool embiggen;
    public bool destroyed = false;

    public bool warp = false;
    GameObject shards;

    public delegate void TrailCollision(string uuid, float amount);
    public static event TrailCollision OnTrailCollision;

    public delegate void OnCollisionIncreaseScore(string uuid, int amount);
    public static event OnCollisionIncreaseScore AddToScore;

    private int scoreChange = 1;
    private static GameObject container;
    private MeshRenderer meshRenderer;
    private BoxCollider blockCollider;
    Team team;
    EntityType entityType = EntityType.TrailBlock;
    public Team Team { get => team; set => team = value; }
    public EntityType EntityType { get => entityType; }

    void Start()
    {
        if (warp) shards = GameObject.FindGameObjectWithTag("field");

        if (container == null)
        {
            container = new GameObject();
            container.name = "FossilBlockContainer";
        }

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;

        blockCollider = GetComponent<BoxCollider>();
        blockCollider.enabled = false;

        if (embiggen) StartCoroutine(ToggleBlockCoroutine(2f));
        else StartCoroutine(ToggleBlockCoroutine(1f));
    }

    IEnumerator ToggleBlockCoroutine(float finalSize)
    {
        var finalTransformScale = transform.localScale;

        if (warp) finalTransformScale *= shards.GetComponent<WarpFieldData>().HybridVector(transform).magnitude;

        var size = 0.01f;

        yield return new WaitForSeconds(waitTime);

        transform.localScale = finalTransformScale * size;

        meshRenderer.enabled = true;
        blockCollider.enabled = true;

        while (size < finalSize)
        {
            size += .5f*Time.deltaTime;
            transform.localScale = finalTransformScale * size;
            yield return null;
        }

        // Add block to team score when created
        ScoringManager.Instance.UpdateTeamScore(team, trailBlockProperties.volume);
    }

    public void InstantiateParticle(Transform skimmer)
    {
        var particle = Instantiate(ParticleEffect);
        particle.transform.parent = transform;
        StartCoroutine(UpdateParticleCoroutine(particle, skimmer));
    }

    IEnumerator UpdateParticleCoroutine(GameObject particle, Transform skimmer)
    {
        var time = 50;
        var timer = 0;
        while (timer < time)
        {
            var distance =  transform.position - skimmer.position;
            particle.transform.localScale = new Vector3(1, 1, distance.magnitude);
            particle.transform.rotation = Quaternion.LookRotation(distance, transform.up);
            particle.transform.position = skimmer.position;
            timer++;

            yield return null;
        }
        Destroy(particle);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsShip(other.gameObject))
        {
            var ship = other.GetComponent<ShipGeometry>().Ship;
            var impactVector = ship.transform.forward * ship.GetComponent<ShipData>().speed;

            Collide(ship);
            Explode(impactVector, ship.Player.PlayerName);
        }
        else if (IsExplosion(other.gameObject))
        {
            if (other.GetComponent<AOEExplosion>().Team == Team)
                return;

            var speed = other.GetComponent<AOEExplosion>().speed;
            var impactVector = (transform.position - other.transform.position).normalized*speed;

            Explode(impactVector, "Player"); // TODO: need to attribute the explosion color to the team that made the explosion
        }
        else if (IsProjectile(other.gameObject))
        {
            if (other.GetComponent<Projectile>().Team == Team)
                return;

            var speed = other.GetComponent<Projectile>().Velocity;
            var impactVector = speed;

            Explode(impactVector, "Player"); // TODO: need to attribute the explosion color to the team that made the explosion
        }
    }

    public void Collide(Ship ship)
    {
        if (ownerId == ship.Player.PlayerUUID)
        {
            Debug.Log($"You hit you're teams tail - ownerId: {ownerId}, team: {team}");
        }
        else
        {
            Debug.Log($"Player ({ship.Player.PlayerUUID}) just gave player({ownerId}) a point via tail collision");
            AddToScore?.Invoke(ownerId, scoreChange);
        }

        //// Player Hit
        if (ship.Player == GameObject.FindWithTag("Player"))
        {
            // TODO: for now, we're only turning off collision on the player. In the future, we want AI ships to explode and all that too
            // TODO: turned off collision toggling for now - need to reintroduce into death sequence somewhere else
            //other.transform.parent.parent.GetComponent<Player>().ToggleCollision(false);

            // TODO: currently AI fuel levels are not impacted when they collide with a trail
            // TODO: use PerformBlockImpactEffects
            OnTrailCollision?.Invoke(ownerId, fuelChange);
            
            ship.PerformTrailBlockImpactEffects(trailBlockProperties);
        }
    }

    void Explode(Vector3 impactVector, string impactId)
    {
        // We don't destroy the trail blocks, we keep the objects around so they can be restored
        gameObject.GetComponent<BoxCollider>().enabled = false;
        gameObject.GetComponent<MeshRenderer>().enabled = false;

        // Make exploding block
        var explodingBlock = Instantiate(FossilBlock);
        explodingBlock.transform.position = transform.position;
        explodingBlock.transform.localEulerAngles = transform.localEulerAngles;
        explodingBlock.transform.localScale = transform.localScale;
        explodingBlock.GetComponent<Renderer>().material = new Material(material);
        explodingBlock.GetComponent<BlockImpact>().HandleImpact(impactVector, impactId);

        destroyed = true;

        // Remove block from team score when destroyed
        ScoringManager.Instance.UpdateTeamScore(team, trailBlockProperties.volume*-1);
    }

    public void restore()
    {
        gameObject.GetComponent<BoxCollider>().enabled = true;
        gameObject.GetComponent<MeshRenderer>().enabled = true;

        destroyed = false;

        // Add block back to team score when created
        ScoringManager.Instance.UpdateTeamScore(team, trailBlockProperties.volume);
    }

    // TODO: utility class needed to hold these
    private bool IsShip(GameObject go)
    {
        return go.layer == LayerMask.NameToLayer("Ships");
    }
    private bool IsSkimmer(GameObject go)
    {
        return go.layer == LayerMask.NameToLayer("Skimmers");
    }
    private bool IsExplosion(GameObject go)
    {
        return go.layer == LayerMask.NameToLayer("Explosions");
    }
    private bool IsProjectile(GameObject go)
    {
        return go.layer == LayerMask.NameToLayer("Projectiles");
    }
}