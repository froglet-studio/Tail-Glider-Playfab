
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class VolumeDisplay : MonoBehaviour
{
    public float Color1Radius = 0.5f;
    public float Color2Radius = 0.5f;
    public float Color3Radius = 0.5f;
    [SerializeField] float upperBound = 300000;

    private Material material;
    

    private void Awake()
    {
        var imageComponent = GetComponent<Image>();
        material = new Material(imageComponent.material);
        imageComponent.material = material;
    }

    private void Update()
    {
        if (StatsManager.Instance != null)
        {
            var teamStats = StatsManager.Instance.teamStats;
            var greenVolume = teamStats.ContainsKey(Teams.Green) ? teamStats[Teams.Green].volumeRemaining : 0f;
            var redVolume = teamStats.ContainsKey(Teams.Red) ? teamStats[Teams.Red].volumeRemaining : 0f;
            var goldVolume = teamStats.ContainsKey(Teams.Yellow) ? teamStats[Teams.Yellow].volumeRemaining : 0f;
            AdjustRadii(greenVolume / upperBound, redVolume / upperBound, goldVolume / upperBound);
        }
    }

    public void UpdateUI()
    {
        if (material)
        {
            material.SetFloat("_Radius1", Color1Radius);
            material.SetFloat("_Radius2", Color2Radius);
            material.SetFloat("_Radius3", Color3Radius);
        }
    }

    // Call this function to update the radii dynamically
    public void AdjustRadii(float r1, float r2, float r3)
    {
        Color1Radius = r1;
        Color2Radius = r2;
        Color3Radius = r3;
        UpdateUI();
    }
}
