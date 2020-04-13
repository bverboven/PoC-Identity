# PoC-Identity
Proof of Concept for .Net Core Identity

## 1. Project
Create an ASP Mvc Project *Identity.Web*.

## 2. Identity

### 2.1 Setup
Scaffold new Identity Item.

1. Set current Layout page
2. Add new DbContext *ApplicationDbContext*
3. Add new IdentityUser *ApplicationUser*

### 2.2 Database
The DbContext and User class are later moved to a separate Library.

To enable migrations use MigrationsAssembly("Identity.Web") where the dbContext is added.

#### Migration
Add-Migration CreateIdentitySchema

#### Database
Update-Database

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
- DbContext
- UserClaimsFactory
- AccountMailHelper
- JwtTokenHelper

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
- enable Static Files
- use Authentication & Authorization
- endpoints: controllers (with RequireAuthorization) & razorpages (for Identity)

### 4.2 IdentityHostingStartup.cs
Services:
- add IEmailSender *IdentityEmailSender*
- add IdentityDbContext *IdentityContext<ApplicationUser>*
- add Identity<ApplicationUser, IdentityRole>
- add Default UI
- add EntityFramework store
- add DefaultTokenProviders
- add ClaimsPrincipalFactory *ApplicationUserClaimsFactory* 
- add helper *AccountMailHelper*
- add Authentication (with external Login Providers)
