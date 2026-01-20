using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace My_Portfolyo.Attributes
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var isAdmin = session.GetString("IsAdmin") == "true";

            if (!isAdmin)
            {
                var lang = context.RouteData.Values["lang"]?.ToString() ?? "tr";
                context.Result = new RedirectResult($"/{lang}/Admin/Login");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
