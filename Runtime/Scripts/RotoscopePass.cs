using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OccaSoftware.Rotoscope.Runtime
{
    class RotoscopePass : ScriptableRenderPass
    {
        private RTHandle source;

        const string dShaderId = "Shader Graphs/DiscontinuityShader";
        const string rotoscopeShaderId = "Shader Graphs/RotoscopeShader";
        private Material dMaterial;
        private Material rotoscopeMaterial;

        RTHandle discontinuitiesRT;
        private const string dRtId = "_Discontinuities";
        RTHandle rotoscopeRT;
        private const string rotoscopeId = "_Rotoscope";

        public RotoscopePass()
        {
            Shader dShader = Shader.Find(dShaderId);
            Shader rShader = Shader.Find(rotoscopeShaderId);

            if (rotoscopeMaterial == null && rShader != null)
            {
                rotoscopeMaterial = CoreUtils.CreateEngineMaterial(rotoscopeShaderId);
            }

            if (dMaterial == null && dShader != null)
            {
                dMaterial = CoreUtils.CreateEngineMaterial(dShaderId);
            }
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (discontinuitiesRT == null)
            {
                discontinuitiesRT = RTHandles.Alloc(Shader.PropertyToID(dRtId), name: dRtId);
            }
            if (rotoscopeRT == null)
            {
                rotoscopeRT = RTHandles.Alloc(Shader.PropertyToID(rotoscopeId), name: rotoscopeId);
            }

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = 0;
            descriptor.colorFormat = RenderTextureFormat.DefaultHDR;
            descriptor.width = Mathf.Max(1, descriptor.width);
            descriptor.height = Mathf.Max(1, descriptor.height);
            RenderingUtils.ReAllocateIfNeeded(
                ref discontinuitiesRT,
                descriptor,
                FilterMode.Point,
                TextureWrapMode.Clamp,
                name: dRtId
            );
            RenderingUtils.ReAllocateIfNeeded(
                ref rotoscopeRT,
                descriptor,
                FilterMode.Point,
                TextureWrapMode.Clamp,
                name: rotoscopeId
            );
        }

        private RotoscopeData data;

        public void SetData(RotoscopeData data)
        {
            this.data = data;
        }

        public override void Execute(
            ScriptableRenderContext context,
            ref RenderingData renderingData
        )
        {
            if (renderingData.cameraData.isPreviewCamera)
                return;

            if (data == null)
                return;

            if (rotoscopeMaterial == null || dMaterial == null)
                return;

            if (discontinuitiesRT == null || rotoscopeRT == null)
                return;

            if (data.Gradient == null)
            {
                data.ValidateAndCreateGradient();
            }

            dMaterial.SetInt(Properties._NumSteps, data.Colors.Count);
            dMaterial.SetFloat(Properties._StartDist, data.StartDistance);
            dMaterial.SetFloat(Properties._EndDist, data.EndDistance);
            rotoscopeMaterial.SetFloat(Properties._NumSteps, data.Colors.Count);
            rotoscopeMaterial.SetTexture(Properties._Gradient, data.Gradient);

            CommandBuffer cmd = CommandBufferPool.Get("Rotoscope");

            source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            if (source == null)
                return;

            Blitter.BlitCameraTexture(cmd, source, discontinuitiesRT, dMaterial, 0);
            cmd.SetGlobalTexture(Properties._DiscontinuityMap, discontinuitiesRT);
            Blitter.BlitCameraTexture(cmd, source, rotoscopeRT, rotoscopeMaterial, 0);
            Blitter.BlitCameraTexture(cmd, rotoscopeRT, source, 0);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        private static class Properties
        {
            public static readonly int _NumSteps = Shader.PropertyToID("_NumSteps");
            public static readonly int _StartDist = Shader.PropertyToID("_StartDist");
            public static readonly int _EndDist = Shader.PropertyToID("_EndDist");
            public static readonly int _Gradient = Shader.PropertyToID("_Gradient");

            public static readonly int _DiscontinuityMap = Shader.PropertyToID("_DiscontinuityMap");
        }

        public void Dispose()
        {
            discontinuitiesRT?.Release();
            rotoscopeRT?.Release();

            discontinuitiesRT = null;
            rotoscopeRT = null;

            rotoscopeMaterial = null;
            dMaterial = null;
        }
    }
}
