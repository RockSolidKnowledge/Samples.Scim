using Rsk.AspNetCore.Scim.Attributes;
using Rsk.AspNetCore.Scim.Models;

namespace Extensions.Resources
{
    public class Organization : Resource
    {
        public Organization():base("Organization")
        {
            
        }
        [Required(Required.Create, Required.Update)]
        [Unique(Uniqueness.Server)]
        public string Name { get; set; }
        public int EmployeeCount { get; set; }
        public string CEO { get; set; }
    }
}
