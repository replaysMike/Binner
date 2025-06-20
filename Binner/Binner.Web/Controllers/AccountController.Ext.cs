using Binner.Model;
using Microsoft.AspNetCore.Mvc;

namespace Binner.Web.Controllers
{
    public partial class AccountController<TAccount> : ControllerBase
        where TAccount : Account, new()
    {
         
    }
}
