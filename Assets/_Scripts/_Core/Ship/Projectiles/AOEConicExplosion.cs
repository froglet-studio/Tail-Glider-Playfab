using System.Collections;
using StarWriter.Core;
using UnityEngine;

public class AOEConicExplosion : AOEExplosion
{
    [SerializeField] float height = 800; //maybe pull from node diameter

    protected override void Start()
    {
        base.Start();

        MaxScaleVector = new Vector3(MaxScale, MaxScale, height);
    }

    protected override IEnumerator ExplodeCoroutine()
    {
        yield return new WaitForSeconds(ExplosionDelay);

        if (TryGetComponent<MeshRenderer>(out var meshRenderer))
            meshRenderer.material = Material;

        var elapsedTime = 0f;
        while (elapsedTime < ExplosionDuration)
        {
            elapsedTime += Time.deltaTime;
            var lerpAmount = Mathf.Sin((elapsedTime / ExplosionDuration) * PI_OVER_TWO);
            container.transform.localScale = Vector3.Lerp(Vector3.zero, MaxScaleVector, lerpAmount);
            GetComponent<SphereCollider>().radius = container.transform.localScale.x / (container.transform.localScale.z * 2);
            Material.SetFloat("_Opacity", Mathf.Clamp((MaxScaleVector - container.transform.localScale).magnitude / MaxScaleVector.magnitude, 0, 1));
            yield return null;
        }

        Destroy(gameObject);
    }
}