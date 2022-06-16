using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OblivionAPI; 

[AttributeUsage(AttributeTargets.All)]
public class RestrictHostAttribute : Attribute, IAuthorizationFilter {
    public void OnAuthorization(AuthorizationFilterContext context) {
        var allowedHostsDetails = Environment.GetEnvironmentVariable("API_ALLOWED_HOSTS");
        
        if (string.IsNullOrEmpty(allowedHostsDetails)) {
            context.Result = new BadRequestResult();
            return;
        }

        var allowedHosts = allowedHostsDetails.Split(',').ToList();
        
        var host = context.HttpContext.Request.Host.Host.ToLower();
        if (!allowedHosts.Contains(host)) context.Result = new BadRequestObjectResult(host);
    }
}
