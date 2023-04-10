# Jwt Authentication

When login is successful, both an `access_token` and `refresh_token` will be issued.

`access_token` = short lived token that can expire. On expiry you can request a new one using the `refresh_token`. Can be stored as a global variable in UI.
`refresh_token` = long lived token that can be used to generate `access_token`. To be stored in a http-only cookie. Should also be Secure=true, SameSite=Strict.

Refresh tokens may be stored in the database for tracking token history and can be revoked. When an `access_token`'s lifetime expires, the client should 
request a new one using the `refresh_token` located in the http-only cookie *as long as the refresh_token is still valid.*

Token validation is defined in the `JwtService` and referenced by `JwtAuthenticationExtensions`. Everything else is vanilla asp.net identity which reduces
the size of the implementation.

## Jwt Examples that helped me design this

* [Jwt auth .net 6](https://www.c-sharpcorner.com/article/jwt-token-authentication-and-authorizations-in-net-core-6-0-web-api/)
* [Jwt auth with refresh tokens in .net core 3](https://jasonwatmore.com/post/2020/05/25/aspnet-core-3-api-jwt-authentication-with-refresh-tokens)
* [Jwt auth with refresh tokens in .net 6](https://jasonwatmore.com/post/2022/01/24/net-6-jwt-authentication-with-refresh-tokens-tutorial-with-example-api)
* [Password Hashing in .net 6](https://github.com/dotnet/AspNetCore/blob/main/src/Identity/Extensions.Core/src/PasswordHasher.cs)
