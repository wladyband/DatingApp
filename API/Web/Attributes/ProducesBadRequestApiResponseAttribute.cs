using API.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class ProducesBadRequestApiResponseAttribute : ProducesResponseTypeAttribute
{
    public ProducesBadRequestApiResponseAttribute()
        : base(typeof(ApiResponse), StatusCodes.Status400BadRequest)
    {
    }
}