﻿using Eventure.Models.RequestDto;
using Eventure.Models.ResponseDto;
using Eventure.Models.Results;
using Eventure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Eventure.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthEndpointsController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthEndpointsController(IAuthService authService, IHttpContextAccessor httpContextAccessor)
    {
        _authService = authService;
        _httpContextAccessor = httpContextAccessor;
    }

    //Login Endpoint
    [HttpPost]
    [Route("/api/login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(LoginResult.Fail("Invalid inputs"));
            }

            var authResult = await _authService.LoginAsync(loginUserDto);

            if (authResult.Succeeded)
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Append("token", authResult.Token, new CookieOptions()
                {
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.Now.AddDays(14),
                    IsEssential = true,
                    Secure = true,
                    HttpOnly = true
                });

                var roles = await _authService.GetRolesAsync(loginUserDto.UserName!);

                return Ok(new LoginResponseDto
                {
                    Roles = roles,
                    UserName = loginUserDto.UserName
                });
            }

            return Unauthorized(new LoginResult { Succeeded = false, ErrorMessage = "Wrong username or password." });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500,
                new LoginResult { Succeeded = false, ErrorMessage = "An error occured on the server." });
        }
    }


    //Logout Endpoint
    [HttpPost]
    [Route("/api/logout")]
    public async Task<IActionResult> Logout()
    {
        return Ok();
    }


    //Registration
    [HttpPost]
    [Route("/api/signup")]
    public async Task<IActionResult> Signup(RegisterUserDto registerUserDto)
    {
        var registrationResult = await _authService.RegisterUser(registerUserDto);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(e => e.Errors.Select(err => err.ErrorMessage));
            return BadRequest(new { Errors = errors });
        }
        
        return Ok(new
        {
            Message = registrationResult.Message,
            User = registerUserDto
        });
        
    }
}