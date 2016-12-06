using System;
using System.Collections.Generic;

namespace WishList.Core.Models
{
    public class WishList
    {
        /// <summary>
        /// Who does this wish list belong to?
        /// </summary>
        public Guid PersonId { get; set; }

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
        public BehaviorRating SelfReportedBehaviorRating { get; set; }

        /// <summary>
        /// What did you actually do?
        /// </summary>
        public BehaviorRating ActualBehaviorRating { get; set; }

        /// <summary>
        /// What would you like Santa to bring you this year?
        /// </summary>
        public List<WishListItem> Items { get; set; }
    }
}
