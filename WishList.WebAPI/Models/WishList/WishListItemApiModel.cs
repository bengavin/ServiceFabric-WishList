using System;

namespace WishList.WebAPI.Models.WishList
{
    public class WishListItemApiModel
    {
        /// <summary>
        /// The type of this item (broad category)
        /// </summary>
        public ItemTypeApiModel ItemType { get; set; }

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
    }
}
