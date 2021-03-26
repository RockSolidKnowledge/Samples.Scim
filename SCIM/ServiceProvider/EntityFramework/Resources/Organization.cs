using Rsk.AspNetCore.Scim.Attributes;
using Rsk.AspNetCore.Scim.Models;

namespace EntityFramework.Resources
{
    public class Organization : Resource
    {
        [Required(Required.Create, Required.Update)]
        [Unique]
        public string Name { get; set; }
        public int EmployeeCount { get; set; }
        public string CEO { get; set; }
    }
}
