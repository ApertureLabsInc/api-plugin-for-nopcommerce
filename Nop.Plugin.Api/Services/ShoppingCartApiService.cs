using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.DTOs.ShoppingCarts;
using Nop.Plugin.Api.Helpers;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;

namespace Nop.Plugin.Api.Services
{
    public class ShoppingCartApiService : IShoppingCartApiService
    {

        #region Private Fields

        private readonly IStoreContext _storeContext;

        private readonly IRepository<ShoppingCartItem> _shoppingCartItemsRepository;

        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;

        private readonly IDTOHelper _dtoHelper;
        private readonly IProductAttributeConverter _productAttributeConverter;

        #endregion

        #region Constructors

        public ShoppingCartApiService(IStoreContext storeContext, IRepository<ShoppingCartItem> shoppingCartItemsRepository, ICustomerService customerService, IShoppingCartService shoppingCartService, IProductService productService, IDTOHelper dtoHelper, IProductAttributeConverter productAttributeConverter)
        {
            _storeContext = storeContext;

            _customerService = customerService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;

            _shoppingCartItemsRepository = shoppingCartItemsRepository;

            _productAttributeConverter = productAttributeConverter;
            _dtoHelper = dtoHelper;
        }

        #endregion

        #region Public Methods

        public ShoppingCartDto GetShoppingCart(int customerId, int shoppingCartTypeId)
        {
            var customerIds = new List<int> { customerId };
            var shoppingCartTypeIds = new List<int> { shoppingCartTypeId };

            var shoppingCartType = (ShoppingCartType)shoppingCartTypeId;

            ShoppingCartDto shoppingCartDto = null;

            var shoppingCartDtos = GetShoppingCarts(customerIds: customerIds, shoppingCartTypeIds: shoppingCartTypeIds);
            if (shoppingCartDtos.Any(x => x.CustomerId == customerId && x.ShoppingCartType == shoppingCartType.ToString()))
            {
                shoppingCartDto = shoppingCartDtos.Single();
            }
            else
            {
                //return an empty shopping cart if the customer does not have any shopping cart items for the specific shopping cart type
                shoppingCartDto = new ShoppingCartDto() { CustomerId = customerId, ShoppingCartType = ((ShoppingCartType)shoppingCartTypeId).ToString(), ShoppingCartItems = new List<ShoppingCartItemDto>() };
            }

            return shoppingCartDto;
        }

        public IList<ShoppingCartDto> GetShoppingCarts(ICollection<int> customerIds = null, ICollection<int> shoppingCartTypeIds = null)
        {
            var shoppingCartItems = _shoppingCartItemsRepository.Table;

            //filter by passed in params
            if (customerIds != null && customerIds.Count > 0)
            {
                shoppingCartItems = shoppingCartItems.Where(x => customerIds.Contains(x.CustomerId));
            }

            if (shoppingCartTypeIds != null && shoppingCartTypeIds.Count > 0)
            {
                shoppingCartItems = shoppingCartItems.Where(x => shoppingCartTypeIds.Contains(x.ShoppingCartTypeId));
            }

            //group by customer and shopping cart type
            var groupedShoppingCartItems = shoppingCartItems.GroupBy(x => new { x.CustomerId, x.ShoppingCartTypeId });

            //have to do this in a foreach (EF breaks if you try to do it via Select)
            var shoppingCartDtos = new List<ShoppingCartDto>();
            foreach (var shoppingCart in groupedShoppingCartItems)
            {
                var shoppingCartDto = _dtoHelper.PrepareShoppingCartDTO(shoppingCart.Key.CustomerId, shoppingCart.Key.ShoppingCartTypeId, shoppingCart);
                shoppingCartDtos.Add(shoppingCartDto);
            }

            return shoppingCartDtos;
        }

        public ShoppingCartDto UpdateShoppingCart(ShoppingCartDto shoppingCartDto)
        {
            var customer = _customerService.GetCustomerById(shoppingCartDto.CustomerId.Value);
            var shoppingCartTypeId = (int)Enum.Parse<ShoppingCartType>(shoppingCartDto.ShoppingCartType);

            //add/update shopping cart items
            foreach (var shoppingCartItemDto in shoppingCartDto.ShoppingCartItems)
            {
                var attributesXml = shoppingCartItemDto.Attributes != null && shoppingCartItemDto.Attributes.Any() ? _productAttributeConverter.ConvertToXml(shoppingCartItemDto.Attributes, shoppingCartItemDto.ProductId.Value) : null;
                var customerEnteredPrice = shoppingCartItemDto.CustomerEnteredPrice.HasValue ? shoppingCartItemDto.CustomerEnteredPrice.Value : 0;
                var quantity = shoppingCartItemDto.Quantity.HasValue ? shoppingCartItemDto.Quantity.Value : 1;

                if (shoppingCartItemDto.Id > 0)
                {
                    _shoppingCartService.UpdateShoppingCartItem(customer, shoppingCartItemDto.Id, attributesXml, customerEnteredPrice, shoppingCartItemDto.RentalStartDateUtc, shoppingCartItemDto.RentalEndDateUtc, quantity);
                }
                else
                {
                    var product = _productService.GetProductById(shoppingCartItemDto.ProductId.Value);
                    _shoppingCartService.AddToCart(customer, product, (ShoppingCartType)shoppingCartTypeId, _storeContext.CurrentStore.Id, attributesXml, customerEnteredPrice, shoppingCartItemDto.RentalStartDateUtc, shoppingCartItemDto.RentalEndDateUtc, quantity);
                }
            }

            //get updated shopping cart items and use them to refresh the dto
            var shoppingCartItems = _shoppingCartItemsRepository.Table.Where(x => x.CustomerId == shoppingCartDto.CustomerId && x.ShoppingCartTypeId == shoppingCartTypeId).ToList();
            shoppingCartDto = _dtoHelper.PrepareShoppingCartDTO(customer.Id, shoppingCartTypeId, shoppingCartItems);

            return shoppingCartDto;
        }

        public void DeleteShoppingCart(ShoppingCartDto shoppingCartDto)
        {
            foreach (var shoppingCartItemDto in shoppingCartDto.ShoppingCartItems)
            {
                var shoppingCartItem = _shoppingCartItemsRepository.GetById(shoppingCartItemDto.Id);
                _shoppingCartItemsRepository.Delete(shoppingCartItem);
            }
        }

        #endregion

        #region Private Methods



        #endregion

    }
}