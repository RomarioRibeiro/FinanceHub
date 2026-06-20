using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FinanceHub.ModelBinders
{
    public class DecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

            var value = valueResult.FirstValue;
            if (string.IsNullOrWhiteSpace(value))
            {
                return Task.CompletedTask;
            }

            var normalizedValue = NormalizeDecimal(value);
            if (decimal.TryParse(
                normalizedValue,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var decimalValue))
            {
                bindingContext.Result = ModelBindingResult.Success(decimalValue);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                "Informe um valor monetario valido.");

            return Task.CompletedTask;
        }

        private static string NormalizeDecimal(string value)
        {
            var sanitized = value.Trim().Replace(" ", string.Empty);
            var lastCommaIndex = sanitized.LastIndexOf(',');
            var lastDotIndex = sanitized.LastIndexOf('.');

            if (lastCommaIndex >= 0 && lastDotIndex >= 0)
            {
                var decimalSeparatorIndex = Math.Max(lastCommaIndex, lastDotIndex);
                var decimalSeparator = sanitized[decimalSeparatorIndex];
                var thousandsSeparator = decimalSeparator == ',' ? "." : ",";

                sanitized = sanitized.Replace(thousandsSeparator, string.Empty);
                sanitized = decimalSeparator == ','
                    ? sanitized.Replace(',', '.')
                    : sanitized;

                return sanitized;
            }

            if (lastCommaIndex >= 0)
            {
                return sanitized.Replace('.', ' ').Replace(",", ".").Replace(" ", string.Empty);
            }

            return sanitized.Replace(",", string.Empty);
        }
    }
}
