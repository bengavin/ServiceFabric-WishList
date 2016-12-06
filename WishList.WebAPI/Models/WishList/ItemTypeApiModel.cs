namespace WishList.WebAPI.Models.WishList
{
    public enum ItemTypeApiModel
    {
        /// <summary>
        /// For games played at the kitchen table
        /// </summary>
        BoardGame,

        /// <summary>
        /// For items, games, etc that are primarily electronic in nature (including video games)
        /// </summary>
        Electronics,

        /// <summary>
        /// Things you wear
        /// </summary>
        Clothing,

        /// <summary>
        /// General catch-all for gift cards of any kind, the item 'Name' should reflect the store name
        /// </summary>
        GiftCard,

        /// <summary>
        /// Stuffed animals and such
        /// </summary>
        StuffedToy,

        /// <summary>
        /// Toys that don't fit in any other category
        /// </summary>
        GeneralToy,

        /// <summary>
        /// The age old gift
        /// </summary>
        Cash,

        /// <summary>
        /// For things that just don't fit anywhere else, Elves can make anything, right?
        /// </summary>
        Other
    }
}
