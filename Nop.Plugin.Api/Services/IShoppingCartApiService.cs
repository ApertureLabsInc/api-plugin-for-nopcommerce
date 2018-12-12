using Nop.Plugin.Api.DTOs.ShoppingCarts;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Services
{
    public interface IShoppingCartApiService
    {
        ShoppingCartDto GetShoppingCart(int customerId, int shoppingCartTypeId);
        IList<ShoppingCartDto> GetShoppingCarts(ICollection<int> customerIds, ICollection<int> shoppingCartTypeIds);
        ShoppingCartDto UpdateShoppingCart(ShoppingCartDto shoppingCartDto);
        void DeleteShoppingCart(ShoppingCartDto shoppingCartDto);
    }
}