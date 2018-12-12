using FluentValidation;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.DTOs.ShoppingCarts;
using Nop.Plugin.Api.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Nop.Plugin.Api.Validators
{
    public class ShoppingCartDtoValidator : BaseDtoValidator<ShoppingCartDto>
    {

        #region Constructors

        public ShoppingCartDtoValidator(IHttpContextAccessor httpContextAccessor, IJsonHelper jsonHelper, Dictionary<string, object> requestJsonDictionary) : base(httpContextAccessor, jsonHelper, requestJsonDictionary)
        {
            SetCustomerIdRule();
            SetShoppingCartTypeRule();

            SetShoppingCartItemsRule();
        }

        #endregion

        #region Private Methods

        private void SetCustomerIdRule()
        {
            //require customer id, even though we also validate it at the cart item level
            SetGreaterThanZeroRule(x => x.CustomerId, "invalid customer_id");
        }

        private void SetShoppingCartItemsRule()
        {
            var key = "shopping_cart_items";
            if (RequestJsonDictionary.ContainsKey(key))
            {
                RuleForEach(c => c.ShoppingCartItems)
                    .Custom((shoppingCartItemDto, validationContext) =>
                    {
                        var shoppingCartItemsJsonDictionary = GetRequestJsonDictionaryCollectionItemDictionary(key, shoppingCartItemDto);

                        var validator = new ShoppingCartItemDtoValidator(HttpContextAccessor, JsonHelper, shoppingCartItemsJsonDictionary);

                        //force create validation for new shopping cart items
                        if (shoppingCartItemDto.Id == 0)
                        {
                            validator.HttpMethod = HttpMethod.Post;
                        }

                        var validationResult = validator.Validate(shoppingCartItemDto);

                        MergeValidationResult(validationContext, validationResult);
                    });
            }
        }

        private void SetShoppingCartTypeRule()
        {
            //require shopping cart type even though we also validate it at the cart item level
            RuleFor(x => x.ShoppingCartType)
                .Must(x =>
                {
                    var parsed = Enum.TryParse(x, true, out ShoppingCartType _);
                    return parsed;
                })
                .WithMessage("Please provide a valid shopping cart type");
        }

        #endregion

    }
}