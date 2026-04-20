using API.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class ProducesNotFoundApiResponseAttribute : ProducesResponseTypeAttribute
{
    public ProducesNotFoundApiResponseAttribute()
        : base(typeof(ApiResponse), StatusCodes.Status404NotFound)
    {
    }
}