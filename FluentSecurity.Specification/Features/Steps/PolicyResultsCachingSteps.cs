﻿using System;
using System.Linq;
using FluentSecurity.Caching;
using FluentSecurity.Specification.Features.Helpers;
using FluentSecurity.Specification.TestData;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace FluentSecurity.Specification.Features.Steps
{
	[Binding]
	public class PolicyResultsCachingSteps
	{
		public enum PolicyType
		{
			WriterPolicy
		}

		public static readonly WriterPolicy WriterPolicy = new WriterPolicy();

		[Given(@"the cache strategy of all controllers is set to (.*) by (.*) for (.*)")]
		public void GivenTheCacheStrategyOfAllControllersIsSetToXByXForX(Cache lifecycle, By level, PolicyType policy)
		{
			Given(configuration => configuration.ForAllControllers().CacheResultsOf<WriterPolicy>(lifecycle, level));
		}

		[Given(@"the cache strategy of all controllers is set to (.*) for (.*)")]
		public void GivenTheCacheStrategyOfAllControllersIsSetToXForX(Cache lifecycle, PolicyType policy)
		{
			Given(configuration => configuration.ForAllControllers().CacheResultsOf<WriterPolicy>(lifecycle));
		}

		[Given(@"the cache strategy of BlogController is set to (.*) by (.*) for (.*)")]
		public void GivenTheCacheStrategyOfBlogControllerIsSetToXByXForX(Cache lifecycle, By level, PolicyType policy)
		{
			Given(configuration => configuration.For<BlogController>().CacheResultsOf<WriterPolicy>(lifecycle, level));
		}

		[Given(@"the cache strategy of BlogController is set to (.*) for (.*)")]
		public void GivenTheCacheStrategyOfBlogControllerIsSetToXForX(Cache lifecycle, PolicyType policy)
		{
			Given(configuration => configuration.For<BlogController>().CacheResultsOf<WriterPolicy>(lifecycle));
		}

		[Given(@"the cache strategy of BlogController AddPost is set to (.*) by (.*) for (.*)")]
		public void GivenTheCacheStrategyOfBlogControllerAddPostIsSetToXByXForX(Cache lifecycle, By level, PolicyType policy)
		{
			Given(configuration => configuration.For<BlogController>(x => x.AddPost()).CacheResultsOf<WriterPolicy>(lifecycle, level));
		}

		[Given(@"the cache strategy of BlogController AddPost is set to (.*) for (.*)")]
		public void GivenTheCacheStrategyOfBlogControllerAddPostIsSetToXForX(Cache lifecycle, PolicyType policy)
		{
			Given(configuration => configuration.For<BlogController>(x => x.AddPost()).CacheResultsOf<WriterPolicy>(lifecycle));
		}

		[When(@"enforcing (.*) for (.*) (.*)")]
		public void WhenEnforcingPolicyXForControllerXActionX(PolicyType policy, string controller, string action)
		{
			EnsureConfigured();

			var controllerName = typeof (BlogController).Namespace + "." + controller;
			var container = SecurityConfiguration.Current.PolicyContainers.GetContainerFor(controllerName, action);
			var policyType = GetPolicyType(policy);

			ScenarioContext.Current.Set(policyType);
			ScenarioContext.Current.Set(container);

			container.EnforcePolicies(SecurityContext.Current);
		}

		[Then(@"it should cache result (.*)")]
		public void ThenItShouldCacheResultX(Cache expectedLifecycle)
		{
			var policyType = ScenarioContext.Current.Get<Type>();
			var manifest = GetPolicyContainer().CacheManifests.SingleOrDefault(x => x.CacheLifecycle == expectedLifecycle && x.PolicyType == policyType);

			Assert.That(manifest, Is.Not.Null);
		}

		[Then(@"it should not cache result")]
		public void ThenItShouldNotCacheResult()
		{
			var policyType = ScenarioContext.Current.Get<Type>();
			var manifest = GetPolicyContainer().CacheManifests.SingleOrDefault(x => x.CacheLifecycle == Cache.DoNotCache && x.PolicyType == policyType);

			Assert.That(manifest, Is.Not.Null);
		}

		[Then(@"it should cache result with key ""(.*)_(.*)_(.*)""")]
		public void ThenItShouldCacheResultWithKeyX(string controller, string action, string policy)
		{
			var policyType = ScenarioContext.Current.Get<Type>();
			var manifest = GetPolicyContainer().CacheManifests.SingleOrDefault(x => x.PolicyType == policyType);
			var cacheKey = PolicyResultCacheKeyBuilder.CreateFromManifest(manifest, WriterPolicy, SecurityContext.Current);

			VerifyCacheKey(cacheKey, controller, action, policy);
		}

		private void Given(Action<ConfigurationExpression> expression)
		{
			ScenarioContext.Current.Givens<ConfigurationExpression>().Add(expression);
		}

		private void EnsureConfigured()
		{
			Action<ConfigurationExpression> expression = e =>
			{
				e.GetAuthenticationStatusFrom(() => true);

				e.ForAllControllers().AddPolicy(WriterPolicy);

				foreach (var given in ScenarioContext.Current.Givens<ConfigurationExpression>())
					given.Invoke(e);
			};
			SecurityConfigurator.Configure(expression);
		}

		private Type GetPolicyType(PolicyType policy)
		{
			switch (policy)
			{
				case PolicyType.WriterPolicy:
					return WriterPolicy.GetType();
				default:
					throw new ArgumentOutOfRangeException("policy");
			}
		}

		private static PolicyContainer GetPolicyContainer()
		{
			return (PolicyContainer)ScenarioContext.Current.Get<IPolicyContainer>();
		}

		private void VerifyCacheKey(string cacheKey, string controller, string action, string policy)
		{
			if (!controller.StartsWith("*")) controller = "FluentSecurity.Specification.TestData." + controller;
			if (!policy.StartsWith("*")) policy = "FluentSecurity.Specification.TestData." + policy;

			var fullExpectedCacheKey = String.Concat("PolicyResult_", controller, "_", action, "_", policy);
			Assert.That(cacheKey, Is.EqualTo(fullExpectedCacheKey));
		}
	}
}