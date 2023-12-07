using UnityEngine;

public static class LayerMaskUnpack
{
    public static bool IsLayerInMask(int layer, LayerMask layerMask)
    {
        return (1 << layer & layerMask.value) != 0;
    }
}
