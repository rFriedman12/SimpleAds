using System;

namespace SimpleAds30.Data
{
    public class Ad
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateAdded { get; set; }
        public string Details { get; set; }
    }
}
