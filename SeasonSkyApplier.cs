using UnityEngine;

public class SeasonSkyApplier : MonoBehaviour
{
    [Header("Skybox")]
    public Material skyboxMaterial;   // 이 시즌에 쓸 스카이박스 머티리얼
    public bool updateGI = true;      // 환경광/리플렉션 갱신

    [Header("Fog (옵션)")]
    public bool applyFog = false;
    public bool fogEnabled = true;
    public Color fogColor = Color.gray;
    public FogMode fogMode = FogMode.Exponential;
    public float fogDensity = 0.01f;
    public float fogStartDistance = 0f;
    public float fogEndDistance = 300f;

    void OnEnable()
    {
        if (skyboxMaterial) RenderSettings.skybox = skyboxMaterial;

        if (applyFog)
        {
            RenderSettings.fog = fogEnabled;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
        }

        if (updateGI) DynamicGI.UpdateEnvironment(); // 앰비언트/리플렉션 즉시 갱신
    }
}
