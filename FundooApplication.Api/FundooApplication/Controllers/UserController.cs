using Microsoft.AspNetCore.Mvc;
using FundooManager.IManager;
using FundooModel.User;
using System.Threading.Tasks;
using System;

namespace FundooApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly IUserManager userManager;
        public UserController(IUserManager userManager)
        {
            this.userManager = userManager;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult> UserRegister(Register register)
        {
            try
            {
                var result = await this.userManager.RegisterUser(register);
                if (result == 1)
                {
                    return this.Ok(new { Status = true, Message = "User Registration Successful", data = register });
                }
                return this.BadRequest(new { Status = false, Message = "User Registration UnSuccessful" });
            }
            catch (Exception ex)
            {
                return this.NotFound(new { Status = false, Message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Login")]
        public  ActionResult UserLogin(Login login)
        {
            try
            {
                var result =  this.userManager.LoginUser(login);
                if (result != null)
                {
                    return this.Ok(new { Status = true, Message = "User Login Successful", data = login });
                }
                return this.BadRequest(new { Status = false, Message = "User Login UnSuccessful" });
            }
            catch (Exception ex)
            {
                return this.NotFound(new { Status = false, Message = ex.Message });
            }
        }

        [HttpPut]
        [Route("ResetPassword")]
        public ActionResult UserResetPassword(ResetPassword resetPassword)
        {
            try
            {
                var result = this.userManager.ResetPassword(resetPassword);
                if (result != null)
                {
                    return this.Ok(new { Status = true, Message = "User password reset Successful", data = resetPassword });
                }
                return this.BadRequest(new { Status = false, Message = "User  password reset UnSuccessful" });
            }
            catch (Exception ex)
            {
                return this.NotFound(new { Status = false, Message = ex.Message });
            }
        }
    }
}
