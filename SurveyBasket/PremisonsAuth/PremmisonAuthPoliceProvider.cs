using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SurveyBasket.PermissionsAuth;

namespace SurveyBasket.PermissionsAuth
{
    public class PermissionAuthorizationPolicyProvider
        : DefaultAuthorizationPolicyProvider
    {
        private readonly AuthorizationOptions _options;

        public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
            : base(options)
        {
            _options = options.Value;
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            var policy = await base.GetPolicyAsync(policyName);

            if (policy is not null)
                return policy;

            var policyBuilder = new AuthorizationPolicyBuilder();
            policyBuilder.AddRequirements(new PermissionRequirement(policyName));

            var newPolicy = policyBuilder.Build();

            _options.AddPolicy(policyName, newPolicy);

            return newPolicy;
        }
    }
}
