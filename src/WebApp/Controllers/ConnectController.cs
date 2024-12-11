using System.Collections.Specialized;
using System.Diagnostics;
using Application.Authorization;
using Core.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers;

public class ConnectController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<HomeController> _logger;

    public ConnectController(IAuthorizationService authorizationService, ILogger<HomeController> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    [HttpGet]
    //public async Task<IActionResult> Authorize([FromForm] NameValueCollection parameters)
    public async Task<IActionResult> Authorize(string client_id, string redirect_uri, string response_type, string scope, string state)
    {
        var authorizationRequest = new AuthorizationRequest
        {
            ClientId = client_id,
            RedirectUri = redirect_uri,
            ResponseType = response_type,
            Scope = scope,
            State = state
        };

        AuthorizationResult result = await _authorizationService.ValidateRequest(authorizationRequest);

        if (!result.IsValid)
        {
            // send user to error page
            return View("Error", "client_id is empty");
        }


        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
