using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Api.DTOs.Categories;
using Nop.Plugin.Api.DTOs.Customers;
using Nop.Plugin.Api.DTOs.Images;
using Nop.Plugin.Api.DTOs.Languages;
using Nop.Plugin.Api.DTOs.OrderItems;
using Nop.Plugin.Api.DTOs.Orders;
using Nop.Plugin.Api.DTOs.Manufacturers;
using Nop.Plugin.Api.DTOs.ProductAttributes;
using Nop.Plugin.Api.DTOs.Products;
using Nop.Plugin.Api.DTOs.ShoppingCarts;
using Nop.Plugin.Api.DTOs.SpecificationAttributes;
using Nop.Plugin.Api.DTOs.Stores;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.Services;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Common;
using Nop.Services.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Tax;

namespace Nop.Plugin.Api.Helpers
{
    public class DTOHelper : IDTOHelper
    {
        private readonly IAclService _aclService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerApiService _customerApiService;
        private readonly IDiscountService _discountService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly IPictureService _pictureService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductAttributeConverter _productAttributeConverter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;
        private readonly IUrlRecordService _urlRecordService;

        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        private readonly CurrencySettings _currencySettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;

        public DTOHelper(IProductService productService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPaymentService paymentService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IProductAttributeService productAttributeService,
            ICustomerService customerService,
            ICustomerApiService customerApiService,
            IDiscountService discountService,
            IGenericAttributeService genericAttributeService,
            IProductAttributeConverter productAttributeConverter,
            IProductAttributeParser productAttributeParser,
            ILanguageService languageService,
            ICurrencyService currencyService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStoreService storeService,
            ITaxService taxService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            IProductTagService productTagService,
            IStoreContext storeContext,
            IWorkContext workContext,
            CurrencySettings currencySettings,
            OrderSettings orderSettings,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings)
        {
            _productService = productService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _paymentService = paymentService;
            _pictureService = pictureService;
            _priceCalculationService = priceCalculationService;
            _productAttributeService = productAttributeService;
            _customerService = customerService;
            _customerApiService = customerApiService;
            _discountService = discountService;
            _genericAttributeService = genericAttributeService;
            _productAttributeConverter = productAttributeConverter;
            _productAttributeParser = productAttributeParser;
            _languageService = languageService;
            _currencyService = currencyService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _storeService = storeService;
            _taxService = taxService;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _productTagService = productTagService;

            _storeContext = storeContext;
            _workContext = workContext;

            _currencySettings = currencySettings;
            _orderSettings = orderSettings;
            _shippingSettings = shippingSettings;
            _taxSettings = taxSettings;
        }

        public ProductDto PrepareProductDTO(Product product)
        {
            var productDto = product.ToDto();

            PrepareProductImages(product.ProductPictures, productDto);

            productDto.SeName = _urlRecordService.GetSeName(product);
            productDto.DiscountIds = product.AppliedDiscounts.Select(discount => discount.Id).ToList();
            productDto.ManufacturerIds = product.ProductManufacturers.Select(pm => pm.ManufacturerId).ToList();
            productDto.RoleIds = _aclService.GetAclRecords(product).Select(acl => acl.CustomerRoleId).ToList();
            productDto.StoreIds = _storeMappingService.GetStoreMappings(product).Select(mapping => mapping.StoreId)
                .ToList();
            productDto.Tags = _productTagService.GetAllProductTagsByProductId(product.Id).Select(tag => tag.Name)
                .ToList();

            productDto.AssociatedProductIds =
                _productService.GetAssociatedProducts(product.Id, showHidden: true)
                    .Select(associatedProduct => associatedProduct.Id)
                    .ToList();

            var allLanguages = _languageService.GetAllLanguages();

            productDto.LocalizedNames = new List<LocalizedNameDto>();

            foreach (var language in allLanguages)
            {
                var localizedNameDto = new LocalizedNameDto
                {
                    LanguageId = language.Id,
                    LocalizedName = _localizationService.GetLocalized(product, x => x.Name, language.Id)
                };

                productDto.LocalizedNames.Add(localizedNameDto);
            }

            return productDto;
        }

