using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static RestrictIP.Middleware.DomainException;


namespace RestrictIP.Controllers
{

    //[AllowAnonymous]
    //[ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    public class ErrorsController : ControllerBase
    {
        
        [Route("error")]
        public IActionResult Error()
        {

            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var statusCode = exception.Error.GetType().Name switch
            {
                "ArgumentException" => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.ServiceUnavailable
            };

            return Problem(detail: exception.Error.Message, statusCode: (int)statusCode);


        }
    }
}