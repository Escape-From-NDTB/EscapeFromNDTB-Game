using NDTB.Data;

namespace NDTB.Systems.Player.Inventory
{
    public class HandSlot
    {
        public SO_ItemData CurrentItem { get; private set; }

        public HandSlot()
        {
            CurrentItem = null;
        }

        public void Equip(SO_ItemData item)
        {
            CurrentItem = item;
        }

        public SO_ItemData Unequip()
        {
            SO_ItemData itemToReturn = CurrentItem;
            CurrentItem = null;
            return itemToReturn;
        }

        public bool IsEmpty()
        {
            return CurrentItem == null;
        }
    }
}