        public CategoryDto PrepareCategoryDTO(Category category)
        {
            var categoryDto = category.ToDto();

            var picture = _pictureService.GetPictureById(category.PictureId);
            var imageDto = PrepareImageDto(picture);

            if (imageDto != null)
            {
                categoryDto.Image = imageDto;
            }

            categoryDto.SeName = _urlRecordService.GetSeName(category);
            categoryDto.DiscountIds = category.AppliedDiscounts.Select(discount => discount.Id).ToList();
            categoryDto.RoleIds = _aclService.GetAclRecords(category).Select(acl => acl.CustomerRoleId).ToList();
            categoryDto.StoreIds = _storeMappingService.GetStoreMappings(category).Select(mapping => mapping.StoreId)
                .ToList();

            var allLanguages = _languageService.GetAllLanguages();

            categoryDto.LocalizedNames = new List<LocalizedNameDto>();

            foreach (var language in allLanguages)
            {
                var localizedNameDto = new LocalizedNameDto
                {
                    LanguageId = language.Id,
                    LocalizedName = _localizationService.GetLocalized(category, x => x.Name, language.Id)
                };

                categoryDto.LocalizedNames.Add(localizedNameDto);
            }

            return categoryDto;
        }

        public OrderDto PrepareOrderDTO(Order order)
        {
            var orderDto = order.ToDto();

            orderDto.OrderItems = order.OrderItems.Select(PrepareOrderItemDTO).ToList();

            var customerDto = _customerApiService.GetCustomerById(order.Customer.Id);

            if (customerDto != null)
            {
                orderDto.Customer = customerDto.ToOrderCustomerDto();
            }

            return orderDto;
        }

        public ShoppingCartDto PrepareShoppingCartDTO(int customerId, int shoppingCartTypeId, IEnumerable<ShoppingCartItem> shoppingCartItems = null)
        {
            ///NOTE:
            ///Most of this logic is borrowed from ShoppingCartModelFactory

            var customer = _customerService.GetCustomerById(customerId);

            var selectedPaymentMethodSystemName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);
            var selectedPaymentMethod = _paymentService.LoadPaymentMethodBySystemName(selectedPaymentMethodSystemName);

            var shoppingCartItemsList = shoppingCartItems.ToList();

            var shoppingCartDto = new ShoppingCartDto();

            #region Cart Overview

            shoppingCartDto.CustomerId = customerId;
            shoppingCartDto.ShoppingCartType = ((ShoppingCartType)shoppingCartTypeId).ToString();

            shoppingCartDto.ShoppingCartItems = shoppingCartItems.Select(x => PrepareShoppingCartItemDTO(x, customer)).ToList();

            var couponCodes = _customerService.ParseAppliedDiscountCouponCodes(customer);
            foreach (var couponCode in couponCodes)
            {
                var discount = _discountService.GetAllDiscountsForCaching(couponCode: couponCode)
                    .FirstOrDefault(d => d.RequiresCouponCode && _discountService.ValidateDiscount(d, customer).IsValid);

                if (discount != null)
                {
                    shoppingCartDto.Coupons.Add(discount.CouponCode);
                }
            }

            shoppingCartDto.Warnings.AddRange(_shoppingCartService.GetShoppingCartWarnings(shoppingCartItemsList, "", false));

            #endregion

            #region Payment

            var availablePaymentMethods = _paymentService.LoadActivePaymentMethods(customer, _storeContext.CurrentStore.Id).Where(pm => !pm.HidePaymentMethod(shoppingCartItems.ToList())).ToList();
            shoppingCartDto.AvailablePaymentMethods.AddRange(availablePaymentMethods.Select(x => x.PaymentMethodDescription));

            shoppingCartDto.PaymentMethod = selectedPaymentMethod?.PaymentMethodDescription;

            #endregion

            #region Shipping

            var address = new Address //TODO:  Cart - need to pass some/all of these in to get estimates
            {
                CountryId = null,
                Country = null,
                StateProvinceId = null,
                StateProvince = null,
                ZipPostalCode = null,
            };

