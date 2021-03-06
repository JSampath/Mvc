﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    public class JavaScriptResourcesTest
    {
        [Fact]
        public void GetEmbeddedJavaScript_LoadsEmbeddedResourceFromManifestStream()
        {
            // Arrange
            var resource = "window.alert('An alert');";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(resource));
            var getManifestResourceStream = new Func<string, Stream>(name => stream);
            var cache = new ConcurrentDictionary<string, string>();

            // Act
            var result = JavaScriptResources.GetEmbeddedJavaScript("test.js", getManifestResourceStream, cache);

            // Assert
            Assert.Equal(resource, result);
        }

        [Fact]
        public void GetEmbeddedJavaScript_AddsResourceToCacheWhenRead()
        {
            // Arrange
            var resource = "window.alert('An alert');";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(resource));
            var getManifestResourceStream = new Func<string, Stream>(name => stream);
            var cache = new ConcurrentDictionary<string, string>();

            // Act
            var result = JavaScriptResources.GetEmbeddedJavaScript("test.js", getManifestResourceStream, cache);

            // Assert
            Assert.Collection(cache, kvp =>
            {
                Assert.Equal("test.js", kvp.Key);
                Assert.Equal(resource, kvp.Value);
            });
        }

        [Fact]
        public void GetEmbeddedJavaScript_LoadsResourceFromCacheAfterInitialCall()
        {
            // Arrange
            var resource = "window.alert('An alert');";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(resource));
            var callCount = 0;
            var getManifestResourceStream = new Func<string, Stream>(name =>
            {
                callCount++;
                return stream;
            });
            var cache = new ConcurrentDictionary<string, string>();

            // Act
            var result = JavaScriptResources.GetEmbeddedJavaScript("test.js", getManifestResourceStream, cache);
            result = JavaScriptResources.GetEmbeddedJavaScript("test.js", getManifestResourceStream, cache);

            // Assert
            Assert.Equal(1, callCount);
        }

        [Theory]
        [InlineData("window.alert(\"[[[0]]]\")", "window.alert(\"{0}\")")]
        [InlineData("var test = { a: 1 };", "var test = {{ a: 1 }};")]
        [InlineData("var test = { a: 1, b: \"[[[0]]]\" };", "var test = {{ a: 1, b: \"{0}\" }};")]
        public void GetEmbeddedJavaScript_PreparesJavaScriptCorrectly(string resource, string expectedResult)
        {
            // Arrange
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(resource));
            var getManifestResourceStream = new Func<string, Stream>(name => stream);
            var cache = new ConcurrentDictionary<string, string>();

            // Act
            var result = JavaScriptResources.GetEmbeddedJavaScript("test.js", getManifestResourceStream, cache);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}