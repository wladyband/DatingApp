using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class ProducesCreatedApiResponseAttribute : ProducesResponseTypeAttribute
{
    public ProducesCreatedApiResponseAttribute(Type responseType)
        : base(typeof(ApiResponse<>).MakeGenericType(responseType), StatusCodes.Status201Created)
    {
    }
}