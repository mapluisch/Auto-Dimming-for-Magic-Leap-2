using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR;
using MagicLeap.OpenXR.Features;

public class AutoDimming : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshPro label; // info / debug label
    [SerializeField] private float minBrightness = 100.0f; // minimum lux thresh after which the dimming sets in 
    [SerializeField] private float maxBrightness = 2500.0f; // maximum lux thresh after which the dimming is maxed out
    [SerializeField] private AnimationCurve brightnessCurve; // defines mapping from ambient light levels to screen dimming values 
    private MagicLeapRenderingExtensionsFeature renderFeature;

    private void Start()
    {
        renderFeature = OpenXRSettings.Instance.GetFeature<MagicLeapRenderingExtensionsFeature>();

        if (LightSensor.current != null)
        {
            InputSystem.EnableDevice(LightSensor.current);
            LightSensor.current.samplingFrequency = 60;
            Debug.Log("light sensor enabled.");
        }
        else Debug.Log("light sensor not available.");
    }

    private void Update()
    {
        if (LightSensor.current != null && LightSensor.current.enabled)
        {
            float ambientLightLux = LightSensor.current.lightLevel.ReadValue();
            float t = Mathf.InverseLerp(minBrightness, maxBrightness, ambientLightLux);
            float dimmingValue = brightnessCurve.Evaluate(t);

            label?.SetText($"{ambientLightLux:F2} lx | {dimmingValue:F2}");

            if (renderFeature != null)
            {
                renderFeature.GlobalDimmerEnabled = true;
                renderFeature.GlobalDimmerValue = Mathf.Clamp01(dimmingValue);
            }
        }
    }

    private void OnDestroy()
    {
        if (LightSensor.current != null)
        {
            InputSystem.DisableDevice(LightSensor.current);
            Debug.Log("Light sensor disabled.");
        }
    }
}
