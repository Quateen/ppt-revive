using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PPTRevive.Application.Auth.Command.ChangeEmail;
using PPTRevive.Application.Auth.Command.ChangePassword;
using PPTRevive.Application.Auth.Command.CompleteForgetPassword;
using PPTRevive.Application.Auth.Command.ForgetPassword;
using PPTRevive.Application.Auth.Command.GoogleLogin;
using PPTRevive.Application.Auth.Command.Login;
using PPTRevive.Application.Auth.Command.RefreshToken;
using PPTRevive.Application.Auth.Command.Register;
using PPTRevive.Application.Auth.Command.SendOtp;
using PPTRevive.Application.Auth.Command.VerifyEmail;

namespace PPTRevive.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("auth")]
public class AuthController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var result = await sender.Send(command);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command)
    {
        var response = await sender.Send(command);
        return !response.Status ? BadRequest(response) : Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IResult> Login(LoginCommand query)
    {
        var result = await sender.Send(query);
        return result.Status ? Results.Ok(result) : Results.BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("google-login")]
    public async Task<IResult> GoogleLogin(GoogleLoginCommand query)
    {
        var result = await sender.Send(query);
        return result.Status ? Results.Ok(result) : Results.BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("forget-password")]
    public async Task<IResult> ForgetPassword(ForgetPasswordCommand query)
    {
        var result = await sender.Send(query);
        return result.Status ? Results.Ok(result) : Results.BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IResult> CompletePasswordReset(CompleteForgetPasswordCommand query)
    {
        var result = await sender.Send(query);
        return result.Status ? Results.Ok(result) : Results.BadRequest(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IResult> ChangePassword(ChangePasswordCommand query)
    {
        var result = await sender.Send(query);
        return result.Status ? Results.Ok(result) : Results.BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpCommand command)
    {
        var response = await sender.Send(command);
        return !response.Status ? BadRequest(response) : Ok(response);
    }

    //[AllowAnonymous]
    //[HttpPost("complete-registration")]
    //public async Task<IResult> CompleteRegistration(CompleteRegistrationCommand query)
    //{
    //    var result = await sender.Send(query);
    //    return result.Status ? Results.Ok(result) : Results.BadRequest(result);
    //}

    [AllowAnonymous]
    [HttpGet("refresh-token")]
    public async Task<IActionResult> RefreshTokenGet()
    {
        var query = new RefreshTokenCommand();
        var result = await sender.Send(query);

        return result.Status ? Ok(result) : BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeEmail([FromBody] EmailChangeCommand command)
    {
        var response = await sender.Send(command);
        return !response.Status ? BadRequest(response) : Ok(response);
    }
}
