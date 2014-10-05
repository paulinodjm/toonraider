using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace InventoryCollections
{
    [System.Serializable]
    public class ItemCollection : IInventoryCollection<Item>
    {
        private List<Item> _items = new List<Item>();

        public void Add(Item item)
        {
            if (item == null)
            {
                Debug.LogError("Trying to add a null item!");
                return;
            }

            _items.Add(item);
            Debug.Log("Item '" + item.Name + "' added");
        }

        public void Remove(Item item)
        {
            if (item == null)
            {
                Debug.LogError("Trying to remove a null item!");
                return;
            }

            _items.Remove(item);
            Debug.Log("Item '" + item.Name + "' removed");
        }

        public int Count()
        {
            return _items.Count;
        }

        public Item Find(string eltName)
        {
            var itemQuery = from item in _items where item.Name == eltName select item;
            foreach (var item in itemQuery)
            {
                return item;
            }
            return null;
        }

        public int Count(string eltName)
        {
            var itemQuery = from item in _items where item.Name == eltName select item;
            return itemQuery.Count();
        }
    }
}