using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class ProducesOkApiResponseAttribute : ProducesResponseTypeAttribute
{
    public ProducesOkApiResponseAttribute(Type responseType)
        : base(typeof(ApiResponse<>).MakeGenericType(responseType), StatusCodes.Status200OK)
    {
    }
}