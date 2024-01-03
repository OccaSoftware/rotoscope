using OccaSoftware.Rotoscope.Runtime;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OccaSoftware.Rotoscope.Runtime
{
    [ExecuteAlways]
    public class Rotoscope : MonoBehaviour
    {
        private RotoscopePass rotoscopePass;
        public RotoscopeData rotoscopeData;

        private void OnEnable()
        {
            rotoscopePass = new RotoscopePass();
            rotoscopePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            rotoscopePass.Dispose();
        }

        private void OnBeginCamera(ScriptableRenderContext context, Camera cam)
        {
            cam.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(rotoscopePass);
            rotoscopePass.SetData(rotoscopeData);
        }
    }
}
