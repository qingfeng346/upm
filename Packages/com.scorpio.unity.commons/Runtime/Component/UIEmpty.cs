namespace UnityEngine.UI
{
    [AddComponentMenu("UGUI/UIEmpty")]
    [DisallowMultipleComponent]
    public class UIEmpty : MaskableGraphic
    {
        protected UIEmpty()
        {
            useLegacyMeshGeneration = false;
        }

        protected sealed override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}
