using Nop.Plugin.Api.ModelBinders;

namespace Nop.Plugin.Api.Models.ShoppingCartsParameters
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;

    [ModelBinder(typeof(ParametersModelBinder<ShoppingCartsParametersModel>))]
    public class ShoppingCartsParametersModel
    {

        #region Constructors

        public ShoppingCartsParametersModel()
        {
            CustomerIds = new List<int>();
        }

        #endregion

        public List<int> CustomerIds { get; set; }
        public List<int> ShoppingCartTypeIds { get; set; }
    }
}