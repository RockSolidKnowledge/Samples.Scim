using System;
using EntityFramework.Entities;
using EntityFramework.Resources;
using Rsk.AspNetCore.Scim.EntityFramework.Mappers;
using Rsk.AspNetCore.Scim.EntityFramework.Stores;

namespace EntityFramework.Mappers
{
    public class OrganizationMapper : IMapper<Organization, OrganizationEntity>
    {
        private readonly IMapMetadata metadataMapper;

        public OrganizationMapper(IMapMetadata metadataMapper)
        {
            this.metadataMapper = metadataMapper ?? throw new ArgumentNullException(nameof(metadataMapper));
        }

        public Organization ToDomain(OrganizationEntity entity)
        {
            var organization = new Organization
            {
                CEO = entity.CEO,
                EmployeeCount = entity.EmployeeCount,
                Name = entity.Name,
                Meta = metadataMapper.ToDomain(entity.Meta),
                Id = entity.Id,
                ExternalId = entity.ExternalId
            };

            return organization;
        }

        public OrganizationEntity ToEntity(Organization domain)
        {
            var entity = new OrganizationEntity
            {
                EmployeeCount = domain.EmployeeCount,
                Name = domain.Name,
                Id = domain.Id,
                CEO = domain.CEO,
                ExternalId = domain.ExternalId
            };

            return entity;
        }
    }
}