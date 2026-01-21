using BlockBlast.Effects;

namespace BlockBlast.Utils
{
    /// <summary>
    /// Object Pool cho CellDestroyEffect
    /// Thêm component này vào một GameObject trong scene
    /// </summary>
    public class ObjectPoolingCellDestroyEffect : ObjectPoolingX<CellDestroyEffect>
    {
        public override CellDestroyEffect GetObjectType(CellDestroyEffect key)
        {
            CellDestroyEffect effect = base.GetObjectType(key);
            effect.transform.SetParent(transform);
            return effect;
        }
    }
}