            var getShippingOptionResponse = _shippingService.GetShippingOptions(shoppingCartItemsList, address, customer, storeId: _storeContext.CurrentStore.Id);
            if (getShippingOptionResponse.Success)
            {
                shoppingCartDto.AvailableShippingMethods.AddRange(getShippingOptionResponse.ShippingOptions.Select(x => x.Name));

                //TODO:  Cart - return shipping options with cost
                //foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                //{
                //    var shippingRate = _orderTotalCalculationService.AdjustShippingRate(shippingOption.Rate, cart, out List<DiscountForCaching> _);
                //    shippingRate = _taxService.GetShippingPrice(shippingRate, _workContext.CurrentCustomer);
                //    shippingRate = _currencyService.ConvertFromPrimaryStoreCurrency(shippingRate, _workContext.WorkingCurrency);
                //    var shippingRateString = _priceFormatter.FormatShippingPrice(shippingRate, true);

                //    model.ShippingOptions.Add(new EstimateShippingResultModel.ShippingOptionModel
                //    {
                //        Name = shippingOption.Name,
                //        Description = shippingOption.Description,
                //        Price = shippingRateString
                //    });
                //}
            }
            else
            {
                shoppingCartDto.Warnings.AddRange(getShippingOptionResponse.Errors);
            }

            if (_shippingSettings.AllowPickUpInStore)
            {
                //TODO:  Cart - support in store pickup?
            }

            var selectedShippingMethod = _genericAttributeService.GetAttribute<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, _storeContext.CurrentStore.Id);
            shoppingCartDto.ShippingMethod = selectedShippingMethod?.Name;

            #endregion

            #region Totals

            //subtotal
            var includeTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
            _orderTotalCalculationService.GetShoppingCartSubTotal(shoppingCartItemsList, includeTax, out decimal orderSubTotalDiscountAmountBase, out List<DiscountForCaching> _, out decimal subTotalWithoutDiscountBase, out decimal _);

