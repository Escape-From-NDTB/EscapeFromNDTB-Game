using NDTB.Data;

namespace NDTB.Core
{
    public interface IMemoryable
    {
        public void Save();
        public Memory Memory { get; set; }
    }
}
