using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.ShoppingCarts
{
    public class ShoppingCartsRootObject : ISerializableObject
    {
        public ShoppingCartsRootObject()
        {
            ShoppingCarts = new List<ShoppingCartDto>();
        }

        [JsonProperty("shopping_carts")]
        public IList<ShoppingCartDto> ShoppingCarts { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "shopping_carts";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof (ShoppingCartItemDto);
        }
    }
}