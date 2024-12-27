using System;
using MultiCraft.Scripts.Engine.Core.Inventories;
using MultiCraft.Scripts.Engine.Core.Items;
using MultiCraft.Scripts.Engine.Core.MeshBuilders;
using MultiCraft.Scripts.Engine.Utils;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Entities
{
    public class DroppedItem : MonoBehaviour
    {
        public float DelayTime = 0.1f;
        public ItemInSlot Item;
        public DropItemRenderer DropItemRenderer;

        private float _spawnTime;

        private void Awake()
        {
            _spawnTime = Time.time;
        }

        public void Init()
        {
            if (Item.Item.Type == ItemType.Block)
            {
                var block = ResourceLoader.Instance.GetBlock(Item.Item.BlockType.Id);
                var blockMesh = DropItemMeshBuilder.GeneratedMesh(block);
                transform.GetChild(0).GetComponent<DropItemRenderer>().SetMesh(blockMesh);
            }
            else
            {
                var item = Item.Item;
                var itemMesh =  DropItemMeshBuilder.GeneratedMesh(item);
                transform.GetChild(0).GetComponent<DropItemRenderer>().SetMesh(itemMesh);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Time.time - _spawnTime <= DelayTime) return;
            if (other.CompareTag("Player"))
            {
                if (other.GetComponent<Inventory>().AddItem(Item))
                {
                    Destroy(gameObject);
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (Time.time - _spawnTime <= DelayTime) return;
            if (other.CompareTag("Player"))
            {
                if (other.GetComponent<Inventory>().AddItem(Item))
                {
                    Destroy(gameObject);
                }
            }
            if (other.CompareTag("DroppedItem"))
            {
                var otherDroppedItem = other.GetComponent<DroppedItem>();
                if (Item != null && otherDroppedItem.Item != null)
                    if (otherDroppedItem.Item.Item.Name == Item.Item.Name)
                    {
                        if (otherDroppedItem._spawnTime < _spawnTime && Item.Amount > 0)
                        {
                            Item.Amount--;
                            otherDroppedItem.Item.Amount++;
                            Destroy(gameObject);
                        }
                    }
            }
        }
    }
}