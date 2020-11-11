namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels
{


    public class Country
    {
        public int Id { get; set; }
        public byte[] RowVersion { get; set; }
        public string Name{ get; set; }
       
    }
}