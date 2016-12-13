using System;
using System.Collections.Generic;

namespace WishList.Core.Models
{
    [Serializable]
    public class WishListApproval
    {
        public WishListApproval()
        {
            ApprovedItems = new List<Guid>();
        }

        /// <summary>
        /// Who approved this wish list?
        /// </summary>
        public string Approver { get; set; }

        /// <summary>
        /// Which items have they approved
        /// </summary>
        public List<Guid> ApprovedItems { get; set; }
    }
}
