using System;

namespace WishList.Core.Models
{
    [Serializable]
    public class Gift
    {
        public Gift()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string MadeBy { get; set; }
        public Guid PersonId { get; set; }
        public WishListItem WishListItem { get; set; }
    }
}
