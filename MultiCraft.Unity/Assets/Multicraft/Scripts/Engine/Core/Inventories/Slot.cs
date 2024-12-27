using System;
using MultiCraft.Scripts.Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MultiCraft.Scripts.Engine.Core.Inventories
{
     public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private Image image;
        private Image itemImage;
        private TextMeshProUGUI itemAmount;

        private Color defaultColor = new Color32(114, 114, 114, 255);
        private Color highlightColor = new Color32(255, 255, 255, 255);
        private Color HotBarSelectedColor = new Color32(255, 255, 255, 255);

        public ItemInSlot Item;

        public bool hasItem => Item != null;

        private void Awake()
        {
            image = GetComponent<Image>();
            itemImage = transform.GetChild(0).GetComponent<Image>();
            itemAmount = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            itemImage.preserveAspect = true;
        }

        private void OnEnable()
        {
            SetDefaultColor();
        }

        public void SetItem(ItemInSlot item)
        {
            Item = item;
            RefreshUI();
        }

        public void AddItem(ItemInSlot item, int amount)
        {
            item.Amount -= amount;
            if (!hasItem)
            {
                SetItem(new ItemInSlot(item.Item, amount));
            }
            else
            {
                Item.Amount += amount;
                RefreshUI();
            }
        }

        public void ResetItem()
        {
            Item = null;
            RefreshUI();
        }

        public void RefreshUI()
        {
            itemImage.gameObject.SetActive(hasItem);
            itemImage.sprite = Item?.Item.Icon;

            itemAmount.gameObject.SetActive(hasItem && Item.Amount > 1);
            itemAmount.text = Item?.Amount.ToString();
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                LeftClick();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                RightClick();
            }
        }

        public virtual void LeftClick()
        {
            var currentItem = InventoryWindow.Instance.CurrentItem;

            if (hasItem)
            {
                if (currentItem == null || Item.Item != currentItem.Item)
                {
                    InventoryWindow.Instance.SetCurrentItem(Item);
                    ResetItem();
                }
                else
                {
                    if (Item.Amount + currentItem.Amount < Item.Item.MaxStackSize)
                    {
                        AddItem(currentItem, currentItem.Amount);
                        InventoryWindow.Instance.CheckCurrentItem();
                    }

                    return;
                }
            }
            else
            {
                InventoryWindow.Instance.ResetCurrentItem();
            }

            if (currentItem != null)
            {
                SetItem(currentItem);
            }
        }

        public virtual void RightClick()
        {
            if (!InventoryWindow.Instance.HasCurrentItem)
                return;

            if (!hasItem)
            {
                AddItem(InventoryWindow.Instance.CurrentItem, 1);
                InventoryWindow.Instance.CheckCurrentItem();
                return;
            }

            if (InventoryWindow.Instance.CurrentItem.Item == Item.Item)
            {
                if (Item.Amount + 1 < Item.Item.MaxStackSize)
                    AddItem(InventoryWindow.Instance.CurrentItem, 1);
                InventoryWindow.Instance.CheckCurrentItem();
                return;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            image.color = highlightColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            image.color = defaultColor;
        }

        public void SetHighLight()
        {
            image.color = HotBarSelectedColor;
        }
        public void SetDefaultColor()
        {
            image.color = defaultColor;
        }
    }
}