using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria.GameInput;

namespace Tyfyter.Utils {
    public static class UITools {
        public static void DrawColoredItemSlot(SpriteBatch spriteBatch, ref Item item, Rectangle destination, Texture2D backTexture, Color slotColor, Color lightColor = default) {
            spriteBatch.Draw(backTexture, destination, null, slotColor, 0f, default(Vector2), SpriteEffects.None, 0f);
            ItemSlot.Draw(spriteBatch, ref item, ItemSlot.Context.ChatItem, destination.TopLeft(), lightColor);
        }
        public static void DrawColoredItemSlot(SpriteBatch spriteBatch, ref Item item, Vector2 position, Texture2D backTexture, Color slotColor, Color lightColor = default) {
            spriteBatch.Draw(backTexture, position, null, slotColor, 0f, default(Vector2), Main.inventoryScale, SpriteEffects.None, 0f);
            ItemSlot.Draw(spriteBatch, ref item, ItemSlot.Context.ChatItem, position, lightColor);
        }
        public class SingleItemSlotUI : UIState {
            public virtual float SlotX => 0;
            public virtual float SlotY => 0;
            public RefItemSlot itemSlot = null;
            protected internal Queue<Action> itemSlotQueue = new Queue<Action>() { };
            /// <summary>
            /// Passes the parameters to an action to be added in Update
            /// </summary>
            public void SafeSetItemSlot(Ref<Item> item, Vector2 position, bool usePercent = false, Func<Item, bool> ValidItemFunc = null, Color? slotColor = null, int context = ItemSlot.Context.InventoryItem, float slotScale = 1f, bool shiftClickToInventory = false, params (Texture2D texture, Color color)[] extraTextures) {
                if (item.Value is null) {
                    item.Value = new Item();
                    item.Value.SetDefaults(0);
                }
                itemSlotQueue.Enqueue(() => SetItemSlot(item, position, usePercent, ValidItemFunc, slotColor, context, slotScale, shiftClickToInventory, extraTextures));
            }
            /// <summary>
            /// Adds a reference-based item slot to the ui state
            /// </summary>
            /// <param name="item"> the item that should be referenced by the new slot</param>
            /// <param name="_position">the position of the slot, leave as null to automatically place the slot</param>
            /// <param name="usePercent">ignored if position is null</param>
            /// <param name="_ValidItemFunc">passed to RefItemSlot constructor</param>
            /// <param name="colorContext">passed to RefItemSlot constructor</param>
            /// <param name="context">passed to RefItemSlot constructor</param>
            /// <param name="slotScale">passed to RefItemSlot constructor</param>
            public void SetItemSlot(Ref<Item> item, Vector2 position, bool usePercent = false, Func<Item, bool> _ValidItemFunc = null, Color? slotColor = null, int context = ItemSlot.Context.InventoryItem, float slotScale = 1f, bool shiftClickToInventory = false, params (Texture2D texture, Color color)[] extraTextures) {
                RefItemSlot itemSlot = new RefItemSlot(_item: item, context: context, scale: slotScale) {
                    ValidItemFunc = _ValidItemFunc ?? (i => true),
                    colorMult = slotColor ?? Color.White,
                    extraTextures = extraTextures,
                    shiftClickToInventory = shiftClickToInventory
                };
                if (usePercent) {
                    itemSlot.Left = new StyleDimension { Percent = position.X };
                    itemSlot.Top = new StyleDimension { Percent = position.Y };
                } else {
                    itemSlot.Left = new StyleDimension { Pixels = position.X };
                    itemSlot.Top = new StyleDimension { Pixels = position.Y };
                }
                RemoveChild(itemSlot);
                this.itemSlot = itemSlot;
                Append(itemSlot);
            }
            public override void Update(GameTime gameTime) {
                if (itemSlotQueue.Count > 0) {
                    itemSlotQueue.Dequeue()();
                }
                itemSlot.Left = new StyleDimension { Pixels = SlotX };
                itemSlot.Top = new StyleDimension { Pixels = SlotY };
                base.Update(gameTime);
            }
        }
        public class RefItemSlot : UIElement {
            public static Color MissingSlotColor => new Color(160, 160, 160, 160);
            public bool slotSourceMissing = false;
            public bool shiftClickToInventory = false;
            internal Ref<Item> item;
            internal readonly int _context;
            private readonly float _scale;
            internal Func<Item, bool> ValidItemFunc;
            protected internal int index = -1;
            public Color colorMult = default;
            public (Texture2D texture, Color color)[] extraTextures = new (Texture2D, Color)[] { };
            public RefItemSlot(int context = ItemSlot.Context.InventoryItem, float scale = 1f, Ref<Item> _item = null) {
                _context = context;
                _scale = scale;
                item = _item;
                Width.Set(Main.inventoryBack9Texture.Width * scale, 0f);
                Height.Set(Main.inventoryBack9Texture.Height * scale, 0f);
            }
            protected override void DrawSelf(SpriteBatch spriteBatch) {
                float oldScale = Main.inventoryScale;
                Main.inventoryScale = 0.85f * _scale;
                Rectangle rectangle = GetDimensions().ToRectangle();
                rectangle.Width = (int)(rectangle.Width * Main.inventoryScale);
                rectangle.Height = (int)(rectangle.Width * Main.inventoryScale);

                if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
                    Main.LocalPlayer.mouseInterface = true;
                    int context = _context;
                    if (shiftClickToInventory && ItemSlot.ShiftInUse) {
                        context = ItemSlot.Context.EquipAccessory;
                    }
                    if (slotSourceMissing) {
                        if (Main.mouseItem?.IsAir ?? true) {
                            ItemSlot.Handle(ref item.Value, context);
                        }
                    } else {
                        if (ValidItemFunc == null || ValidItemFunc(Main.mouseItem)) {
                            ItemSlot.Handle(ref item.Value, context);
                        }
                    }
                }
                // Draw draws the slot itself and Item. Depending on context, the color will change, as will drawing other things like stack counts.
                Color slotColor = colorMult;
                Color slotColorMult = Color.White;
                if (slotSourceMissing) {
                    slotColorMult = MissingSlotColor;
                }
                Item _ = new Item();
                DrawColoredItemSlot(spriteBatch, ref _, rectangle.TopLeft(), Main.inventoryBack13Texture, slotColor.MultiplyRGBA(slotColorMult));
                foreach ((Texture2D texture, Color color) texture in extraTextures) {
                    spriteBatch.Draw(texture.texture, rectangle.TopLeft(), null, texture.color.MultiplyRGBA(slotColorMult), 0, default, Main.inventoryScale, SpriteEffects.None, 0);
                }
                ItemSlot.Draw(spriteBatch, ref item.Value, ItemSlot.Context.ChatItem, rectangle.TopLeft(), slotColorMult);
                Main.inventoryScale = oldScale;
            }
        }
    }
}