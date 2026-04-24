
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelizeFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class CustomPassSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public int screenHeight = 144;
    }

    [SerializeField] private CustomPassSettings settings;
    [SerializeField, HideInInspector] private Shader shader;
    
    private PixelizePass customPass;

    public override void Create()
    {
        if (settings == null) return;

#if UNITY_EDITOR
        if (shader == null)
        {
            shader = Shader.Find("Hidden/Pixelize");
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        if (shader == null)
        {
            Debug.LogWarning("Pixelize shader not found.");
            return;
        }

        customPass = new PixelizePass(settings, shader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_EDITOR
        if (renderingData.cameraData.isSceneViewCamera) return;
#endif
        if (customPass == null) return;
        renderer.EnqueuePass(customPass);
    }

    protected override void Dispose(bool disposing)
    {
        if (customPass != null)
        {
            customPass.Dispose();
        }
    }
}
