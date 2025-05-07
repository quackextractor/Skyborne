using UnityEngine;

namespace TrueClouds.Scripts
{
    class CloudCamera3D: CloudCamera
    {
        [ImageEffectOpaque]
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            RenderClouds(source, destination);
        }
    }
}
