﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Turbocharged.Vault.Tests
{
    public class VaultClientFacts
    {
        TokenAuthentication _token;
        AppIdAuthentication _appId;
        Uri _uri;

        Dictionary<string, object> EMPTY_SECRET = new Dictionary<string, object>();

        public VaultClientFacts()
        {
            _token = new TokenAuthentication(Configuration.Token);
            _appId = new AppIdAuthentication(Configuration.AppId, Configuration.UserId);
            _uri = Configuration.VaultUri;
        }

        async Task<VaultClient> InitializeWithTokenAsync()
        {
            var vault = new VaultClient(_uri, _token);
            await vault.AuthorizeAsync();
            return vault;
        }

        async Task<VaultClient> InitializeWithAppId()
        {
            var vault = new VaultClient(_uri, _appId);
            await vault.AuthorizeAsync();
            return vault;
        }

        #region Sealing/Unsealing

        [Fact]
        public async Task CanRequestSealStatus()
        {
            var vault = await InitializeWithTokenAsync();
            var response = await vault.SealStatusAsync();
            Assert.NotNull(response);
            Assert.Equal(false, response.Sealed);
        }

        #endregion

        #region Authentication/Authorization

        [Fact]
        public async Task WritingSecretsThrowsAVaultExceptionWhenNotAuthorized()
        {
            var badToken = new TokenAuthentication(Guid.NewGuid().ToString());
            var vault = new VaultClient(_uri, badToken);
            await vault.AuthorizeAsync(); // Not strictly necessary for TokenAuth, but whatever
            await Assert.ThrowsAsync<VaultException>(() => vault.WriteSecretAsync("foo", EMPTY_SECRET));
        }

        [Fact]
        public async Task CanUseTokenAuthentication()
        {
            var vault = await InitializeWithTokenAsync();
            await vault.WriteSecretAsync("foo", new Dictionary<string, object> { { "foo", "bar" } });
        }

        [Fact]
        public async Task CanUseAppIdAuthentication()
        {
            var vault = await InitializeWithAppId();
            await vault.WriteSecretAsync("foo", new Dictionary<string, object> { { "foo", "bar" } });
        }

        #endregion

        #region Secrets

        [Fact]
        public async Task CanWriteAndReadSecrets()
        {
            var path = "secret/foo";
            var expectedValue = Guid.NewGuid().ToString();
            var obj = new Dictionary<string, object>() { { "Value", expectedValue } };
            var vault = await InitializeWithTokenAsync();
            await vault.WriteSecretAsync(path, obj);
            var result = await vault.LeaseAsync(path);
            Assert.NotNull(result);
            Assert.Equal(expectedValue, result.Data["Value"]);
        }

        [Fact]
        public async Task CanDeleteSecrets()
        {
            var path = "secret/foo";
            var expectedValue = Guid.NewGuid().ToString();
            var obj = new Dictionary<string, object>() { { "Value", expectedValue } };
            var vault = await InitializeWithTokenAsync();
            await vault.WriteSecretAsync(path, obj);
            await vault.DeleteSecretAsync(path);
            var result = await vault.LeaseAsync(path);
            Assert.Null(result);
        }

        #endregion
    }
}
