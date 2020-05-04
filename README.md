# PoC-Identity

Proof of Concept for .Net Core Identity.
The MVC controllers, Api controllers, Identity razor pages and JavaScript client are all combined in a single project.
Features:
- Identity
- Cookies authentication
- Jwt authentication for APIs (with custom refreshtoken implementation)
- 2 factor authentication
- External logins (registers a user automatically the first time)
- Authorization: RoleClaims & UserClaims


## 1. Project

Create an ASP Mvc Project *Identity.Web*.


## 2. Identity

### 2.1 Setup

Scaffold new Identity Item.

1. Set current Layout page
2. Add new DbContext *ApplicationDbContext*
3. Add new IdentityUser *ApplicationUser*

Modifications:

- _LoginPartial.cshtml
- _ValidationScriptsPartial: remove /Identity/ in script src tags
- Login, Register: add autocomplete attributes ([see Google suggests](https://developers.google.com/web/fundamentals/security/credential-management/save-forms))
- EnableAuthenticator: display [QR Code](https://docs.microsoft.com/en-gb/aspnet/core/security/authentication/identity-enable-qrcodes?view=aspnetcore-3.1)


### 2.2 Database

The DbContext and User class are later moved to a separate Library.

To enable migrations use MigrationsAssembly("Identity.Web") where the dbContext is added.

*Add-Migration CreateIdentitySchema*

*Update-Database*


### 2.3 Files to override:

- ConfirmEmailChange
- ExternalLogin
- Login
- Register
- Manage/Email
- Manage/EnableAuthenticator
- Manage/ExternalLogins
- Manage/Index


## 3. Identity.Library

Class library with core data, service, and helper classes. Must be in .Net Core 3.1!

- DbContext *IdentityContext*
- UserManager *ApplicationUserManager*
- SignInManager *ApplicationSignInManager* (logs an entry for a login attempt)
- UserClaimsFactory *ApplicationUserClaimsFactory*
- *AccountMailHelper*
- *JwtTokenHelper*
- *LoginEntryStore*
- *RefreshTokenStore*


## 4. Start Up

### 4.1 Startup.cs

Services:

- enable Mvc & RazorPages
- add HttpContextAccessor
- add CORS
- add Authentication (with Cookie)
- add Authorization (configure Policies)

Configure:

- use CORS
- enable Static Files (using ~\wwwroot\dist folder as root)
- use Authentication & Authorization
- endpoints: controllers (with RequireAuthorization) & razorpages (for Identity)


### 4.2 IdentityHostingStartup.cs

Services:

- add IEmailSender *IdentityEmailSender*
- add *LoginEntryStore*
- add *RefreshTokenStore*
- add *AccountMailHelper*
- add IdentityDbContext *IdentityContext<ApplicationUser>*
- add Identity<ApplicationUser, IdentityRole>
- add Default UI
- add EntityFramework store
- add DefaultTokenProviders
- add *ApplicationUserManager*
- add *ApplicationSignInManager*
- add *ApplicationUserClaimsFactory* 
- add Authentication (with external Login Providers)


## 5. Controllers

### AccountController

Uses the Cookies authentication scheme to deliver user information.


### AccountApiController

Uses the Bearer authentication scheme for logging in and to deliver user information.
This controller also enables [refreshing the access token](https://medium.com/@kedren.villena/refresh-jwt-token-with-asp-net-core-c-25c2c9ee984b) by using a refresh token.


## 6. Javascript Client (Jwt)
A JavaScript client (*/wwwroot/dist/index.html*) is used to test the Bearer token and *AccountApiController*.
A **Refresh Token** to avoid invalidating the Access Token is also implemented.
