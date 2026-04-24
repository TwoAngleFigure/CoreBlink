
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class PixelizePass : ScriptableRenderPass
{
    private class PassData
    {
        public TextureHandle sourceTexture;
        public Material material;
    }

    private PixelizeFeature.CustomPassSettings settings;
    private Material material;

    public PixelizePass(PixelizeFeature.CustomPassSettings settings, Shader shader)
    {
        this.settings = settings;
        this.renderPassEvent = settings.renderPassEvent;
        if (material == null && shader != null) material = CoreUtils.CreateEngineMaterial(shader);
    }

    public void Dispose()
    {
        if (material != null)
        {
            CoreUtils.Destroy(material);
        }
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        int pixelScreenHeight = settings.screenHeight;
        int pixelScreenWidth = (int)(pixelScreenHeight * cameraData.camera.aspect + 0.5f);

        material.SetVector("_BlockCount", new Vector2(pixelScreenWidth, pixelScreenHeight));
        material.SetVector("_BlockSize", new Vector2(1.0f / pixelScreenWidth, 1.0f / pixelScreenHeight));
        material.SetVector("_HalfBlockSize", new Vector2(0.5f / pixelScreenWidth, 0.5f / pixelScreenHeight));

        TextureHandle source = resourceData.activeColorTexture;

        RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
        descriptor.height = pixelScreenHeight;
        descriptor.width = pixelScreenWidth;
        descriptor.depthBufferBits = 0;

        TextureHandle pixelBuffer = UniversalRenderer.CreateRenderGraphTexture(
            renderGraph, descriptor, "_PixelBuffer", false, FilterMode.Point);

        // Pass 1: source -> pixelBuffer (with pixelize material)
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelize Downsample", out var passData))
        {
            passData.sourceTexture = source;
            passData.material = material;

            builder.UseTexture(source);
            builder.SetRenderAttachment(pixelBuffer, 0);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                Blitter.BlitTexture(context.cmd, data.sourceTexture, new Vector4(1, 1, 0, 0), data.material, 0);
            });
        }

        // Pass 2: pixelBuffer -> source (copy back)
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelize Upsample", out var passData2))
        {
            passData2.sourceTexture = pixelBuffer;
            passData2.material = null;

            builder.UseTexture(pixelBuffer);
            builder.SetRenderAttachment(source, 0);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                Blitter.BlitTexture(context.cmd, data.sourceTexture, new Vector4(1, 1, 0, 0),
                    Blitter.GetBlitMaterial(TextureDimension.Tex2D), 0);
            });
        }
    }
}
