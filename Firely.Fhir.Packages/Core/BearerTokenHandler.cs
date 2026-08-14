/*
 * Copyright (c) 2026, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 *
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// A <see cref="DelegatingHandler"/> that authenticates each outgoing request with a Bearer token
    /// obtained from a caller-supplied token provider. The provider is invoked for every request, so it
    /// can supply a refreshed token when a previous one has expired.
    /// </summary>
    internal class BearerTokenHandler : DelegatingHandler
    {
        private readonly Func<CancellationToken, Task<string>> _tokenProvider;

        public BearerTokenHandler(Func<CancellationToken, Task<string>> tokenProvider, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _tokenProvider(cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}

#nullable restore
