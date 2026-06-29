using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class UnifiedOutlineFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public LayerMask outlineLayer;
        public Color outlineColor = Color.cyan;

        [Range(1, 12)]
        public int thickness = 3;

        public Material maskMaterial;
        public Material outlineMaterial;

        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public Settings settings = new Settings();

    private UnifiedOutlinePass _pass;

    public override void Create()
    {
        _pass = new UnifiedOutlinePass(settings)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.maskMaterial == null || settings.outlineMaterial == null)
            return;

        renderer.EnqueuePass(_pass);
    }

    private class UnifiedOutlinePass : ScriptableRenderPass
    {
        private readonly Settings _settings;

        private static readonly int SourceTexId = Shader.PropertyToID("_SourceTex");
        private static readonly int MaskTexId = Shader.PropertyToID("_MaskTex");
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int ThicknessId = Shader.PropertyToID("_Thickness");
        private static readonly int TexelSizeId = Shader.PropertyToID("_MaskTex_TexelSize_Custom");

        private class PassData
        {
            public TextureHandle sourceColor;
            public TextureHandle destinationColor;
            public TextureHandle maskTexture;

            public Material outlineMaterial;
            public Color outlineColor;
            public int thickness;
        }

        private class MaskPassData
        {
            public RendererListHandle rendererList;
        }

        public UnifiedOutlinePass(Settings settings)
        {
            _settings = settings;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            if (resourceData.isActiveTargetBackBuffer)
                return;

            TextureHandle cameraColor = resourceData.activeColorTexture;

            RenderTextureDescriptor cameraDescriptor = cameraData.cameraTargetDescriptor;
            cameraDescriptor.depthBufferBits = 0;
            cameraDescriptor.msaaSamples = 1;
            cameraDescriptor.colorFormat = RenderTextureFormat.R8;

            TextureHandle maskTexture = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                cameraDescriptor,
                "_UnifiedOutlineMask",
                false
            );

            using (IRasterRenderGraphBuilder builder =
                   renderGraph.AddRasterRenderPass<MaskPassData>(
                       "Unified Outline Mask",
                       out MaskPassData passData))
            {
                List<ShaderTagId> shaderTagIds = new()
                {
                    new ShaderTagId("UniversalForward"),
                    new ShaderTagId("UniversalForwardOnly"),
                    new ShaderTagId("SRPDefaultUnlit")
                };

                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(
                    shaderTagIds,
                    renderingData,
                    cameraData,
                    lightData,
                    SortingCriteria.CommonOpaque
                );

                drawingSettings.overrideMaterial = _settings.maskMaterial;
                drawingSettings.overrideMaterialPassIndex = 0;

                FilteringSettings filteringSettings = new FilteringSettings(
                    RenderQueueRange.all,
                    _settings.outlineLayer
                );

                RendererListParams rendererListParams = new RendererListParams(
                    renderingData.cullResults,
                    drawingSettings,
                    filteringSettings
                );

                passData.rendererList = renderGraph.CreateRendererList(rendererListParams);

                builder.UseRendererList(passData.rendererList);
                builder.SetRenderAttachment(maskTexture, 0);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc(
                    static (MaskPassData data, RasterGraphContext context) =>
                    {
                        context.cmd.ClearRenderTarget(false, true, Color.black);
                        context.cmd.DrawRendererList(data.rendererList);
                    });
            }

            RenderTextureDescriptor colorDescriptor = cameraData.cameraTargetDescriptor;
            colorDescriptor.depthBufferBits = 0;

            TextureHandle destinationColor = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                colorDescriptor,
                "_UnifiedOutlineColor",
                false
            );

            using (IRasterRenderGraphBuilder builder =
                   renderGraph.AddRasterRenderPass<PassData>(
                       "Unified Outline Composite",
                       out PassData passData))
            {
                passData.sourceColor = cameraColor;
                passData.destinationColor = destinationColor;
                passData.maskTexture = maskTexture;

                passData.outlineMaterial = _settings.outlineMaterial;
                passData.outlineColor = _settings.outlineColor;
                passData.thickness = _settings.thickness;

                builder.UseTexture(cameraColor, AccessFlags.Read);
                builder.UseTexture(maskTexture, AccessFlags.Read);
                builder.SetRenderAttachment(destinationColor, 0);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc(
                    static (PassData data, RasterGraphContext context) =>
                    {
                        Material mat = data.outlineMaterial;

                        mat.SetTexture(SourceTexId, data.sourceColor);
                        mat.SetTexture(MaskTexId, data.maskTexture);
                        mat.SetColor(OutlineColorId, data.outlineColor);
                        mat.SetInt(ThicknessId, data.thickness);

                        // In RenderGraph we don't directly read rt.width/height from TextureHandle.
                        // Use screen params inside shader instead, or pass camera descriptor separately.
                        Vector4 texelSize = new Vector4(
                            1f / Screen.width,
                            1f / Screen.height,
                            Screen.width,
                            Screen.height
                        );

                        mat.SetVector(TexelSizeId, texelSize);

                        Blitter.BlitTexture(
                            context.cmd,
                            data.sourceColor,
                            new Vector4(1, 1, 0, 0),
                            mat,
                            0
                        );
                    });
            }

            resourceData.cameraColor = destinationColor;
        }
    }
}