using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class ProducesUnauthorizedApiResponseAttribute : ProducesResponseTypeAttribute
{
    public ProducesUnauthorizedApiResponseAttribute()
        : base(typeof(ApiResponse), StatusCodes.Status401Unauthorized)
    {
    }
}