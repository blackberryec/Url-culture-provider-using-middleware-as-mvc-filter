using Microsoft.AspNetCore.Routing.Constraints;

namespace WebApplication1
{
    public class CultureRouteConstraint : RegexRouteConstraint
    {
        public CultureRouteConstraint()
        : base(@"^[a-zA-Z]{2}(\-[a-zA-Z]{2})?$") { }
    }
}
