using API.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class Status201CreatedAttribute : ProducesResponseTypeAttribute
{
    public Status201CreatedAttribute()
        : base(StatusCodes.Status201Created)
    {
    }

    public Status201CreatedAttribute(Type responseType)
        : base(typeof(ApiResponse<>).MakeGenericType(responseType), StatusCodes.Status201Created)
    {
    }
}