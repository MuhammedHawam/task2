using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PIF.EBP.WebAPI.Controllers.Requests.Account
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}