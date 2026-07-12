## Authenticating an ASP.NET Web App with the OAuth2 Client ID and Secret

Now that you have the **Client ID** and **Client Secret** strings, you'll need to plug those values into
your application.

The following sample code uses the [Google.Apis.Auth](https://www.nuget.org/packages/Google.Apis.Auth/)
nuget package for obtaining the access token which will be needed by MailKit to pass on to the GMail
server.

Add Google Authentication processor to your **Program.cs**.

```csharp
builder.Services.AddAuthentication (options => {
    // This forces challenge results to be handled by Google OpenID Handler, so there's no
    // need to add an AccountController that emits challenges for Login.
    options.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
    
    // This forces forbid results to be handled by Google OpenID Handler, which checks if
    // extra scopes are required and does automatic incremental auth.
    options.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
    
    // Default scheme that will handle everything else.
    // Once a user is authenticated, the OAuth2 token info is stored in cookies.
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie (options => {
    options.ExpireTimeSpan = TimeSpan.FromMinutes (5);
})
.AddGoogleOpenIdConnect (options => {
    var secrets = GoogleClientSecrets.FromFile ("client_secret.json").Secrets;
    options.ClientId = secrets.ClientId;
    options.ClientSecret = secrets.ClientSecret;
});
```

Ensure that you are using Authorization and HttpsRedirection in your **Program.cs**:

```csharp
app.UseHttpsRedirection ();
app.UseStaticFiles ();

app.UseRouting ();

app.UseAuthentication ();
app.UseAuthorization ();
```

Now, using the **GoogleScopedAuthorizeAttribute**, you can request scopes saved in a library as constants and request tokens for these scopes.

```csharp
[GoogleScopedAuthorize(DriveService.ScopeConstants.DriveReadonly)]
public async Task AuthenticateAsync ([FromServices] IGoogleAuthProvider auth)
{
    GoogleCredential? googleCred = await auth.GetCredentialAsync ();
    string token = await googleCred.UnderlyingCredential.GetAccessTokenForRequestAsync ();
    
    var oauth2 = new SaslMechanismOAuthBearer ("UserEmail", token);
    
    using var emailClient = new ImapClient ();
    await emailClient.ConnectAsync ("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
    await emailClient.AuthenticateAsync (oauth2);
    await emailClient.DisconnectAsync (true);
}