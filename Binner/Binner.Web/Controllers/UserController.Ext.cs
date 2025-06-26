using Binner.Model;
using Microsoft.AspNetCore.Mvc;

namespace Binner.Web.Controllers
{
    public partial class UserController<TUser> : ControllerBase
        where TUser : User, new()
    {
    }
}
