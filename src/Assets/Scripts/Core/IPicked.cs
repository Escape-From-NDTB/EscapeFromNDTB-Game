using NDTB.Data;

namespace NDTB.Core
{
    public interface IPicked
    {
        public void Pick();
        public SO_ItemData Item { get; set; }
    }
}
