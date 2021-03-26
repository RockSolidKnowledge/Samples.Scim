using Rsk.AspNetCore.Scim.EntityFramework.Entities;
using Rsk.AspNetCore.Scim.EntityFramework.Stores;

namespace EntityFramework.Entities
{
    public class OrganizationEntity : IResourceEntity
    {
        public string Id { get; set; }
        public Meta Meta { get; set; }
        public string ExternalId { get; set; }
        public string Schemas { get; set; }
        public string Name { get; set; }
        public int EmployeeCount { get; set; }
        public string CEO { get; set; }
    }
}
