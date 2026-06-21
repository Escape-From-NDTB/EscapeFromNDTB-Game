using NDTB.Data;

namespace NDTB.Systems.Player.Inventory
{
    public class HandSlot
    {
        public ItemData CurrentItem { get; private set; }

        public HandSlot()
        {
            CurrentItem = null;
        }

        public void Equip(ItemData item)
        {
            CurrentItem = item;
        }

        public ItemData Unequip()
        {
            ItemData itemToReturn = CurrentItem;
            CurrentItem = null;
            return itemToReturn;
        }

        public bool IsEmpty()
        {
            return CurrentItem == null;
        }
    }
}
