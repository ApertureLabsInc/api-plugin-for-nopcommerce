using System;
using System.Collections.Generic;
using System.Linq;
using Hausera.Core.Shared.EnumsAndConstants;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.DataStructures;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Services
{
    public class ProductApiService : IProductApiService
    {
        private readonly IStoreMappingService _storeMappingService;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductCategory> _productCategoryMappingRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<RelatedProduct> _relatedProductsRepository;


        public ProductApiService(IRepository<Product> productRepository,
            IRepository<ProductCategory> productCategoryMappingRepository,
            IRepository<Vendor> vendorRepository,
            IRepository<RelatedProduct> relatedProductsRepository,
            IStoreMappingService storeMappingService)
        {
            _productRepository = productRepository;
            _productCategoryMappingRepository = productCategoryMappingRepository;
            _vendorRepository = vendorRepository;
            _storeMappingService = storeMappingService;
            _relatedProductsRepository = relatedProductsRepository;
        }

        public IList<Product> GetProducts(IList<int> ids = null,
            IList<string> skus = null,
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, DateTime? updatedAtMin = null, DateTime? updatedAtMax = null,
            int limit = Configurations.DefaultLimit, int page = Configurations.DefaultPageValue, int sinceId = Configurations.DefaultSinceId,
            int? categoryId = null, bool includeChildren = false, string vendorName = null, bool? publishedStatus = null)
        {
            var query = GetProductsQuery(createdAtMin, createdAtMax, updatedAtMin, updatedAtMax, vendorName, publishedStatus, ids, skus, categoryId, includeChildren);

            if (sinceId > 0)
            {
                query = query.Where(c => c.Id > sinceId);
            }

            return new ApiList<Product>(query, page - 1, limit);
        }
        
        public int GetProductsCount(DateTime? createdAtMin = null, DateTime? createdAtMax = null, 
            DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, bool? publishedStatus = null, string vendorName = null, 
            int? categoryId = null)
        {
            var query = GetProductsQuery(createdAtMin, createdAtMax, updatedAtMin, updatedAtMax, vendorName,
                                         publishedStatus, categoryId: categoryId);

            return query.ToList().Count(p => _storeMappingService.Authorize(p));
        }

        public Product GetProductById(int productId)
        {
            if (productId == 0)
                return null;

            return _productRepository.Table.FirstOrDefault(product => product.Id == productId && !product.Deleted);
        }

        public Product GetProductByIdNoTracking(int productId)
        {
            if (productId == 0)
                return null;

            return _productRepository.Table.FirstOrDefault(product => product.Id == productId && !product.Deleted);
        }

        private IQueryable<Product> GetProductsQuery(DateTime? createdAtMin = null, DateTime? createdAtMax = null, 
            DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, string vendorName = null, 
            bool? publishedStatus = null, IList<int> ids = null, IList<string> skus = null, int? categoryId = null, bool includeChildren = false)
        {
            var query = _productRepository.Table;

            if (ids != null && ids.Count > 0)
            {
                query = query.Where(c => ids.Contains(c.Id));
            }

            if (skus != null && skus.Count > 0)
            {
                query = query.Where(x => skus.Contains(x.Sku));
            }

            if (publishedStatus != null)
            {
                query = query.Where(c => c.Published == publishedStatus.Value);
            }

            // always return products that are not deleted!!!
            query = query.Where(c => !c.Deleted);

            if (createdAtMin != null)
            {
                query = query.Where(c => c.CreatedOnUtc > createdAtMin.Value);
            }

            if (createdAtMax != null)
            {
                query = query.Where(c => c.CreatedOnUtc < createdAtMax.Value);
            }

            if (updatedAtMin != null)
            {
               query = query.Where(c => c.UpdatedOnUtc > updatedAtMin.Value);
            }

            if (updatedAtMax != null)
            {
                query = query.Where(c => c.UpdatedOnUtc < updatedAtMax.Value);
            }

            if (!string.IsNullOrEmpty(vendorName))
            {
                query = from vendor in _vendorRepository.Table
                        join product in _productRepository.Table on vendor.Id equals product.VendorId
                        where vendor.Name == vendorName && !vendor.Deleted && vendor.Active
                        select product;
            }

            if (categoryId != null)
            {
                var categoryMappingsForProduct = from productCategoryMapping in _productCategoryMappingRepository.Table
                                                 where productCategoryMapping.CategoryId == categoryId
                                                 select productCategoryMapping;

                query = from product in query
                        join productCategoryMapping in categoryMappingsForProduct on product.Id equals productCategoryMapping.ProductId
                        select product;
            }

            if(includeChildren)
            {
                var childrenProducts = from childProduct in _productRepository.Table
                                       join product in query on childProduct.ParentGroupedProductId equals product.Id
                                       select childProduct;

                query = query.Union(childrenProducts);
            }

            query = query.OrderBy(product => product.Id);

            return query;
        }

        public IList<Product> GetRelatedProducts(int productId, ProductRelationshipType relationshipType, bool includeChildren = true)
        {
            var relatedProducts = from relatedProduct in _relatedProductsRepository.Table
                        where (relatedProduct.ProductId1 == productId || relatedProduct.ProductId2 == productId) && relatedProduct.ProductRelationshipType == relationshipType
                                  select relatedProduct;

            var query = from product in _productRepository.Table
                        from relatedProduct in relatedProducts 
                        where (product.Id == relatedProduct.ProductId1 || product.Id == relatedProduct.ProductId2) && product.Id != productId
                        orderby relatedProduct.DisplayOrder
                        select product;

            if (includeChildren)
            {
                var childrenProducts = from childProduct in _productRepository.Table
                                       join product in query on childProduct.ParentGroupedProductId equals product.Id
                                       select childProduct;

                query = query.Union(childrenProducts);
            }


            return query.ToList();
        }
    }
}