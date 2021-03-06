using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ForsakenBorders.Backend.Filters
{
    /// <summary>
    /// Path Parameter Validation Rules Filter
    /// </summary>
    public class GeneratePathParamsValidationFilter : IOperationFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="context">OperationFilterContext</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            IList<ApiParameterDescription> pars = context.ApiDescription.ParameterDescriptions;

            foreach (ApiParameterDescription par in pars)
            {
                OpenApiParameter swaggerParam = operation.Parameters.SingleOrDefault(p => p.Name == par.Name);

                IEnumerable<CustomAttributeData> attributes = ((ControllerParameterDescriptor)par.ParameterDescriptor).ParameterInfo.CustomAttributes;

                if (attributes != null && attributes.Any() && swaggerParam != null)
                {
                    // Required - [Required]
                    CustomAttributeData requiredAttr = attributes.FirstOrDefault(p => p.AttributeType == typeof(RequiredAttribute));
                    if (requiredAttr != null)
                    {
                        swaggerParam.Required = true;
                    }

                    // Regex Pattern [RegularExpression]
                    CustomAttributeData regexAttr = attributes.FirstOrDefault(p => p.AttributeType == typeof(RegularExpressionAttribute));
                    if (regexAttr != null)
                    {
                        string regex = (string)regexAttr.ConstructorArguments[0].Value;
                        if (swaggerParam is OpenApiParameter parameter)
                        {
                            parameter.Schema.Pattern = regex;
                        }
                    }

                    // String Length [StringLength]
                    int? minLength = null, maxLength = null;
                    CustomAttributeData stringLengthAttr = attributes.FirstOrDefault(p => p.AttributeType == typeof(StringLengthAttribute));
                    if (stringLengthAttr != null)
                    {
                        if (stringLengthAttr.NamedArguments.Count == 1)
                        {
                            minLength = (int)stringLengthAttr.NamedArguments.Single(p => p.MemberName == "MinimumLength").TypedValue.Value;
                        }
                        maxLength = (int)stringLengthAttr.ConstructorArguments[0].Value;
                    }

                    CustomAttributeData minLengthAttr = attributes.FirstOrDefault(p => p.AttributeType == typeof(MinLengthAttribute));
                    if (minLengthAttr != null)
                    {
                        minLength = (int)minLengthAttr.ConstructorArguments[0].Value;
                    }

                    CustomAttributeData maxLengthAttr = attributes.FirstOrDefault(p => p.AttributeType == typeof(MaxLengthAttribute));
                    if (maxLengthAttr != null)
                    {
                        maxLength = (int)maxLengthAttr.ConstructorArguments[0].Value;
                    }

                    if (swaggerParam is OpenApiParameter parameter1)
                    {
                        parameter1.Schema.MinLength = minLength;
                        parameter1.Schema.MaxLength = maxLength;
                    }

                    // Range [Range]
                    CustomAttributeData rangeAttr = attributes.FirstOrDefault(p => p.AttributeType == typeof(RangeAttribute));
                    if (rangeAttr != null)
                    {
                        int rangeMin = (int)rangeAttr.ConstructorArguments[0].Value;
                        int rangeMax = (int)rangeAttr.ConstructorArguments[1].Value;

                        if (swaggerParam is OpenApiParameter parameter2)
                        {
                            parameter2.Schema.Minimum = rangeMin;
                            parameter2.Schema.Maximum = rangeMax;
                        }
                    }
                }
            }
        }
    }
}
