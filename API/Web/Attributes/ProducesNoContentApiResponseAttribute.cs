using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class ProducesNoContentApiResponseAttribute : ProducesResponseTypeAttribute
{
    public ProducesNoContentApiResponseAttribute()
        : base(StatusCodes.Status204NoContent)
    {
    }
}