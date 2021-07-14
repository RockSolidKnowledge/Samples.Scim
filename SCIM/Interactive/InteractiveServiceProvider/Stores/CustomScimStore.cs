using System;
using Microsoft.AspNetCore.Authentication;
using Rsk.AspNetCore.Scim.Factories;
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Stores;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rsk.AspNetCore.Scim.Attributes;
using Rsk.AspNetCore.Scim.Constants;
using Rsk.AspNetCore.Scim.Enums;
using Rsk.AspNetCore.Scim.Exceptions;
using Rsk.AspNetCore.Scim.Extensions;
using Rsk.AspNetCore.Scim.Results;

namespace InteractiveServiceProvider.Stores
{
    public class MyInMemoryScimStore<T> : IScimStore<T> where T : Resource
    {
        private readonly ICollection<T> resources;
        private readonly ISystemClock systemClock;
        private readonly IExpressionFactory expressionFactory;
        private readonly IStoreScimExtensions extensionStore;

        public MyInMemoryScimStore(ISystemClock systemClock, IExpressionFactory expressionFactory, IStoreScimExtensions extensionStore) :
                    this(new List<T>(), systemClock, expressionFactory, extensionStore)
        {   
        }
        
        public Task<IList<T>> GetAll()
        {
            lock (resources)
            {
                var allResources = resources.ToList();
                return Task.FromResult<IList<T>>(allResources);
            }
        }

        internal MyInMemoryScimStore(ICollection<T> resources, ISystemClock systemClock, IExpressionFactory expressionFactory, IStoreScimExtensions extensionStore)
        {
            this.resources = resources ?? throw new ArgumentNullException(nameof(resources));
            this.systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
            this.expressionFactory = expressionFactory ?? throw new ArgumentNullException(nameof(expressionFactory));
            this.extensionStore = extensionStore ?? throw new ArgumentNullException(nameof(extensionStore));
        }
        
        public async Task<IScimResult<T>> Add(T resource, IEnumerable<ScimExtensionValue> scimExtensions, string resourceSchema)
        {
            if (resource == null) throw new ArgumentNullException();
            if (scimExtensions == null) throw new ArgumentNullException();

            var addResult =  await AddToStore(resource);

            if (addResult.Status == ScimResultStatus.Failure) return addResult;

            var extensionAddResult = await extensionStore.AddExtensions(scimExtensions, addResult.Resource.Id, resourceSchema);

            return ScimResult<T>.Map(resource, extensionAddResult);
        }

        public Task<T> GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException(nameof(id));

            lock (resources)
            {
                var existingResource = resources.FirstOrDefault(resource => resource.Id == id);
                return Task.FromResult(existingResource);
            }
        }
        
        public async Task<IScimResult<T>> Update(T resource, IEnumerable<ScimExtensionValue> scimExtensions, string resourceSchema)
        {
            if (resource == null) throw new ArgumentNullException();
            if (scimExtensions == null) throw new ArgumentNullException();

            var updateResult = await TryUpdateResource(resource);

            if (updateResult.Status == ScimResultStatus.Failure) return updateResult;

            var extensionUpdateResult = await extensionStore.UpdateExtensions(scimExtensions, updateResult.Resource.Id, resourceSchema);

            return ScimResult<T>.Map(resource, extensionUpdateResult);
        }

        internal virtual async Task<IScimResult<T>> TryUpdateResource(T resource)
        {
            var existingResource = await GetById(resource.Id);

            if (existingResource == null) throw new ScimStoreException("Resource does not exist");

            Sanitize(resource, existingResource, typeof(T));

            lock (resources)
            {
                resources.Remove(existingResource);

                if (!IsUnique(resource, resources))
                {
                    resources.Add(existingResource);
                    {
                        return ScimResult<T>.Error(ScimStatusCode.Status409Conflict);
                    }
                }

                resource.Meta = new Metadata
                {
                    Created = existingResource.Meta.Created,
                    LastModified = systemClock.UtcNow
                };

                resources.Add(resource);
            }

            return ScimResult<T>.Success(resource);
        }

        internal virtual Task<IScimResult<T>> AddToStore(T resource)
        {
            lock (resources)
            {
                if (!IsUnique(resource, resources))
                {
                    return Task.FromResult((IScimResult<T>)ScimResult<T>.Error(ScimStatusCode.Status409Conflict,
                        "Resource already exists in the store"));
                }

                var createdTime = systemClock.UtcNow;

                resource.Id = Guid.NewGuid().ToString();
                resource.Meta = new Metadata
                {
                    Created = createdTime,
                    LastModified = createdTime
                };

                resources.Add(resource);
            }

            return Task.FromResult((IScimResult<T>)ScimResult<T>.Success(resource));
        }

        internal virtual bool IsUnique(T entity, ICollection<T> collection)
        {
            var expression = expressionFactory.CreateUniquenessExpression(entity, entity);
            if (expression != null)
            {
                var storeResource = collection.FirstOrDefault(expression);

                if (storeResource != null)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Modifies the inResource so that an update operation will not replace readOnly properties with new values.
        /// </summary>
        internal virtual void Sanitize(object inResource, object storeResource, Type resourceType)
        {
            if (inResource == null && storeResource == null) return;

            var resourceProperties = resourceType.GetProperties(ScimConstants.DefaultFlags);

            foreach (var resourceProperty in resourceProperties)
            {
                var attributes = resourceProperty.GetCustomAttributes(typeof(MutabilityAttribute), true);

                if (attributes.Length > 1)
                {
                    continue;
                }

                var readOnly = attributes.Any(a =>
                {
                    var mutabilityAttribute = a as MutabilityAttribute;

                    return mutabilityAttribute.Value == Mutability.ReadOnly;
                });

                if (readOnly)
                {
                    var storeValue = resourceProperty.GetValue(storeResource, null);
                    resourceProperty.SetValue(inResource, storeValue, null);
                }
                else
                {
                    var isSimple = resourceProperty.PropertyType.IsSimple();

                    if (!isSimple)
                    {
                        var inPropValue = resourceProperty.GetValue(inResource, null);
                        var storePropValue = resourceProperty.GetValue(storeResource, null);
                        Sanitize(inPropValue, storePropValue, resourceProperty.PropertyType);
                    }
                }
            }
        }
        
        public Task<IEnumerable<(bool Exists, string Id)>> Exists(IEnumerable<string> ids)
        {
            lock (resources)
            {
                return Task.FromResult(ids.Select(id => (Exists: resources.Any(r => r.Id == id), Id: id)));
            }
        }

        public async Task<IScimResult> Delete(string id, string resourceSchema)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(resourceSchema)) throw new ArgumentNullException(nameof(resourceSchema));

            var existingResource = await GetById(id);

            if (existingResource == null) throw new ScimStoreException("Resource does not exist");

            lock (resources)
            {
                resources.Remove(existingResource);
            }

            var extensionStoreResult = await extensionStore.Delete(id, resourceSchema);

            return extensionStoreResult;
        }

        public Task<IScimResult<IEnumerable<ScimExtensionValue>>> GetExtensionsForResource(string resourceId, string resourceSchema)
        {
            if(string.IsNullOrWhiteSpace(resourceId)) throw new ArgumentException(nameof(resourceId));
            if(string.IsNullOrWhiteSpace(resourceSchema)) throw new ArgumentException(nameof(resourceSchema));

            return extensionStore.GetExtensions(resourceId, resourceSchema);
        }
    }
}