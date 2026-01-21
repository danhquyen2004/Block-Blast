using BlockBlast.Core;

namespace BlockBlast.Utils
{
    /// <summary>
    /// Object Pool cho Block
    /// Thêm component này vào một GameObject trong scene
    /// </summary>
    public class ObjectPoolingBlock : ObjectPoolingX<Block>
    {
        public override Block GetObjectType(Block key)
        {
            Block block = base.GetObjectType(key);
            block.transform.SetParent(transform);
            return block;
        }
    }
}
