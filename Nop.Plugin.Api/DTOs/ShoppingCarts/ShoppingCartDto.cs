using FluentValidation.Attributes;
using Newtonsoft.Json;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.DTOs.Base;
using Nop.Plugin.Api.Validators;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTOs.ShoppingCarts
{
    [Validator(typeof(ShoppingCartDtoValidator))]
    [JsonObject(Title = "shopping_cart")]
    public class ShoppingCartDto : BaseDto
    {

        #region Private Fields

        private int? _shoppingCartTypeId;

        #endregion

        #region Constructors

        public ShoppingCartDto() : base()
        {
            Id = 1; //this is a hack to allow us to use the BaseDtoValidator (carts don't exist as an entity in nop, so there's no "real" id)

            AvailablePaymentMethods = new List<string>();
            AvailableShippingMethods = new List<string>();
            Coupons = new List<string>();
            Warnings = new List<string>();
        }

        #endregion

        #region Cart Overview

        [JsonProperty("customer_id")]
        public int? CustomerId { get; set; }

        [JsonProperty("shopping_cart_type")]
        public string ShoppingCartType
        {
            get
            {
                var shoppingCartTypeId = _shoppingCartTypeId;

                if (shoppingCartTypeId != null) return ((ShoppingCartType)shoppingCartTypeId).ToString();

                return null;
            }
            set
            {
                ShoppingCartType shoppingCartType;
                if (Enum.TryParse(value, true, out shoppingCartType))
                {
                    _shoppingCartTypeId = (int)shoppingCartType;
                }
                else _shoppingCartTypeId = null;
            }
        }

        [JsonProperty("shopping_cart_items")]
        public List<ShoppingCartItemDto> ShoppingCartItems { get; set; }

        [JsonProperty("coupons")]
        public List<string> Coupons { get; set; }

        [JsonProperty("warnings")]
        public List<string> Warnings { get; set; }

        #endregion

        #region Shipping

        [JsonProperty("shipping_method")]
        public string ShippingMethod { get; set; }

        [JsonProperty("available_shipping_methods")]
        public List<string> AvailableShippingMethods { get; set; }

        #endregion

        #region Payment

        [JsonProperty("payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty("available_payment_methods")]
        public List<string> AvailablePaymentMethods { get; set; }

        #endregion

        #region Totals

        //subtotal
        [JsonProperty("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonProperty("subtotal_discount")]
        public decimal SubtotalDiscount { get; set; }

        //shipping/payment
        [JsonProperty("shipping_cost")]
        public decimal Shipping { get; set; }

        [JsonProperty("payment_method_additional_fee")]
        public decimal PaymentMethodAdditionalFee { get; set; }

        //tax
        [JsonProperty("tax")]
        public decimal Tax { get; set; }

        //total
        [JsonProperty("total")]
        public decimal Total { get; set; }

        [JsonProperty("total_discount")]
        public decimal TotalDiscount { get; set; }

        #endregion

    }
}