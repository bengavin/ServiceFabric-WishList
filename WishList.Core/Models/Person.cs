using System;

namespace WishList.Core.Models
{
    [Serializable]
    public class Person
    {
        public Person()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string EmailAddress { get; set; }
        public string TwitterHandle { get; set; }
        public BehaviorRating BehaviorRating { get; set; }
    }
}
