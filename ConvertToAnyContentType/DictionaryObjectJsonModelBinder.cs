using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System.Linq;
namespace ConvertToAnyContentType
{
    public class DictionaryObjectJsonModelBinder : IModelBinder
    {
        private readonly ILogger<DictionaryObjectJsonModelBinder> _logger;

        private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            Converters = { new DictionaryStringObjectJsonConverter() }
        };

        public DictionaryObjectJsonModelBinder()
        {
            //  _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            if (bindingContext.ModelType != typeof(Dictionary<string, object>))
            {
                throw new NotSupportedException($"The '{nameof(DictionaryObjectJsonModelBinder)}' model binder should only be used on Dictionary<string, object>, it will not work on '{bindingContext.ModelType.Name}'");
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            try
            {
                if (bindingContext.HttpContext.Request.ContentType.Contains("application/x-www-form-urlencoded"))
                {
                    var form = await bindingContext.HttpContext.Request.ReadFormAsync();
                    foreach (var item in form)
                    {
                        data.Add(item.Key, item.Value);
                    }
                }
                else if (bindingContext.HttpContext.Request.ContentType.Contains("multipart/form-data"))
                {
                    var form = await bindingContext.HttpContext.Request.ReadFormAsync();
                    foreach (var item in form)
                    {
                        data.Add(item.Key, item.Value);
                    }
                }
                else if (bindingContext.HttpContext.Request.ContentType.Contains("application/json"))
                {
                    data = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(bindingContext.HttpContext.Request.Body, DefaultJsonSerializerOptions);
                }
                bindingContext.Result = ModelBindingResult.Success(data);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when trying to model bind Dictionary<string, object>");
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
    }
}