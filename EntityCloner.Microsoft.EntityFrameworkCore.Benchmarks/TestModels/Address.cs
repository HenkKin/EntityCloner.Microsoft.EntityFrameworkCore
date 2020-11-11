namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels
{
    public class Address
    {
        public string Street { get; set; }
        public int HouseNumber { get; set; }
        public Country Country { get; set; }
        public int CountryId { get; set; }
    }
}