using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.DTOs.ShoppingCarts;
using Nop.Plugin.Api.Helpers;

namespace Nop.Plugin.Api.Services
{
    public class ShoppingCartApiService : IShoppingCartApiService
    {

        #region Private Fields

        private readonly IRepository<ShoppingCartItem> _shoppingCartItemsRepository;
        private readonly IDTOHelper _dtoHelper;

        #endregion

        #region Constructors

        public ShoppingCartApiService(IRepository<ShoppingCartItem> shoppingCartItemsRepository, IDTOHelper dtoHelper)
        {
            _shoppingCartItemsRepository = shoppingCartItemsRepository;
            _dtoHelper = dtoHelper;
        }

        #endregion

        #region Public Methods

        public ShoppingCartDto GetShoppingCart(int customerId, int shoppingCartTypeId)
        {
            var customerIds = new List<int> { customerId };
            var shoppingCartTypeIds = new List<int> { shoppingCartTypeId };

            var shoppingCartDto = GetShoppingCarts(customerIds: customerIds, shoppingCartTypeIds: shoppingCartTypeIds).Single();

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
            throw new System.NotImplementedException();
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