using API.Web.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesBadRequestApiResponse]
    public class BaseApiController : ControllerBase
    {
    }
}
