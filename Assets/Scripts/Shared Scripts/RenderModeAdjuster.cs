using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RenderModeAdjuster {
    public static void SetTransparent(Material mat) {
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    public static void SetOpaque(Material mat) {
        mat.SetOverrideTag("RenderType", "");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = -1;
    }
}