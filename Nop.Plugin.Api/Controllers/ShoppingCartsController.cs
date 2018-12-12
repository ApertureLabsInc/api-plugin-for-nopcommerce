using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTOs.Errors;
using Nop.Plugin.Api.DTOs.ShoppingCarts;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.ShoppingCartsParameters;
using Nop.Plugin.Api.Services;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Net;

namespace Nop.Plugin.Api.Controllers
{
    [ApiAuthorize(Policy = JwtBearerDefaults.AuthenticationScheme, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ShoppingCartsController : BaseApiController
    {

        private readonly IShoppingCartApiService _shoppingCartApiService;
        private readonly IDTOHelper _dtoHelper;

        public ShoppingCartsController(IJsonFieldsSerializer jsonFieldsSerializer, 
                                  ICustomerActivityService customerActivityService,
                                  ILocalizationService localizationService,
                                  IAclService aclService,
                                  IStoreMappingService storeMappingService,
                                  IStoreService storeService,
                                  ICustomerService customerService,
                                  IDiscountService discountService,
                                  IPictureService pictureService,
                                  IShoppingCartApiService shoppingCartApiService,
                                  IDTOHelper dtoHelper) : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _shoppingCartApiService = shoppingCartApiService;
            _dtoHelper = dtoHelper;
        }

        [HttpGet]
        [Route("/api/shopping_carts/{customerId}")]
        [ProducesResponseType(typeof(ShoppingCartsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetShoppingCart(int customerId, int shoppingCartTypeId = 1)
        {
            var shoppingCartDto = _shoppingCartApiService.GetShoppingCart(customerId, shoppingCartTypeId);

            var shoppingCartsRootObject = new ShoppingCartsRootObject();
            shoppingCartsRootObject.ShoppingCarts.Add(shoppingCartDto);

            var json = JsonFieldsSerializer.Serialize(shoppingCartsRootObject, "");

            return new RawJsonActionResult(json);
        }

        [HttpGet]
        [Route("/api/shopping_carts")]
        [ProducesResponseType(typeof(ShoppingCartsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetShoppingCarts(ShoppingCartsParametersModel parameters)
        {
            var shoppingCartDtos = _shoppingCartApiService.GetShoppingCarts(parameters.CustomerIds, parameters.ShoppingCartTypeIds);

            var ShoppingCartsRootObject = new ShoppingCartsRootObject()
            {
                ShoppingCarts = shoppingCartDtos
            };

            var json = JsonFieldsSerializer.Serialize(ShoppingCartsRootObject, "");

            return new RawJsonActionResult(json);
        }

        [HttpPut]
        [Route("/api/shopping_carts")]
        [ProducesResponseType(typeof(ShoppingCartsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]

        public IActionResult UpdateShoppingCart([ModelBinder(typeof(JsonModelBinder<ShoppingCartDto>))] Delta<ShoppingCartDto> shoppingCartDtoDelta)
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var shoppingCartDto = _shoppingCartApiService.GetShoppingCart(shoppingCartDtoDelta.Dto.CustomerId.Value, (int)Enum.Parse<ShoppingCartType>(shoppingCartDtoDelta.Dto.ShoppingCartType));

            shoppingCartDtoDelta.Merge(shoppingCartDto);

            shoppingCartDto = _shoppingCartApiService.UpdateShoppingCart(shoppingCartDto);

            var shoppingCartsRootObject = new ShoppingCartsRootObject();
            shoppingCartsRootObject.ShoppingCarts.Add(shoppingCartDto);

            var json = JsonFieldsSerializer.Serialize(shoppingCartsRootObject, "");

            return new RawJsonActionResult(json);
        }

        [HttpDelete]
        [Route("/api/shopping_carts/{customerId}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public IActionResult DeleteShoppingCartByCustomerId(int customerId, int shoppingCartTypeId = 1)
        {
            if (customerId <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "customer id", "invalid customer id");
            }

            var shoppingCartDto = _shoppingCartApiService.GetShoppingCart(customerId, shoppingCartTypeId);
            if (shoppingCartDto == null)
            {
                return Error(HttpStatusCode.NotFound, "shopping cart", "not found");
            }

            _shoppingCartApiService.DeleteShoppingCart(shoppingCartDto);

            return new RawJsonActionResult("{}");
        }
    }
}