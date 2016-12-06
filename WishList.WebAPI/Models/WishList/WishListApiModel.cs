using System.Collections.Generic;

namespace WishList.WebAPI.Models.WishList
{
    public class WishListApiModel
    {
        /// <summary>
        /// The name you were given [or took]
        /// </summary>
        public string GivenName { get; set; }

        /// <summary>
        /// Your family name
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Everyone has email these days, right?
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Nobody does email anymore, right?
        /// </summary>
        public string TwitterHandle { get; set; }

        /// <summary>
        /// How do you feel you did this year?
        /// </summary>
        public BehaviorRatingApiModel BehaviorRating { get; set; }

        /// <summary>
        /// What would you like Santa to bring you this year?
        /// </summary>
        public List<WishListItemApiModel> Items { get; set; }
    }
}
