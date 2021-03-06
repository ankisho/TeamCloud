﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Azure.Resources;
using Xunit;

namespace TeamCloud.Azure.Tests.Resources
{
    public class AzureResourceIdentifierTests
    {
        [Theory]
        [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/TestRG")]
        [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/TestRG/")]
        public void ParseResourceGroupId(string resourceId)
        {
            var resourceIdentifier = AzureResourceIdentifier.Parse(resourceId);

            Assert.Equal(Guid.Empty, resourceIdentifier.SubscriptionId);
            Assert.Equal("TestRG", resourceIdentifier.ResourceGroup);
        }

        [Theory]
        [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType/TestResourceName")]
        [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType/TestResourceName/")]
        [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType/TestResourceName/SubResourceType/SubResourceName")]
        [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType/TestResourceName/SubResourceType/SubResourceName/")]
        public void ParseResourceId(string resourceId)
        {
            var resourceIdentifier = AzureResourceIdentifier.Parse(resourceId);

            Assert.Equal(Guid.Empty, resourceIdentifier.SubscriptionId);
            Assert.Equal("TestRG", resourceIdentifier.ResourceGroup);

            Assert.Equal("resourceProviders", resourceIdentifier.ResourceTypes[0].Key);
            Assert.Equal("TestProviderName", resourceIdentifier.ResourceTypes[0].Value);

            Assert.Equal("TestResourceType", resourceIdentifier.ResourceTypes[1].Key);
            Assert.Equal("TestResourceName", resourceIdentifier.ResourceTypes[1].Value);

            if (resourceIdentifier.ResourceTypes.Count == 3)
            {
                Assert.Equal("SubResourceType", resourceIdentifier.ResourceTypes[2].Key);
                Assert.Equal("SubResourceName", resourceIdentifier.ResourceTypes[2].Value);
            }
        }

        [Theory]
        [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType")]
        [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType/")]
        public void ParseResourceIdUnnamed(string resourceId)
        {
            var resourceIdentifier = AzureResourceIdentifier.Parse(resourceId, allowUnnamedResource: true);

            Assert.Equal(Guid.Empty, resourceIdentifier.SubscriptionId);
            Assert.Equal("TestRG", resourceIdentifier.ResourceGroup);
            Assert.True(resourceIdentifier.ResourceTypes.Count == 2);
            Assert.Null(resourceIdentifier.ResourceName);

            Assert.Equal("resourceProviders", resourceIdentifier.ResourceTypes[0].Key);
            Assert.Equal("TestProviderName", resourceIdentifier.ResourceTypes[0].Value);

            Assert.Equal("TestResourceType", resourceIdentifier.ResourceTypes[1].Key);
            Assert.Null(resourceIdentifier.ResourceTypes[1].Value);
        }

        [Theory]
        [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType")]
        [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType/")]
        public void ParseResourceIdUnnamedNotAllowed(string resourceId)
        {
            Assert.Throws<ArgumentException>(() => AzureResourceIdentifier.Parse(resourceId, allowUnnamedResource: false));
        }
    }
}