            var subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subTotalWithoutDiscountBase, _workContext.WorkingCurrency);
            shoppingCartDto.Subtotal = subtotal;

            if (orderSubTotalDiscountAmountBase > decimal.Zero)
            {
                var subtotalDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(orderSubTotalDiscountAmountBase, _workContext.WorkingCurrency);
                shoppingCartDto.SubtotalDiscount = subtotalDiscount;
            }

            //shipping/payment fees
            var shipping = _orderTotalCalculationService.GetShoppingCartShippingTotal(shoppingCartItemsList);
            if (shipping.HasValue)
            {
                shipping = _currencyService.ConvertFromPrimaryStoreCurrency(shipping.Value, _workContext.WorkingCurrency);
                shoppingCartDto.Shipping = shipping.Value;
            }

            if (selectedPaymentMethod != null)
            {
                shoppingCartDto.PaymentMethodAdditionalFee = selectedPaymentMethod.GetAdditionalHandlingFee(shoppingCartItemsList);
            }

            //tax
            var tax = _orderTotalCalculationService.GetTaxTotal(shoppingCartItemsList, out SortedDictionary<decimal, decimal> taxRates);
            tax = _currencyService.ConvertFromPrimaryStoreCurrency(tax, _workContext.WorkingCurrency);
            shoppingCartDto.Tax = tax;

            //total
            var total = _orderTotalCalculationService.GetShoppingCartTotal(shoppingCartItemsList, out decimal orderTotalDiscountAmountBase, out List<DiscountForCaching> _, out List<AppliedGiftCard> appliedGiftCards, out int redeemedRewardPoints, out decimal redeemedRewardPointsAmount);
            if (!total.HasValue)
            {
                shoppingCartDto.Warnings.Add("Order total couldn't be calculated");
            }

            if (total.HasValue)
            {
                total = _currencyService.ConvertFromPrimaryStoreCurrency(total.Value, _workContext.WorkingCurrency);
                shoppingCartDto.Total = total.Value;
            }

            if (orderTotalDiscountAmountBase > decimal.Zero)
            {
                var orderTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderTotalDiscountAmountBase, _workContext.WorkingCurrency);
                shoppingCartDto.TotalDiscount = orderTotalDiscountAmount;
            }

            #endregion

            return shoppingCartDto;
        }

        public ShoppingCartItemDto PrepareShoppingCartItemDTO(ShoppingCartItem shoppingCartItem, Customer customer = null)
        {
            customer = customer ?? _customerService.GetCustomerById(shoppingCartItem.CustomerId);

            var shoppingCartItemDto = shoppingCartItem.ToDto();

            shoppingCartItemDto.Attributes = _productAttributeConverter.Parse(shoppingCartItem.AttributesXml);
            
            shoppingCartItemDto.ProductDto = PrepareProductDTO(shoppingCartItem.Product);
            shoppingCartItemDto.CustomerDto = shoppingCartItem.Customer.ToCustomerForShoppingCartItemDto();

            var productAttributeCombination = _productAttributeParser.FindProductAttributeCombination(shoppingCartItem.Product, shoppingCartItem.AttributesXml);
            if (productAttributeCombination != null)
            {
                shoppingCartItemDto.ProductAttributeCombinationId = productAttributeCombination.Id;
            }

            ///NOTE:
            ///Most of this logic is borrowed from ShoppingCartModelFactory

            //unit price
            if (!shoppingCartItem.Product.CallForPrice)
            {
                var unitPrice = _taxService.GetProductPrice(shoppingCartItem.Product, _priceCalculationService.GetUnitPrice(shoppingCartItem), out decimal _);
                unitPrice = _currencyService.ConvertFromPrimaryStoreCurrency(unitPrice, _workContext.WorkingCurrency);
                shoppingCartItemDto.UnitPrice = unitPrice;
            }

            //subtotal/discount
            if (!shoppingCartItem.Product.CallForPrice)
            {
                var shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(shoppingCartItem.Product, _priceCalculationService.GetSubTotal(shoppingCartItem, true, out decimal shoppingCartItemDiscountBase, out List<DiscountForCaching> _, out int? maximumDiscountQty), out decimal taxRate);
                var shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
                shoppingCartItemDto.Subtotal = shoppingCartItemSubTotalWithDiscount;

                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    shoppingCartItemDiscountBase = _taxService.GetProductPrice(shoppingCartItem.Product, shoppingCartItemDiscountBase, out taxRate);
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        var shoppingCartItemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                        shoppingCartItemDto.Discount = shoppingCartItemDiscount;
                    }
                }
            }

            //warnings
            var warnings = _shoppingCartService.GetShoppingCartItemWarnings(
                customer,
                shoppingCartItem.ShoppingCartType,
                shoppingCartItem.Product,
                shoppingCartItem.StoreId,
                shoppingCartItem.AttributesXml,
                shoppingCartItem.CustomerEnteredPrice,
                shoppingCartItem.RentalStartDateUtc,
                shoppingCartItem.RentalEndDateUtc,
                shoppingCartItem.Quantity,
                false,
                shoppingCartItem.Id).ToList();

            shoppingCartItemDto.Warnings = warnings;

            return shoppingCartItemDto;
        }

        public OrderItemDto PrepareOrderItemDTO(OrderItem orderItem)
        {
            var dto = orderItem.ToDto();

            dto.Product = PrepareProductDTO(orderItem.Product);

            var productAttributeCombination = _productAttributeParser.FindProductAttributeCombination(orderItem.Product, orderItem.AttributesXml);
            if (productAttributeCombination != null)
            {
                dto.ProductAttributeCombinationId = productAttributeCombination.Id;
            }

            return dto;
        }

        public StoreDto PrepareStoreDTO(Store store)
        {
            var storeDto = store.ToDto();

            var primaryCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);

            if (!string.IsNullOrEmpty(primaryCurrency.DisplayLocale))
            {
                storeDto.PrimaryCurrencyDisplayLocale = primaryCurrency.DisplayLocale;
            }

            storeDto.LanguageIds = _languageService.GetAllLanguages(false, store.Id).Select(x => x.Id).ToList();

            return storeDto;
        }

        public LanguageDto PrepateLanguageDto(Language language)
        {
            var languageDto = language.ToDto();

            languageDto.StoreIds = _storeMappingService.GetStoreMappings(language).Select(mapping => mapping.StoreId)
                .ToList();

            if (languageDto.StoreIds.Count == 0)
            {
                languageDto.StoreIds = _storeService.GetAllStores().Select(s => s.Id).ToList();
            }

            return languageDto;
        }

        public ProductAttributeDto PrepareProductAttributeDTO(ProductAttribute productAttribute)
        {
            return productAttribute.ToDto();
        }

        private void PrepareProductImages(IEnumerable<ProductPicture> productPictures, ProductDto productDto)
        {
            if (productDto.Images == null)
            {
                productDto.Images = new List<ImageMappingDto>();
            }

            // Here we prepare the resulted dto image.
            foreach (var productPicture in productPictures)
            {
                var imageDto = PrepareImageDto(productPicture.Picture);

                if (imageDto != null)
                {
                    var productImageDto = new ImageMappingDto
                    {
                        Id = productPicture.Id,
                        PictureId = productPicture.PictureId,
                        Position = productPicture.DisplayOrder,
                        Src = imageDto.Src,
                        Attachment = imageDto.Attachment
                    };

                    productDto.Images.Add(productImageDto);
                }
            }
        }

        protected ImageDto PrepareImageDto(Picture picture)
        {
            ImageDto image = null;

            if (picture != null)
            {
                // We don't use the image from the passed dto directly 
                // because the picture may be passed with src and the result should only include the base64 format.
                image = new ImageDto
                {
                    //Attachment = Convert.ToBase64String(picture.PictureBinary),
                    Src = _pictureService.GetPictureUrl(picture),
                    EntityPictureType = picture.EntityPictureType
                };
            }

            return image;
        }

        private void PrepareProductAttributes(IEnumerable<ProductAttributeMapping> productAttributeMappings,
            ProductDto productDto)
        {
            if (productDto.ProductAttributeMappings == null)
            {
                productDto.ProductAttributeMappings = new List<ProductAttributeMappingDto>();
            }

            foreach (var productAttributeMapping in productAttributeMappings)
            {
                var productAttributeMappingDto =
                    PrepareProductAttributeMappingDto(productAttributeMapping);

                if (productAttributeMappingDto != null)
                {
                    productDto.ProductAttributeMappings.Add(productAttributeMappingDto);
                }
            }
        }

        private ProductAttributeMappingDto PrepareProductAttributeMappingDto(
            ProductAttributeMapping productAttributeMapping)
        {
            ProductAttributeMappingDto productAttributeMappingDto = null;

            if (productAttributeMapping != null)
            {
                productAttributeMappingDto = new ProductAttributeMappingDto
                {
                    Id = productAttributeMapping.Id,
                    ProductAttributeId = productAttributeMapping.ProductAttributeId,
                    ProductAttributeName = _productAttributeService
                        .GetProductAttributeById(productAttributeMapping.ProductAttributeId).Name,
                    TextPrompt = productAttributeMapping.TextPrompt,
                    DefaultValue = productAttributeMapping.DefaultValue,
                    AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId,
                    DisplayOrder = productAttributeMapping.DisplayOrder,
                    IsRequired = productAttributeMapping.IsRequired,
                    ProductAttributeValues = productAttributeMapping.ProductAttributeValues
                        .Select(x => PrepareProductAttributeValueDto(x, productAttributeMapping.Product)).ToList()
                };
            }

            return productAttributeMappingDto;
        }

        private ProductAttributeValueDto PrepareProductAttributeValueDto(ProductAttributeValue productAttributeValue,
            Product product)
        {
            ProductAttributeValueDto productAttributeValueDto = null;

            if (productAttributeValue != null)
            {
                productAttributeValueDto = productAttributeValue.ToDto();
                if (productAttributeValue.ImageSquaresPictureId > 0)
                {
                    var imageSquaresPicture =
                        _pictureService.GetPictureById(productAttributeValue.ImageSquaresPictureId);
                    var imageDto = PrepareImageDto(imageSquaresPicture);
                    productAttributeValueDto.ImageSquaresImage = imageDto;
                }

                if (productAttributeValue.PictureId > 0)
                {
                    // make sure that the picture is mapped to the product
                    // This is needed since if you delete the product picture mapping from the nopCommerce administrationthe
                    // then the attribute value is not updated and it will point to a picture that has been deleted
                    var productPicture =
                        product.ProductPictures.FirstOrDefault(pp => pp.PictureId == productAttributeValue.PictureId);
                    if (productPicture != null)
                    {
                        productAttributeValueDto.ProductPictureId = productPicture.Id;
                    }
                }
            }

            return productAttributeValueDto;
        }
       
        private void PrepareProductAttributeCombinations(IEnumerable<ProductAttributeCombination> productAttributeCombinations,
            ProductDto productDto)
        {
            productDto.ProductAttributeCombinations = productDto.ProductAttributeCombinations ?? new List<ProductAttributeCombinationDto>();

            foreach (var productAttributeCombination in productAttributeCombinations)
            {
                var productAttributeCombinationDto = PrepareProductAttributeCombinationDto(productAttributeCombination);
                if (productAttributeCombinationDto != null)
                {
                    productDto.ProductAttributeCombinations.Add(productAttributeCombinationDto);
                }
            }
        }

        private ProductAttributeCombinationDto PrepareProductAttributeCombinationDto(ProductAttributeCombination productAttributeCombination)
        {
            return productAttributeCombination.ToDto();
        }

        public void PrepareProductSpecificationAttributes(IEnumerable<ProductSpecificationAttribute> productSpecificationAttributes, ProductDto productDto)
        {
            if (productDto.ProductSpecificationAttributes == null)
                productDto.ProductSpecificationAttributes = new List<ProductSpecificationAttributeDto>();

            foreach (var productSpecificationAttribute in productSpecificationAttributes)
            {
                ProductSpecificationAttributeDto productSpecificationAttributeDto = PrepareProductSpecificationAttributeDto(productSpecificationAttribute);

                if (productSpecificationAttributeDto != null)
                {
                    productDto.ProductSpecificationAttributes.Add(productSpecificationAttributeDto);
                }
            }
        }

        public ProductSpecificationAttributeDto PrepareProductSpecificationAttributeDto(ProductSpecificationAttribute productSpecificationAttribute)
        {
            return productSpecificationAttribute.ToDto();
        }

        public SpecificationAttributeDto PrepareSpecificationAttributeDto(SpecificationAttribute specificationAttribute)
        {
            return specificationAttribute.ToDto();
        }
        
        public ManufacturerDto PrepareManufacturerDto(Manufacturer manufacturer)
        {
            var manufacturerDto = manufacturer.ToDto();

            var picture = _pictureService.GetPictureById(manufacturer.PictureId);
            var imageDto = PrepareImageDto(picture);

            if (imageDto != null)
            {
                manufacturerDto.Image = imageDto;
            }

            manufacturerDto.SeName = _urlRecordService.GetSeName(manufacturer);
            manufacturerDto.DiscountIds = manufacturer.AppliedDiscounts.Select(discount => discount.Id).ToList();
            manufacturerDto.RoleIds = _aclService.GetAclRecords(manufacturer).Select(acl => acl.CustomerRoleId).ToList();
            manufacturerDto.StoreIds = _storeMappingService.GetStoreMappings(manufacturer).Select(mapping => mapping.StoreId)
                .ToList();

            var allLanguages = _languageService.GetAllLanguages();

            manufacturerDto.LocalizedNames = new List<LocalizedNameDto>();

            foreach (var language in allLanguages)
            {
                var localizedNameDto = new LocalizedNameDto
                {
                    LanguageId = language.Id,
                    LocalizedName = _localizationService.GetLocalized(manufacturer, x => x.Name, language.Id)
                };

                manufacturerDto.LocalizedNames.Add(localizedNameDto);
            }

            return manufacturerDto;
        }
    }
}