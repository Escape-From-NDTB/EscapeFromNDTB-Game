using NDTB.Data;

namespace NDTB.Systems.Player.Inventory
{
    public class InventoryItem
    {
        public ItemData ItemData;
        public int X;
        public int Y;
        public bool IsRotated;

        public InventoryItem(ItemData data, int x, int y)
        {
            this.ItemData = data;
            this.X = x;
            this.Y = y;
            this.IsRotated = false;
        }

        public void Rotate()
        {
            IsRotated = !IsRotated;
        }

        public int GetWidth()
        {
            return IsRotated ? ItemData.Height : ItemData.Width;
        }

        public int GetHeight()
        {
            return IsRotated ? ItemData.Width : ItemData.Height;
        }
    }
}
