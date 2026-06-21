using NDTB.Data;

namespace NDTB.Core
{
    public interface IPicked
    {
        public void Pick();
        public ItemData Item { get; set; }
    }
}
