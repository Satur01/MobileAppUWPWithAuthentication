using Microsoft.Azure.Mobile.Server;

namespace telmexhubmobileappService.DataObjects
{
    public class Person : EntityData
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }
    }
}