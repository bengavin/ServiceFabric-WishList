using System;

namespace WishList.Core.Models
{
    public class WishListItem
    {
        /// <summary>
        /// The type of this item (broad category)
        /// </summary>
        public ItemType ItemType { get; set; }

        /// <summary>
        /// The common name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A link to a website that contains information about this item (even Santa's elves don't know everything)
        /// </summary>
        public Uri ReferenceUri { get; set; }

        /// <summary>
        /// Your guess at the approximate retail value of this item (in US$)
        /// </summary>
        public decimal? ApproximateRetailValue { get; set; }

        /// <summary>
        /// Is this gift item approved by the review committee?
        /// </summary>
        public bool IsApproved { get; set; }
    }
}
