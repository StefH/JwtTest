﻿using JwtTest;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace WebApplicationTest.Filters
{
    public class MyAuthFilter : IAuthenticationFilter
    {
        public bool AllowMultiple { get; } = true;

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            // 1. Look for credentials in the request.
            HttpRequestMessage request = context.Request;
            AuthenticationHeaderValue authorization = request.Headers.Authorization;

            // 2. If there are no credentials, do nothing.
            if (authorization == null)
            {
                return;
            }

            // 3. If there are credentials but the filter does not recognize the authentication scheme, do nothing.
            if (authorization.Scheme != "Bearer")
            {
                return;
            }

            string authorizationToken = authorization.Parameter;

            // 4. If the credentials are missing, set the error result.
            if (string.IsNullOrEmpty(authorizationToken))
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                return;
            }

            // 5. If there are credentials that the filter understands, try to validate them.
            byte[] secretBytes = Encoding.UTF8.GetBytes("stef");
            try
            {
                var payload = Jose.JWT.Decode<PayloadTest>(authorizationToken, secretBytes);

                if (DateTime.UtcNow > DateTimeUtils.UnixTimeStampToDateTime(payload.Expires))
                {
                    throw new Exception();
                }
            }
            catch
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request);
                return;
            }

            var i = new GenericIdentity("user");
            IPrincipal principal = new GenericPrincipal(i, new string[] { });
            //if (principal == null)
            //{
            //    context.ErrorResult = new AuthenticationFailureResult("Invalid username or password", request);
            //}

            // 6. If the credentials are valid, set principal.
            //else
            {
                context.Principal = principal;
            }
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            var challenge = new AuthenticationHeaderValue("Bearer");
            context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);
            return Task.FromResult(0);
        }
    }
}