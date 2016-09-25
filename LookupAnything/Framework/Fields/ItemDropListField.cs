using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Pathoschild.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows a list of item drops.</summary>
    internal class ItemDropListField : GenericField
    {
        /*********
        ** Properties
        *********/
        /// <summary>The possible drops.</summary>
        private readonly Tuple<ItemDropData, Object>[] Drops;

        /// <summary>The text to display if there are no items.</summary>
        private readonly string DefaultText;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="drops">The possible drops.</param>
        /// <param name="defaultText">The text to display if there are no items (or <c>null</c> to hide the field).</param>
        public ItemDropListField(string label, IEnumerable<ItemDropData> drops, string defaultText = null)
            : base(label, null)
        {
            this.Drops =
                (
                    from drop in drops
                    let item = GameHelper.GetObjectBySpriteIndex(drop.ItemID)
                    orderby drop.Probability descending, item.Name ascending
                    select Tuple.Create(drop, item)
                )
                .ToArray();
            this.DefaultText = defaultText;
            this.HasValue = defaultText != null || this.Drops.Any();
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            if (!this.Drops.Any())
                return spriteBatch.DrawTextBlock(font, this.DefaultText, position, wrapWidth);

            // get icon size
            Vector2 iconSize = new Vector2(font.MeasureString("ABC").Y);

            // list drops
            bool canReroll = Game1.player.isWearingRing(Ring.burglarsRing);
            float height = 0;
            foreach (var entry in this.Drops)
            {
                // get data
                ItemDropData drop = entry.Item1;
                Object item = entry.Item2;
                bool isGuaranteed = drop.Probability > .99f;

                // draw icon
                spriteBatch.DrawIcon(item, position.X, position.Y + height, iconSize, isGuaranteed ? Color.White : Color.White * 0.5f);

                // draw text
                string text = isGuaranteed ? item.Name : $"{Math.Round(drop.Probability, 3) * 100}% chance of {item.Name}";
                if (drop.MaxDrop > 1)
                    text += $" (1 to {drop.MaxDrop})";
                Vector2 textSize = spriteBatch.DrawTextBlock(font, text, position + new Vector2(iconSize.X + 5, height + 5), wrapWidth, isGuaranteed ? Color.Black : Color.Gray);

                // cross out item if it definitely won't drop
                if (!isGuaranteed && !canReroll)
                    spriteBatch.DrawLine(position.X + iconSize.X + 5, position.Y + height + iconSize.Y / 2, new Vector2(textSize.X, 1), Color.Gray);

                height += textSize.Y + 5;
            }

            // return size
            return new Vector2(wrapWidth, height);
        }
    }
}