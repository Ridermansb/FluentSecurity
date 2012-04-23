using System;
using FluentSecurity.Caching;
using FluentSecurity.Policy.ViolationHandlers.Conventions;

namespace FluentSecurity.Configuration
{
	public class AdvancedConfiguration : IAdvancedConfiguration
	{
		internal AdvancedConfiguration()
		{
			Conventions = new Conventions
			{
				new FindByPolicyNameConvention(),
				new FindDefaultPolicyViolationHandlerByNameConvention()
			};

			SetDefaultResultsCacheLifecycle(Cache.DoNotCache);
		}

		public Conventions Conventions { get; private set; }
		public Cache DefaultResultsCacheLifecycle { get; private set; }
		public Action<ISecurityContext> SecurityContextModifyer { get; private set; }

		public void SetDefaultResultsCacheLifecycle(Cache lifecycle)
		{
			DefaultResultsCacheLifecycle = lifecycle;
		}

		public void ModifySecurityContext(Action<ISecurityContext> modifyer)
		{
			SecurityContextModifyer = modifyer;
		}

		public void Violations(Action<ViolationsExpression> violationsExpression)
		{
			if (violationsExpression == null) throw new ArgumentNullException("violationsExpression");
			violationsExpression.Invoke(new ViolationsExpression(Conventions));
		}
	}
}