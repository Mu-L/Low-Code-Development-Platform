namespace RO.Web
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;
    using System.Web.Security;
    using System.Security.Principal;
    using RO.Facade3;
    using RO.Common3;
    using RO.Common3.Data;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Net;
    using System.Web.Script.Serialization;
    using System.Web.Configuration;
    using System.Configuration;
    using Newtonsoft;
    using Fido2NetLib;
    using Fido2NetLib.Objects;
    using System.Threading.Tasks;
    using Nethereum.Hex.HexConvertors.Extensions;

    public partial class MyAccountModule : RO.Web.ModuleBase
    {
        public class WebAuthnChallenge
        {
            public string nonce;
            public string usrId;
            public string appDomainUrl;
            public string appPath;
            public int expiredAfter;
        }

        public class WebAuthnMeta
        {
            public string credentialId;
            public string publicKey;
            public string aaguid;
            public string authenticatorType;
            public string appPath;
            public string usrId;
            public string platform;
            public string requestIPChain;
            public bool isMobile;
            public bool userPresence;
            public bool userVerified;
        }

        public class AuthenticatorAttestationRawResponseEx : AuthenticatorAttestationRawResponse
        {
            public string platform;
            public bool isMobile;
        }
        private string Encrypt128(string text, string iv, string key)
        {
            // AesCryptoServiceProvider
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            SHA256CryptoServiceProvider hashsha256 = new SHA256CryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 128;
            aes.IV = hashsha256.ComputeHash(UTF8Encoding.UTF8.GetBytes(iv).Take(16).ToArray());
            aes.Key = hashsha256.ComputeHash(UTF8Encoding.UTF8.GetBytes(key).Take(16).ToArray());
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Convert string to byte array
            byte[] src = Encoding.Unicode.GetBytes(text);

            // encryption
            using (ICryptoTransform encrypt = aes.CreateEncryptor())
            {
                byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);

                // Convert byte array to Base64 strings
                return Convert.ToBase64String(dest);
            }
        }

        /// <summary>
        /// AES decryption
        /// </summary>
        private string Decrypt128(string text, string iv, string key)
        {
            // AesCryptoServiceProvider
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            SHA256CryptoServiceProvider hashsha256 = new SHA256CryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 128;
            aes.IV = hashsha256.ComputeHash(UTF8Encoding.UTF8.GetBytes(iv).Take(16).ToArray());
            aes.Key = hashsha256.ComputeHash(UTF8Encoding.UTF8.GetBytes(key).Take(16).ToArray());
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Convert Base64 strings to byte array
            byte[] src = System.Convert.FromBase64String(text);

            // decryption
            using (ICryptoTransform decrypt = aes.CreateDecryptor())
            {
                byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
                return Encoding.Unicode.GetString(dest);
            }
        }


        private byte LcSystemId;
        private string LcSysConnString;
        private string LcAppConnString;
        private string LcAppDb;
        private string LcDesDb;
        private string LcAppPw;

        Dictionary<string, string> CmpPrj = new Dictionary<string, string>();
        string ssd = string.Empty;
        string csy = string.Empty;
        private List<DataRow> LinkedUserLogin = new List<DataRow>();

        public MyAccountModule()
        {
            this.Init += new System.EventHandler(Page_Init);
        }

        protected LoginUsr LoginByJWTToken(string refresh_token)
        {
            var context = HttpContext.Current;
            var auth = GetAuthObject();
            string guid = Guid.NewGuid().ToString();
            string appPath = HttpContext.Current.Request.ApplicationPath;
            string domain = context.Request.Url.GetLeftPart(UriPartial.Authority);
            Func<string, string> getStoredToken = (accessCode) =>
            {
                return refresh_token;
            };
            Func<LoginUsr, UsrCurr, UsrImpr, UsrPref, string, bool, bool> validateScope = (loginUsr, usrCurr, usrImpr, usrPref, currentHandle, ignoreCache) =>
            {
                LUser = loginUsr;
                LCurr = usrCurr;
                LImpr = usrImpr;
                LPref = usrPref;
                return true;
            };
            try
            {
                var access_token = auth.GetToken("", "", "refresh_token", refresh_token, "", "", "", appPath, domain, getStoredToken, validateScope);
                if (access_token != null && access_token.ContainsKey("access_token"))
                    return LUser;
                else return null;
            }
            catch {
                return null;
            }
        }
        protected void SetJWTCookie()
        {
            var context = HttpContext.Current;
            var auth = GetAuthObject();
            int access_token_validity = 10 * 60;
            string guid = Guid.NewGuid().ToString();
            string xForwardedFor = HttpContext.Current.Request.Headers["X-Forwarded-For"];
            string appPath = 
                    !string.IsNullOrEmpty(xForwardedFor) 
                    && !string.IsNullOrEmpty(Config.ExtBasePath) 
                        ? Config.ExtBasePath 
                        : HttpContext.Current.Request.ApplicationPath;
            string domain = context.Request.Url.GetLeftPart(UriPartial.Authority);
            string jwtToken = auth.CreateLoginJWT(LUser, LUser.DefCompanyId, LUser.DefProjectId, LUser.DefSystemId, LCurr, LImpr, appPath, access_token_validity, guid);
            Func<string, string> getStoredToken = (accessCode) =>
            {
                return jwtToken;
            };
            Func<LoginUsr, UsrCurr, UsrImpr, UsrPref, string, bool, bool> validateScope = (loginUsr, usrCurr, usrImpr, usrPref, currentHandle, ignoreCache) =>
            {
                //LUser = loginUsr;
                //LCurr = usrCurr;
                //LImpr = usrImpr;
                return true;
            };
            var access_token = auth.GetToken("", "", "authorization_code", guid, "", "", "", appPath, domain, getStoredToken, validateScope);
            var sha256 = new SHA256Managed();
            var handle = Convert.ToBase64String(sha256.ComputeHash(System.Text.UTF8Encoding.UTF8.GetBytes(LUser.LoginName.ToLower()))).Replace("=","_");
            /* convention must match clientRule.js(and React side) */
            HttpCookie tokenInCookie = new HttpCookie((appPath??"/").Substring(1).ToUpper().Replace("/","") + "_" + "tokenInCookieJS", handle);
            HttpCookie refreshTokenCookie = new HttpCookie((appPath ?? "/").Substring(1).ToUpper().Replace("/", "") + "_" + "tokenJS", access_token["refresh_token"]);
            tokenInCookie.HttpOnly = false;
            tokenInCookie.Path = "/"; // must be root to cater for browser case sensitivity oddity we use app path prefix to avoid name space conflicts
            tokenInCookie.Expires = DateTime.Now.AddMinutes(2);
            tokenInCookie.Secure = IsSecureConnection();
            //tokenInCookie.Domain = domain;
            refreshTokenCookie.HttpOnly = false;
            refreshTokenCookie.Path = "/"; // must be root to cater for browser case sensitivity oddity we use app path prefix to avoid name space conflicts
            //refreshTokenCookie.Domain = domain;
            refreshTokenCookie.Expires = DateTime.Now.AddMinutes(2);
            refreshTokenCookie.Secure = IsSecureConnection();
            Response.Cookies.Add(tokenInCookie);
            Response.Cookies.Add(refreshTokenCookie);

        }
        protected void Page_Load(object sender, System.EventArgs e)
        {
            bool hasTerms = System.IO.File.Exists(Server.MapPath("~/home/terms_of_service.pdf"));

            if ((Request.QueryString["typ"] ?? "").ToUpper() == "E")
            {
                cHome.Visible = false;
            }
            if (LUser != null && LUser.LoginName.ToLower() != "anonymous" && (Request.QueryString["exit"] ?? "").ToString().ToUpper() == "Y")
            {
                base.Signout(true);
                return;
            }
            if (!IsPostBack)
            {
                TwoFactorAuthenticationPanel.Visible = Config.EnableTwoFactorAuth == "Y" || Config.EnableTwoFactorAuth == "M";

                string extAppDomainUrl =
                    !string.IsNullOrWhiteSpace(Config.ExtBaseUrl) && !string.IsNullOrEmpty(Request.Headers["X-Forwarded-For"])
                        ? Config.ExtBaseUrl
                        : string.IsNullOrEmpty(Request.Url.Query)
                            ? Request.Url.AbsoluteUri
                            : Request.Url.AbsoluteUri.Replace(Request.Url.Query, "").Replace(Request.Url.Segments[Request.Url.Segments.Length - 1], "");
                // for client side login sync between React and asp.net(only when served under the same app tree 
                cAppDomainUrl.Text = extAppDomainUrl.EndsWith("/") ? extAppDomainUrl.Substring(0,extAppDomainUrl.Length - 1) : extAppDomainUrl ;
                if (base.SystemsList == null) { base.SystemsDict = (new LoginSystem()).GetSystemsList(string.Empty, string.Empty); }    // Instantiate base.SystemsList.
                if (!Request.IsLocal 
                    && (Config.EnableSsl 
                        || (IsProxy() && Config.ExtBaseUrl.ToLower().StartsWith("https:"))))
                {
                    System.Web.Configuration.SessionStateSection SessionSettings = ConfigurationManager.GetSection("system.web/sessionState") as System.Web.Configuration.SessionStateSection;
                    string sessionCookieName = SessionSettings != null ? SessionSettings.CookieName : null;
                    HttpCookie sessionCookie = Response.Cookies[sessionCookieName];
                    if (Request.Cookies["secureChannel"] == null)
                    {
                        HttpCookie x = new HttpCookie("secureChannel", "test");
                        x.Secure = true;
                        x.HttpOnly = true;
                        Response.AppendCookie(x);
                        HttpCookie y = new HttpCookie("secureChannelResult", "test");
                        y.Secure = true;
                        y.HttpOnly = true;
                        if (sessionCookie != null)
                        {
                            Response.Cookies[sessionCookieName].Expires = new DateTime(1900, 1, 1);
                        }
                        this.Redirect(Request.Url.AbsoluteUri.Replace("http://", "https://"));
                    }
                    if (sessionCookie != null)
                    {
                        if (Request.Cookies[sessionCookieName] != null
                            && Request.Cookies[sessionCookieName].Value != sessionCookie.Value)
                        {
                            Response.Cookies.Remove(sessionCookieName);
                            Response.Cookies.Add(Request.Cookies[sessionCookieName]);
                            Request.Cookies[sessionCookieName].Secure = true;
                        }
                        else
                            sessionCookie.Secure = true;
                    }
                }

                // acquire the Windows login Name if possible 
                if (this.Page.User.Identity.GetType() == typeof(System.Security.Principal.WindowsIdentity)
                    && !string.IsNullOrEmpty(this.Page.User.Identity.Name))
                {
                    Session["WindowsLoginName"] = this.Page.User.Identity.Name;
                }
                if (!IsPostBack &&
                    (Session["RequestWindowsLogin"] ?? "").ToString() == "1")
                {
                    Session["RequestWindowsLogin"] = "2";
                    Response.StatusCode = 401;
                    Response.End();
                }
                if (!IsPostBack
                  && this.Page.User.Identity.IsAuthenticated
                  && !string.IsNullOrEmpty(this.Page.User.Identity.Name)
                  && this.Page.User.Identity.GetType() == typeof(System.Security.Principal.WindowsIdentity)
                  && (Session["RequestWindowsLogin"] ?? "").ToString() != string.Empty)
                {
                    Session.Remove("RequestWindowsLogin");

                    if (!string.IsNullOrEmpty(this.Page.User.Identity.Name)) SSOLogin("", this.Page.User.Identity.Name, "W");
                }

                if (!Request.IsAuthenticated || base.LUser == null || LUser.LoginName.ToLower() == "anonymous")
                {
                    if (Request.QueryString["k"] != null)
                    {
                        RecognizeIp(); ShowLoginPanel();
                    }
                    else if (Request.QueryString["j"] != null)
                    {
                        ShowProfilePanel(true);
                    }
                    else
                    {
                        ShowLoginPanel();
                    }
                }
                else
                {
                    if (!LUser.TwoFactorAuth && Config.EnableTwoFactorAuth == "M")
                    {
                        PreMsgPopup("Please enable two factor authentication.");
                    }

                    bool bOTPRemembered = VerifyRememberedOTP();
                    LUser.OTPValidated = LUser.OTPValidated || bOTPRemembered;
                    
                    cForgetOTPCache.Visible = bOTPRemembered;
                    if (Request.QueryString["k"] != null) RecognizeIp();
                    if (!(Request.QueryString["j"] != null) && !PasswordExpired(LUser) && !LUser.OTPValidated && LUser.TwoFactorAuth && (Config.EnableTwoFactorAuth == "Y" || Config.EnableTwoFactorAuth == "M"))
                    {
                        ShowOTPPanel(bOTPRemembered);
                    }
                    else
                    {
                        bool needToResetPassword = (Request.QueryString["j"] != null || PasswordExpired(LUser));
                        ShowProfilePanel(needToResetPassword);
                        string returnUrl = Request.QueryString["ReturnUrl"];
                        if (!string.IsNullOrEmpty(returnUrl) && !needToResetPassword)
                        {
                            if ((Request.QueryString["response_type"] ?? "").Contains("id_token"))
                            {
                                System.Web.Configuration.SessionStateSection SessionSettings = ConfigurationManager.GetSection("system.web/sessionState") as System.Web.Configuration.SessionStateSection;
                                string sessionCookieName = SessionSettings != null ? SessionSettings.CookieName : null;
                                int defCompanyId = LUser.DefCompanyId;
                                byte defSystemId = LUser.DefSystemId;
                                int defProjectId = LUser.DefProjectId;
                                string resources = "";
                                int access_token_validity = 300;
                                string guidHandle = Guid.NewGuid().ToString();
                                string LoginJWT = GetAuthObject().CreateLoginJWT(LUser, defCompanyId, defProjectId, defSystemId, LCurr, LImpr, resources, access_token_validity, guidHandle);
                                HttpCookie sessionCookie = Request.Cookies[sessionCookieName];
                                HttpCookie c = new HttpCookie("RintagiLoginToken", LoginJWT);
                                c.Path = "/";
                                c.HttpOnly = true;
                                c.Domain = sessionCookie.Domain;
                                SetupSSD();
                                Response.Cookies.Set(c);
                            }

                            if ((Request.QueryString["response_mode"] == "fragment") && !string.IsNullOrEmpty(Request.QueryString["response_type"]))
                            {
                                string access_code = Guid.NewGuid().ToString().Replace("-", "").ToLower();
                                Session["RintagiLoginAccessCode"] = access_code;

                                if (returnUrl.StartsWith("urn:ietf:wg:oauth:2.0:oob:auto")
                                || returnUrl.StartsWith("urn:ietf:wg:oauth:2.0:oob"))
                                {
                                    this.Page.Title = "Success code=" + access_code;
                                    this.Visible = false;
                                    //Response.End();
                                }
                                else
                                {
                                    this.Redirect(returnUrl + (Request.QueryString["response_mode"] == "fragment"
                                        ? "#code=" + access_code // +"&id_token=" + LoginJWT 
                                        : ""));
                                }
                            }
                            else
                            {
                                string script =
                                @"<script type='text/javascript' language='javascript'>
			                Sys.Application.add_load(function () {setTimeout(function() { try { window.location = '" + (!string.IsNullOrEmpty(returnUrl) ? returnUrl : Config.SslUrl) + @"';} catch (er) {};}, 0);});
			            </script>";
                                ScriptManager.RegisterStartupScript(cMsgContent, typeof(Label), "AutoRedirect", script, false);
                            }
                        }
                        else
                        {
                            //Do nothing.
                        }
                    }
                }
                Response.Cache.SetExpires(DateTime.Now.AddSeconds(-60));
                cLoginAttempts.Text = "0";
                cHome.NavigateUrl = Config.SslUrl.StartsWith("http") ? Config.SslUrl : "~/" + Config.SslUrl;

                if (LUser != null && LUser.HasPic)
                {
                    cPicMed.ImageUrl = base.GetUrlWithQSHash("~/DnLoad.aspx?key=" + LUser.UsrId.ToString() + "&tbl=dbo.Usr&knm=UsrId&col=PicMed&sys=3"); cPicMed.Visible = true;
                }
                else { cPicMed.ImageUrl = "~/images/DefaultImg.png"; }

                if ((Config.EnableTwoFactorAuth ?? "") == "Y" || (Config.EnableTwoFactorAuth ?? "") == "M")
                {
                    if (LUser != null && LUser.LoginName != "Anonymous")
                    {
                        RegisterWebAuthnSetup(extAppDomainUrl, null);
                        RegisterEth1WalletSetup(extAppDomainUrl, null);
                    }
                    else
                    {
                        VerifyWebAuthnSetup(extAppDomainUrl, null);
                        VerifyEth1WalletSetup(extAppDomainUrl, null);
                    }
                }
            }

            if (!IsPostBack &&
                (Session["RequestWindowsLink"]??"").ToString() =="1")
            {
                Session["RequestWindowsLink"] = "2";
                Response.StatusCode = 401;
                Response.End();
            }
            if (LUser != null && LUser.UsrName != "Anonymous" 
                && this.Page.User.Identity.IsAuthenticated
              && !string.IsNullOrEmpty(this.Page.User.Identity.Name)
              && this.Page.User.Identity.GetType() == typeof(System.Security.Principal.WindowsIdentity)
              && (Session["RequestWindowsLink"]??"").ToString() =="2"
                )
            {
                Session.Remove("RequestWindowsLink");
                LinkUserLogin(LUser.UsrId, "W", this.Page.User.Identity.Name);
                // different browser handle this windows authentication thing differently
                // so we 'simulate' a form login here as if it is just third party like google
                FormsAuthenticationTicket Ticket = new FormsAuthenticationTicket(LUser.LoginName, false, 3600);
                FormsAuthentication.SetAuthCookie(LUser.LoginName, false);
                HttpContext.Current.User = new GenericPrincipal(new FormsIdentity(Ticket), null);
                Response.End();
            }

            // windows integrated login
            if (IsPostBack
              && this.Page.User.Identity.IsAuthenticated
              && !string.IsNullOrEmpty(this.Page.User.Identity.Name)
              && this.Page.User.Identity.GetType() == typeof(System.Security.Principal.WindowsIdentity)
              && (Session["RequestWindowsLogin"]??"").ToString() != string.Empty)
            {
                Session.Remove("RequestWindowsLogin");

                if (!string.IsNullOrEmpty(this.Page.User.Identity.Name)) SSOLogin("", this.Page.User.Identity.Name, "W");
            }
            if (IsPostBack &&
                LUser != null && LUser.UsrName != "Anonymous"
                && !string.IsNullOrEmpty(this.Page.User.Identity.Name)
                && this.Page.User.Identity.GetType() == typeof(System.Security.Principal.WindowsIdentity)
                && (Session["RequestWindowsLink"] ?? "").ToString() != string.Empty
                )
            {
                Session.Remove("RequestWindowsLink");
                LinkUserLogin(LUser.UsrId, "W", this.Page.User.Identity.Name);
            }
            // windows login via proxy service
            if (!IsPostBack && LoginPanel.Visible 
                //&& !string.IsNullOrEmpty(Request.QueryString["ClaimedID"]) 
                && !string.IsNullOrEmpty(Request.QueryString["LoginTokenTicket"]))
            {
                try
                {
                    //string iv = Session["LoginTokenTicket" + Request.QueryString["LoginTokenTicket"]].ToString();
                    //Session.Remove("LoginTokenTicket" + Request.QueryString["LoginTokenTicket"]);
                    string claimedID = WindowLoginGetClaim(Request.QueryString["LoginTokenTicket"]);

                    //string claimedID = Decrypt128(Request.QueryString["ClaimedID"], iv, System.Web.Configuration.WebConfigurationManager.AppSettings["TrustedLoginFederationKey"]);

                    if (!string.IsNullOrEmpty(claimedID) ) SSOLogin("", claimedID, "W");
                }
                catch { }
            }
            var azureScope = "openid profile email";
            //var azureScope = "https://graph.microsoft.com/.default";

            // microsoft online login
            if (!IsPostBack &&
                (((!string.IsNullOrEmpty(Request.Form["code"]) || !string.IsNullOrEmpty(Request.Form["id_token"])) &&
                !string.IsNullOrEmpty(Request.Form["state"]))
                ||
                (Session["DirectPostedData"] != null && !string.IsNullOrEmpty(((NameValueCollection)Session["DirectPostedData"])["id_token"])))
                )
            {
                var form = Session["DirectPostedData"] as NameValueCollection ?? Request.Form;

                Dictionary<string, string> oauth2Ticket = Session[form["state"]] as Dictionary<string, string>;
                if (oauth2Ticket != null)
                {
                    Session.Remove("DirectPostedData");
                    try
                    {
                        Session.Remove(form["state"]);
                        string office365LoginName = GetAzureLoginID(form["code"], form["id_token"], azureScope);
                        if (!string.IsNullOrEmpty(office365LoginName))
                        {
                            if (LUser == null || LUser.LoginName == "Anonymous")
                            {
                                SSOLogin("", office365LoginName, office365LoginName.Contains("#") ? "M" : "O");
                            }
                            else
                            {
                                LinkUserLogin(LUser.UsrId, office365LoginName.Contains("#") ? "M" : "O", office365LoginName);
                                this.Redirect("~/MyAccount.aspx");
                            }
                        }
                    }
                    catch (Exception err) {
                        ErrorTrace(err, "critical");
                    }
                }
            }

            // Azure OAuth v2.0, access code in query, use state nounce to associate
            if (!IsPostBack
                && !string.IsNullOrEmpty(Request.QueryString["code"])
                && !string.IsNullOrEmpty(Request.QueryString["state"])
                && (Session[Request.QueryString["state"]] as Dictionary<string, string>) != null
                )
            {
                try
                {
                    Dictionary<string, string> oauth2Ticket = Session[Request.QueryString["state"]] as Dictionary<string, string>;
                    Session.Remove(Request.QueryString["state"]);
                    string returnUrl = oauth2Ticket.ContainsKey("ReturnUrl") ? oauth2Ticket["ReturnUrl"] : "";
                    string office365LoginName = GetAzureLoginID(Request.QueryString["code"], null, azureScope);
                    if (!string.IsNullOrEmpty(office365LoginName))
                    {
                        if (LUser == null || LUser.LoginName == "Anonymous")
                        {
                            SSOLogin("", office365LoginName, office365LoginName.Contains("#") ? "M" : "O", returnUrl);
                        }
                        else
                        {
                            LinkUserLogin(LUser.UsrId, office365LoginName.Contains("#") ? "M" : "O", office365LoginName);
                            this.Redirect("~/MyAccount.aspx");
                        }
                    }
                }
                catch (Exception err)
                {
                    ErrorTrace(err, "critical");
                }
            }

            /* login from JWT Token(from React side) */
            if (IsPostBack
              && !string.IsNullOrEmpty(cJWTToken.Text))
            {

                if (!string.IsNullOrEmpty(cJWTToken.Text)) SSOLogin("", "", "SJ");
            }


            if (LUser != null && LUser.LoginName != "Anonymous")
            {
                LinkedUserLogin = (new LoginSystem().GetLinkedUserLogin(LUser.UsrId).AsEnumerable()).ToList();
                cJWTLogin.Visible = false;
                cRemoveLinkedLogin.Visible = cRemoveLinkedLogin.Visible && LinkedUserLogin.Count > 0;
                cRemoveLinkedLogin.Checked = LImpr.UsrGroups == "5"; 
            }

            if (!string.IsNullOrEmpty(Config.GoogleClientId))
            {
                GoogleLoginPanel.Visible = LoginPanel.Visible;
                cLinkGooglePanel.Visible = NewProfilePanel.Visible;
                cGoogleAccessToken.Visible = true;
                cGoogleLoginBtn.Visible = true;
                LoadGoogleClient(Config.GoogleClientId);
                cGoogleSignInBtn.OnClientClick = "GoogleSignIn('" + Config.GoogleClientId + "','" + cGoogleAccessToken.ClientID + "','" + cGoogleLoginBtn.ClientID + "');return false;";
                if ((from dr in LinkedUserLogin where "G".IndexOf(dr["ProviderCd"].ToString().Trim()) >= 0 select dr).Count() == 0) cLinkGoogleBtn.OnClientClick = "GoogleSignIn('" + Config.GoogleClientId + "','" + cGoogleAccessToken.ClientID + "','" + cGoogleLoginBtn.ClientID + "');return false;";
            }
            if (!string.IsNullOrEmpty(Config.FacebookAppId))
            {
                FacebookLoginPanel.Visible = LoginPanel.Visible;
                cLinkFacebookPanel.Visible = NewProfilePanel.Visible;
                cFacebookAccessToken.Visible = true;
                cFacebookLoginBtn.Visible = true;
                LoadFacebookClient(Config.FacebookAppId, "en-us", "");
                cFacebookSignInBtn.OnClientClick = "FacebookSignIn('" + Config.FacebookAppId + "','" + cFacebookAccessToken.ClientID + "','" + cFacebookLoginBtn.ClientID + "');return false;";
                if ((from dr in LinkedUserLogin where "F".IndexOf(dr["ProviderCd"].ToString().Trim()) >= 0 select dr).Count() == 0) cLinkFacebookBtn.OnClientClick = "FacebookSignIn('" + Config.FacebookAppId + "','" + cFacebookAccessToken.ClientID + "','" + cFacebookLoginBtn.ClientID + "');return false;";
            }
            if (GoogleLoginPanel.Visible == true || FacebookLoginPanel.Visible == true)
            {
                separatePanel.Visible = true;
            }
            if (LoginPanel.Visible && !string.IsNullOrEmpty(Config.SignUpUrl))
            {
                SignUpPanel.Visible = true; cSignUpUrl.NavigateUrl = Config.SignUpUrl;
            }
            if (!string.IsNullOrEmpty(Config.AzureAPIClientId))
            {
                Office365LoginPanel.Visible = LoginPanel.Visible;
                cLinkMicrosoftPanel.Visible = NewProfilePanel.Visible;
            }
            if (!string.IsNullOrEmpty(Config.TrustedLoginFederationUrl))
            {
                WindowsLoginPanel.Visible = LoginPanel.Visible ;
                cLinkWindowsPanel.Visible = NewProfilePanel.Visible;
            }

            TermsPanel.Visible = hasTerms && Request.QueryString["j"] != null; // only show when there is terms of service and this is password reset request
            TranslateItems();
            // always disable the confirm dialog panel, postback or non-postback
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, string> labels = (from x in GetLabels().AsEnumerable()
                                                 select x).ToDictionary(dr => dr.Field<string>(0), dr => dr.Field<string>(1));
            string jsonObj = jss.Serialize(labels);
            ScriptManager.RegisterStartupScript(this, this.GetType(), this.ClientID + "TermTranslation", string.Format(this.ClientID + "_Labels={0};", jsonObj), true);

        }

        #region CultureId autocomplete support
        protected void cbPostBack(object sender, System.EventArgs e)
        {
        }

        protected void cbCultureId1(object sender, System.EventArgs e)
        {
            SetCultureId1((RoboCoder.WebControls.ComboBox)sender, string.Empty);
        }

        private void SetCultureId1(RoboCoder.WebControls.ComboBox ddl, string keyId)
        {
            System.Collections.Generic.Dictionary<string, string> context = new System.Collections.Generic.Dictionary<string, string>();
            context["method"] = "GetDdlCultureId";
            context["addnew"] = "Y";
            context["mKey"] = "CultureTypeId";
            context["mVal"] = "CultureTypeLabel";
            context["mTip"] = "CultureTypeLabel";
            context["mImg"] = "CultureTypeLabel";
            context["ssd"] = Request.QueryString["ssd"];
            context["scr"] = "1";
            context["csy"] = "3";
            context["filter"] = "0";
            context["isSys"] = "N";
            context["conn"] = string.Empty;
            ddl.AutoCompleteUrl = "AutoComplete.aspx/DdlSuggests";
            ddl.DataContext = context;
            if (ddl != null)
            {
                DataView dv = null;
                if (keyId == string.Empty && ddl.SearchText.StartsWith("**")) { keyId = ddl.SearchText.Substring(2); }
                try
                {
                    dv = new DataView((new AdminSystem()).GetDdl(1, "GetDdlCultureId", true, false, 0, keyId, null, null, string.Empty, base.LImpr, base.LCurr));
                }
                catch { return; }
                ddl.DataSource = dv;
                try { ddl.SelectByValue(keyId, string.Empty, false); }
                catch { try { ddl.SelectedIndex = 0; } catch { } }
            }
        }
        #endregion

        protected void Page_Init(object sender, EventArgs e)
        {
            InitializeComponent();
            EnforceSSL();
        }

        #region Web Form Designer generated code
        ///		Required method for Designer support - do not modify
        ///		the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            if (LcSysConnString == null) { SetSystem(3); }

        }
        #endregion

        private void SetSystem(byte SystemId)
        {
            LcSystemId = SystemId;
            LcSysConnString = base.SysConnectStr(SystemId);
            LcAppConnString = base.AppConnectStr(SystemId);
            LcDesDb = base.DesDb(SystemId);
            LcAppDb = base.AppDb(SystemId);
            LcAppPw = base.AppPwd(SystemId);
        }

        private void ShowLoginPanel()
        {
            LoginPanel.Visible = true;
            cLang.Visible = false;
            PwdResetPanel.Visible = false;
            UserProfilePanel.Visible = false;
            cLoginName.Focus();
            if (Request.QueryString["wrn"] != null)
            {
                if (Request.QueryString["wrn"].ToString() == "1")
                {
                    cWarnMsg1.Visible = true; cWarnMsg2.Visible = false;
                }
                else
                {
                    cWarnMsg1.Visible = false; cWarnMsg2.Visible = true;
                }
            }
            else { cWarnMsg1.Visible = false; cWarnMsg1.Visible = false; }
            cMaintMsg.Text = (new AdminSystem()).GetMaintMsg();
            if (string.IsNullOrEmpty(cMaintMsg.Text)) { cMaintMsg.Visible = false; }
        }

        private void RecognizeIp()
        {
            try
            {
                if (Request.QueryString["k"] != null)
                {
                    string usrId = Request.QueryString["k"];
                    if (ValidateResetUrl(usrId).Value)
                    {
                        (new LoginSystem()).SetUsrSafeIP(int.Parse(usrId), Request.QueryString["ip"]);  // userId is ok but ip should be hashed for better security.
                        PreMsgPopup("Your IP address has been inserted into the safe list, thank you."); return;
                    }
                    else
                    {
                        throw new Exception("Invalid Reset Request");
                    }
                }
            }
            catch {  }
        }

        private string ResetRequestLoginName()
        {
            try
            {
                if (!string.IsNullOrEmpty(Request.QueryString["j"]))
                {
                    string usrId = Request.QueryString["j"];
                    var usr = ValidateResetUrl(usrId);
                    if (usr.Value)
                    {
                        return usr.Key;
                    }
                    else
                    {
                        throw new Exception("Invalid Reset Request");
                    }
                }
            }
            catch { return null; }
            return null;
        }

        private void ShowOTPPanel(bool bOTPRemembered)
        {
            LoginPanel.Visible = false;
            PwdResetPanel.Visible = false;
            OTPPanel.Visible = true;
            UserProfilePanel.Visible = false;
            cLang.Visible = false;
            cOTPAccessCode.EncryptionKey = (new LoginSystem()).WrGetUsrOTPSecret(LUser.UsrId);
        }

        private void ShowProfilePanel(bool bChangePwd)
        {
            LoginPanel.Visible = false;
            PwdResetPanel.Visible = false;
            UserProfilePanel.Visible = true; cHome.Visible = LUser != null && LUser.LoginName.ToLower() != "anonymous" && (Request.QueryString["typ"] ?? "").ToUpper() != "E";
            if (bChangePwd)
            {
                if (ResetRequestLoginName() != null || (LUser != null && LUser.LoginName.ToLower() != "anonymous"))
                {
                    cLang.Visible = false;
                    NewProfilePanel.Visible = false;
                    cRemoveLinkedLogin.Visible = false;
                    cUpdPwdBtn.OnClientClick = "return UpdPassword(true)"; // bypass password prompt  
                    cNewUsrPassword.Focus();
                }
                else
                {
                    ShowLoginPanel();
                }
                TwoFactorAuthenticationPanel.Visible = false;
            }
            else
            {
                cNewLoginName.Text = LUser.LoginName;
                cNewUsrName.Text = LUser.UsrName;
                cNewUsrEmail.Text = LUser.UsrEmail;
                SetCultureId1(cLang, LUser.CultureId.ToString());
                cLang.Visible = true;
                if (!string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]) && !bChangePwd)
                {
                    FormsAuthentication.RedirectFromLoginPage(LUser.LoginName, false);
                }
                if (LUser.TwoFactorAuth)
                {
                    TranslateItem(cDisableTwoFactor, GetLabels().Rows, "cDisableTwoFactor");
                }
                else
                {
                    cDisableTwoFactor.Visible = false;
                    TranslateItem(cDisableTwoFactor, GetLabels().Rows, "OTPEnableBtn");
                }
                cHardenedLogin.Text = LUser.OTPValidated && (Config.EnableTwoFactorAuth == "Y" || Config.EnableTwoFactorAuth == "M") ? "Y" : "N";
                cShowTwoFactorKey.Visible = LUser.TwoFactorAuth;
            }
        }

        private void ShowResetPwdPanel()
        {
            LoginPanel.Visible = false;
            PwdResetPanel.Visible = true;
            UserProfilePanel.Visible = false;
            cResetLoginName.Focus();
        }

        public void cPicMedUpl_Click(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrEmpty(LUser.UsrId.ToString()) && cPicMedFi.HasFile && cPicMedFi.PostedFile.FileName != string.Empty && "image/gif,image/jpeg,image/png,image/tiff,image/pjpeg,image/x-png".IndexOf(cPicMedFi.PostedFile.ContentType) >= 0)
            {
                byte[] dc;
                System.Drawing.Image oBMP = System.Drawing.Image.FromStream(cPicMedFi.PostedFile.InputStream);
                int nHeight = 100;
                int nWidth = int.Parse((Math.Round(decimal.Parse(oBMP.Width.ToString()) * (100 / decimal.Parse(oBMP.Height.ToString())))).ToString());
                Bitmap nBMP = new Bitmap(oBMP, nWidth, nHeight);
                using (System.IO.MemoryStream sm = new System.IO.MemoryStream())
                {
                    nBMP.Save(sm, System.Drawing.Imaging.ImageFormat.Jpeg);
                    sm.Position = 0;
                    dc = new byte[sm.Length + 1];
                    sm.Read(dc, 0, dc.Length); sm.Close();
                }
                oBMP.Dispose(); nBMP.Dispose();
                (new AdminSystem()).UpdDbImg(LUser.UsrId.ToString(), "dbo.Usr", "UsrId", "PicMed", dc, LcAppConnString, LcAppPw);
                cPicMed.ImageUrl = "~/DnLoad.aspx?key=" + LUser.UsrId.ToString() + "&tbl=dbo.Usr&knm=UsrId&col=PicMed&sys=3";
                cPicMedPan.Visible = false; cPicMed.Visible = true;
            }
            else if (!(!string.IsNullOrEmpty(LUser.UsrId.ToString()))) { PreMsgPopup("Please save the record first before upload"); }
        }

        public void cPicMedTgo_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            if (cPicMed.Visible)
            {
                cPicMedPan.Visible = true; cPicMed.Visible = false;
            }
            else
            {
                cPicMedPan.Visible = false; cPicMed.Visible = true;
            }
        }

        public void ConfirmPwdBtn_Click(object sender, System.EventArgs e)
        {
            Credential cr = new Credential(LUser.LoginName, cConfirmPwd.Text);
            LoginUsr usr = (new LoginSystem()).GetLoginSecure(cr);
            if (usr == null)
            {
                if (!(new LoginSystem()).IsNullLegacyPwd(LUser.LoginName))
                {
                    usr = (new LoginSystem()).GetLoginLegacy(LUser.LoginName, cConfirmPwd.Text);
                }
            }

        }
        public void cCancelAccount_Click(object sender, System.EventArgs e)
        {
            try
            {
                (new LoginSystem()).CancelUserAccount(LUser.UsrId);
                cMsg.Text = TranslateItem(GetLabels().Rows, "AccountCancelledMsg"); cMsgDiv.Visible = true;
            }
            catch (Exception err)
            {
                PreMsgPopup(err.Message); return;
            }
        }
        private bool SetLoginUser(LoginUsr usr, bool redirect, Action locked, Action failed, Credential cr, string Provider, string ProviderLoginName, string returnUrl)
        {
            if (usr != null && usr.UsrId > 0)
            {
                if ((new LoginSystem()).ChkLoginStatus(usr.LoginName))
                {
                    if (!(new LoginSystem()).IsUsrSafeIP(usr.UsrId, GetVisitorIPAddress())) // Email the account holder.
                    {
                        string from = base.SysCustServEmail(3);
                        var reset_url1 = GetResetLoginUrl(usr.UsrId.ToString(), "", "", "k", "&ip=" + HttpUtility.UrlEncode(GetVisitorIPAddress()), null, null);
                        var reset_url2 = GetResetLoginUrl(usr.UsrId.ToString(), "", "", "j", "", null, null);
                        string machineName = Environment.MachineName;
                        string sBody = "Someone recently tried to login to your account '" + usr.LoginName.Left(3) + "****" + usr.LoginName.Right(3) + "' at <b>"
                                + ResolveUrlCustom(HttpContext.Current.Request.Url.AbsolutePath, false, true)
                                + string.Format(" (On Server {0})", machineName) + "</b> from an unrecognized IP location <b>"
                                + GetVisitorIPAddress()
                                + "</b>.<br /><br />You may choose to ignore this message or click <a href=" + reset_url1.Value + ">YES</a> if this IP Address location will be used again or click <a href=" + reset_url2.Value + ">NO</a> to reset your password immediately.";
                        try
                        {
                            base.SendEmail("Review Recent Login", sBody, usr.UsrEmail, from, from, Config.WebTitle + " Customer Care", true);
                        }
                        catch {}
                    }
                    (new LoginSystem()).SetLoginStatus(usr.LoginName, true, GetVisitorIPAddress(),Provider, ProviderLoginName);
                    ResetSSD();
                    base.LUser = usr;
                    base.LCurr = new UsrCurr(usr.DefCompanyId, usr.DefProjectId, usr.DefSystemId, usr.DefSystemId);
                    SetUsrPreference();
                    base.LImpr = null; SetImpersonation(usr.UsrId);
                    base.VMenu = null;
                    if (usr.LoginName != "Anonymous")
                    {
                        LinkedUserLogin = (new LoginSystem().GetLinkedUserLogin(LUser.UsrId).AsEnumerable()).ToList();
                        string appSig = cMyAppSig.Text;
                        string appMachine = cMyMachine.Text;
                        string fcmToken = cFCMToken.Text;
                        if (!string.IsNullOrEmpty(fcmToken))
                        {
                            try
                            {
                                new LoginSystem().UpdUsrNotificationChannel(usr.UsrId, fcmToken, Request.UserAgent, GetClientIp(), appMachine, appSig, "fcm");
                            }
                            catch (Exception ex)
                            {
                                ErrorTrace(ex, "warning");
                            }
                        }
                    }
                    Session.Remove("ProjectList");
                    Session.Remove("CompanyList");
                    if (Provider != "SJ")
                    {
                        SetJWTCookie();
                    }
                    if (redirect)
                    {
                        if (this.Page.User.Identity.GetType() == typeof(System.Security.Principal.WindowsIdentity) || !string.IsNullOrEmpty(returnUrl))
                        {
                            // different browser handle this windows authentication thing differently
                            // so we 'simulate' a form login here as if it is just third party like google
                            FormsAuthenticationTicket Ticket = new FormsAuthenticationTicket(usr.LoginName, false, 3600);
                            FormsAuthentication.SetAuthCookie(usr.LoginName, false);
                            HttpContext.Current.User = new GenericPrincipal(new FormsIdentity(Ticket), null);
                            string returnURL = !string.IsNullOrEmpty(returnUrl) ? returnUrl : Request.QueryString["ReturnUrl"];

                            if (!string.IsNullOrEmpty(returnURL) && !returnURL.StartsWith(FormsAuthentication.LoginUrl)) this.Redirect(returnURL);
                            else this.Redirect("Default.aspx");
                        }
                        else
                            FormsAuthentication.RedirectFromLoginPage(usr.LoginName, false);
                    }
                    else
                    {

                        FormsAuthenticationTicket Ticket = new FormsAuthenticationTicket(usr.LoginName, false, 3600);
                        FormsAuthentication.SetAuthCookie(usr.LoginName, false);
                        HttpContext.Current.User = new GenericPrincipal(new FormsIdentity(Ticket), null);
                    }
                    return true;
                }
                else
                {
                    locked();
                    return false;
                }
            }
            else
            {
                if (Provider != "SJ") (new LoginSystem()).SetLoginStatus(cLoginName.Text, false, GetVisitorIPAddress(),Provider,ProviderLoginName);
                failed();
                return false;
            }
        }
        private bool LoginUser(string LoginName, string Password, bool redirect, Action locked, Action failed)
        {
            Credential cr = new Credential(LoginName, Password);
            LoginUsr usr = (new LoginSystem()).GetLoginSecure(cr);
            if (usr == null)
            {
                if (!(new LoginSystem()).IsNullLegacyPwd(LoginName))
                {
                    usr = (new LoginSystem()).GetLoginLegacy(LoginName, Password);
                }
            }
            return SetLoginUser(usr, redirect, locked, failed, cr,"","", null);
        }

        public void SSOLogin(string SelectedLoginName, string ProviderLoginName, string Provider, string returnUrl = null)
        {
            LoginUsr intendedUser = Provider == "SJ" ? LoginByJWTToken(cJWTToken.Text) : null;
            string providerLoginName = (Provider ?? "").Trim() == "O" || (Provider ?? "").Trim() == "M"
                                ? ProviderLoginName.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Last()
                                : ProviderLoginName;
            Credential cr = new Credential(SelectedLoginName, intendedUser != null ? intendedUser.UsrId.ToString() : providerLoginName, new byte[32], Provider.Left(5));
            LoginUsr usr = (new LoginSystem()).GetLoginSecure(cr);

            if (usr != null)
            {
                usr.OTPValidated = !string.IsNullOrEmpty(Provider);
            }
            else if (Provider == "SJ")
            {
                LUser = null;
            }
            if (Provider == "SJ")
            {
                cJWTToken.Visible = false;
                cJWTLogin.Visible = false;

            }
            bool bBlocked = !string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]) || !string.IsNullOrEmpty(returnUrl);
            bool success = SetLoginUser(usr, bBlocked || string.IsNullOrEmpty(Request.QueryString["typ"]),
                () => { PreMsgPopup(TranslateItem(GetLabels().Rows, "AccountLockedMsg")); },
                () =>
                {
                    if (Provider != "SJ")
                    {
                        PreMsgPopup(TranslateItem(GetLabels().Rows, "LoginFailedMsg"));
                    }
                    ShowLoginPanel();
                }, cr, Provider, ProviderLoginName, returnUrl);
            if (!bBlocked && !string.IsNullOrEmpty(Request.QueryString["typ"]) && usr != null)
            {
                string guid = Guid.NewGuid().ToString();
                string appPath = HttpContext.Current.Request.ApplicationPath;
                int access_token_validity = 10 * 60;
                if (Provider != "SJ")
                {
                    string jwtToken = GetAuthObject().CreateLoginJWT(LUser, usr.DefCompanyId, usr.DefProjectId, usr.DefSystemId, LCurr, LImpr, appPath, access_token_validity, guid);
                }
                else
                {
                }

                this.Redirect(Config.SslUrl);
            }
        }

        public void GoogleLoginBtn_Click(object sender, System.EventArgs e)
        {
            string accessToken = cGoogleAccessToken.Text;
            Dictionary<string, object> profile = GetGoogleProfile(accessToken);
            if (profile.ContainsKey("email"))
            {
                string providerLoginName = profile["email"].ToString();
                if (!Request.IsAuthenticated || LUser == null || LUser.LoginName == "Anonymous")
                {
                    DataTable dt = (new LoginSystem()).GetLogins(providerLoginName, "G");
                    if (dt.Rows.Count <= 1) SSOLogin("", providerLoginName, "G");
                    else
                    {
                        cLoginNameChoice.DataSource = dt.DefaultView;
                        cLoginNameChoice.DataBind();
                        cLoginNameChoice.SelectedIndex = 0;
                        cProviderLoginName.Text = providerLoginName;
                        cProvider.Text = "G";
                        SelectLoginPanel.Visible = true;
                        LoginPanel.Visible = false;

                    }
                }
                else
                {
                    LinkUserLogin(LUser.UsrId, "G", profile["email"].ToString());
                }
            }
        }
        public void WebAuthnLoginBtn_Click(object sender, System.EventArgs e)
        {
            try
            {
                var x = cWebAuthnResult.Text;
                Fido2Configuration fido2Config = new Fido2Configuration
                {
                    Origin = "https://" + XHost(),
                    ServerDomain = XHost(),
                    ServerName = "site name",
                    ServerIcon = "https://www.rintagi.com/images/a.gif",
                    Timeout = 60 * 1000,
                    TimestampDriftTolerance = 1000
                };

                var fido2 = new Fido2(fido2Config);


                // 4. Create callback to check if userhandle owns the credentialId
                IsUserHandleOwnerOfCredentialIdAsync callback = async (args) =>
                {
                    var u = args.UserHandle;
                    var id = args.CredentialId;
                    return await Task.FromResult(true);
                };

                // 5. Make the assertion
                var request = Newtonsoft.Json.JsonConvert.DeserializeObject<AssertionOptions>(cAssertionRequest.Text);
                AuthenticatorAssertionRawResponse assertionResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthenticatorAssertionRawResponse>(x);
                var credentialId = base64UrlEncode(assertionResponse.Id);
                // 3. Get credential counter from database(design flaw of WebAuthn, required centralized storage), similar to ethereum transaction nonce
                uint storedCounter = 0; // 0 means always success
                var dtLogin = new LoginSystem().GetLogins(credentialId, "Fido2");
                var loginMeta = dtLogin.Rows[0]["LoginMeta"].ToString();
                var loginName = dtLogin.Rows[0]["RLoginName"].ToString();
                var webAuthMeta = Newtonsoft.Json.JsonConvert.DeserializeObject<WebAuthnMeta>(loginMeta);
                var publicKey = base64UrlDecode(webAuthMeta.publicKey);
                var success = fido2.MakeAssertionAsync(assertionResponse, request, publicKey, storedCounter, callback).Result;
                var clientData = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthenticatorResponse>(System.Text.UTF8Encoding.UTF8.GetString(assertionResponse.Response.ClientDataJson));
                var challenge = clientData.Challenge;
                var assertionState = Newtonsoft.Json.JsonConvert.DeserializeObject<WebAuthnChallenge>(System.Text.UTF8Encoding.UTF8.GetString(challenge));
                var authData = new AuthenticatorData(assertionResponse.Response.AuthenticatorData);
                var userPresence = authData.UserPresent;
                var userVerified = authData.UserVerified;

                SSOLogin(null, credentialId, "Fido2");
            }
            catch (Exception ex)
            {
                Common3.Utils.NeverThrow(ex);
                PreMsgPopup("login failed");
            }
        }
        public void Eth1WalletLoginBtn_Click(object sender, System.EventArgs e)
        {
            string sig = cWebAuthnResult.Text;
            if (!string.IsNullOrEmpty(sig))
            {
                byte[] message = System.Text.UTF8Encoding.UTF8.GetBytes(cEth1AssertionRequest.Text);
                try
                {
                    string messageSigner = new Nethereum.Signer.EthereumMessageSigner().EcRecover(message, sig);
                    SSOLogin(null, messageSigner, "Eth1");
                }
                catch (Exception ex)
                {
                    Common3.Utils.NeverThrow(ex);
                    PreMsgPopup("login failed");
                }

            }
        }
        public void FacebookLoginBtn_Click(object sender, System.EventArgs e)
        {
            string accessToken = cFacebookAccessToken.Text;
            Dictionary<string, object> profile = GetFacebookProfile(accessToken);
            if (profile.ContainsKey("email"))
            {
                string providerLoginName = profile["email"].ToString();
                if (!Request.IsAuthenticated || LUser == null || LUser.LoginName == "Anonymous")
                {
                    DataTable dt = (new LoginSystem()).GetLogins(providerLoginName, "F");
                    if (dt.Rows.Count <= 1) SSOLogin("", providerLoginName, "F");
                    else
                    {
                        cLoginNameChoice.DataSource = dt.DefaultView;
                        cLoginNameChoice.DataBind();
                        cLoginNameChoice.SelectedIndex = 0;
                        cProviderLoginName.Text = providerLoginName;
                        cProvider.Text = "F";
                        SelectLoginPanel.Visible = true;
                        LoginPanel.Visible = false;
                    }
                }
                else
                {
                    LinkUserLogin(LUser.UsrId, "F", profile["email"].ToString());
                }
            }
        }
        private string GetHMACHash(string val)
        {
            System.Security.Cryptography.HMACSHA256 hmac = new System.Security.Cryptography.HMACSHA256(UTF8Encoding.UTF8.GetBytes(System.Web.Configuration.WebConfigurationManager.AppSettings["TrustedLoginFederationKey"]));
            byte[] hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(val));
            string hashString = BitConverter.ToString(hash);
            return hashString.Replace("-", "");
        }

        private string WindowLoginGetClaim(string LoginTokenTicket)
        {
            string reverseVia = Request.ServerVariables["HTTP_REVERSE_VIA"];
            string clientIP = Request.UserHostAddress;
            string browserClientIP = (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["HTTP_CLIENT_IP"] ?? "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            string userAgent = Request.UserAgent;
            Dictionary<string, string> param = new Dictionary<string, string>();
            param["Ticket"] = LoginTokenTicket;
            param["Sig"] = GetHMACHash(LoginTokenTicket);
            string y = new JavaScriptSerializer().Serialize(param);
            string url = System.Web.Configuration.WebConfigurationManager.AppSettings["TrustedLoginFederationUrl"] + "/GetLoginClaim";
            Uri uri = new Uri(url);
            System.Net.HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);
            webRequest.PreAuthenticate = true;
            webRequest.Accept = "application/json";
            webRequest.ContentType = "application/json";
            webRequest.Method = "POST";
            webRequest.KeepAlive = true;
            byte[] data = Encoding.UTF8.GetBytes(y);
            webRequest.ContentLength = data.Length;
            System.IO.Stream dataStream = webRequest.GetRequestStream();
            dataStream.Write(data, 0, data.Length);
            dataStream.Close();
            try
            {
                var webResponse = webRequest.GetResponse();
                var responseStream = webResponse.GetResponseStream();
                var sr = new System.IO.StreamReader(responseStream, Encoding.UTF8);
                var result = sr.ReadToEnd();
                Dictionary<string, Dictionary<string, string>> ret = new JavaScriptSerializer().Deserialize<Dictionary<string, Dictionary<string, string>>>(result);
                Dictionary<string, string> claimedInfo = ret["d"];
                if (claimedInfo["LoginUserAgent"] == userAgent)
                {
                    return Decrypt128(claimedInfo["ClaimedID"], claimedInfo["ClaimedIV"], System.Web.Configuration.WebConfigurationManager.AppSettings["TrustedLoginFederationKey"]);
                }
                return "";
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    using (System.IO.Stream responseStream = response.GetResponseStream())
                    using (var reader = new System.IO.StreamReader(responseStream))
                    {
                        string text = reader.ReadToEnd();
                        if (response.ContentType.StartsWith("application/json"))
                        {
                            return "";
                        }
                        else
                        {
                            return "";
                            //throw new Exception(text, e);
                        }
                    }
                }
            }
        }
        private string WindowLoginRegisterTicket(string json)
        {
            string reverseVia = Request.ServerVariables["HTTP_REVERSE_VIA"];
            string clientIP = Request.UserHostAddress;
            string browserClientIP = (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["HTTP_CLIENT_IP"] ?? "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            string userAgent = Request.UserAgent;
            Dictionary<string, string> param = new Dictionary<string, string>();
            string randomIV = Guid.NewGuid().ToString().Replace("-", "");
            string ticket = Guid.NewGuid().ToString().Replace("-", "");
            param["Ticket"] = ticket;
            param["ClaimedIV"] = randomIV;
            param["Sig"] = GetHMACHash(randomIV);
            Dictionary<string, Dictionary<string, string>> x = new Dictionary<string, Dictionary<string, string>> { { "TicketInfo", param } };
            string y = new JavaScriptSerializer().Serialize(x);
            string url = Config.TrustedLoginFederationUrl + "/RegisterLoginTicket";
            Uri uri = new Uri(url);
            System.Net.HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);
            webRequest.PreAuthenticate = true;
            webRequest.Accept = "application/json";
            webRequest.ContentType = "application/json";
            webRequest.Method = "POST";
            webRequest.KeepAlive = true;
            byte[] data = Encoding.UTF8.GetBytes(y);
            webRequest.ContentLength = data.Length;
            System.IO.Stream dataStream = webRequest.GetRequestStream();
            dataStream.Write(data, 0, data.Length);
            dataStream.Close();
            try
            {
                var webResponse = webRequest.GetResponse();
                var responseStream = webResponse.GetResponseStream();
                var sr = new System.IO.StreamReader(responseStream, Encoding.UTF8);
                var result = sr.ReadToEnd();
                //return result;
                return ticket;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    using (System.IO.Stream responseStream = response.GetResponseStream())
                    using (var reader = new System.IO.StreamReader(responseStream))
                    {
                        string text = reader.ReadToEnd();
                        if (response.ContentType.StartsWith("application/json"))
                        {
                            return "";
                        }
                        else
                        {
                            return "";
                            //throw new Exception(text, e);
                        }
                    }
                }
            }
        }

        public void WindowsLoginBtn_Click(object sender, System.EventArgs e)
        {
            if (this.Page.User.Identity.IsAuthenticated
                && !string.IsNullOrEmpty(this.Page.User.Identity.Name)
                && this.Page.User.Identity.GetType() == typeof(System.Security.Principal.WindowsIdentity))
            {
                if (!string.IsNullOrEmpty(this.Page.User.Identity.Name)) SSOLogin("", this.Page.User.Identity.Name, "W");
            }
            else if (Config.TrustedLoginFederationUrl == "integrated")
            {
                Session["RequestWindowsLogin"] = "1";
                Response.StatusCode = 401;
            }
            else
            {
                string myHostAndPort = Request.Url.GetComponents(UriComponents.HostAndPort, UriFormat.SafeUnescaped);
                string loginUrl = Config.TrustedLoginFederationUrl.ReplaceInsensitive("localhost", Request.Url.Host);
                string myUrl = Request.Url.ToString();
                string randomIV = Guid.NewGuid().ToString().Replace("-", "");
                string ticket = WindowLoginRegisterTicket(randomIV);
                Session["LoginTokenTicket" + ticket] = randomIV;
                this.Redirect(loginUrl + (loginUrl.Contains("?") ? "&" : "?") + "LoginTokenTicket=" + System.Web.HttpUtility.UrlEncode(ticket) + "&ReturnUrl=" + System.Web.HttpUtility.UrlEncode(myUrl));
                //string accessToken = cFacebookAccessToken.Text;
                //Dictionary<string, object> profile = GetFacebookProfile(accessToken);
                //if (!string.IsNullOrEmpty(windowsLoginName))
                //{
                //    string providerLoginName = windowsLoginName;
                //    SSOLogin("", providerLoginName, "W");
                //}
            }

        }
        public void AzureLoginBtn_Click(object sender, System.EventArgs e)
        {
            string clientID = Config.AzureAPIClientId;
            string secret = Config.AzureAPIScret;
            string state = Guid.NewGuid().ToString().Replace("-", "");
            string replyUrl = Config.AzureAPIRedirectUrl;
            //string scope = "https://graph.microsoft.com/user.read https://graph.microsoft.com/.default";
            string azureScope = "openid profile email";
            //string azureScope = "openid profile email https://graph.microsoft.com/.default";
            string loginUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize"
                    + "?response_type=code"
                //   + "?response_type=id_token"
                //                                    + "%20id_token"
                    + "&response_mode=query" // only allow code
                //                    + "&response_mode=form_post" // no cookie sent
                /* use this to get the id_token directly
                 */
                    + "&redirect_uri=" + System.Web.HttpUtility.UrlEncode(replyUrl)
                    + "&client_id=" + clientID
                    + "&nonce=" + state
                    + "&scope=" + System.Web.HttpUtility.UrlEncode(azureScope).Replace("+", "%20")
                    + (LUser == null || LUser.LoginName == "Anonymous" ? "" : "&prompt=consent")
                    + "&state=" + System.Web.HttpUtility.UrlEncode(state);
            Session[state] = new Dictionary<string, string>() {
                {"Provider","Azure" },
                {"ReturnUrl",Request.QueryString["ReturnUrl"]}
            };
            Response.Redirect(loginUrl);
        }

        public void CancelLoginBtn_Click(object sender, System.EventArgs e)
        {
            this.Redirect("~/MyAccount.aspx?typ=" + Request.QueryString["typ"].ToString().ToUpper());
        }

        public void PickLoginBtn_Click(object sender, System.EventArgs e)
        {
            SSOLogin(cLoginNameChoice.SelectedValue, cProviderLoginName.Text, cProvider.Text);
        }

        public void LoginBtn_Click(object sender, System.EventArgs e)
        {
            int attempts = 0;
            int.TryParse(cLoginAttempts.Text, out attempts);
            DataTable dtLabel = GetLabels();

            if (attempts >= 2 && cMathAnswer.Text.Trim() != cMathExpectedAnswer.Text.Trim())
            {
                PreMsgPopup(TranslateItem(dtLabel.Rows, "WrongMathAnswerMsg")); return;
            }

            LoginMathPanel.Visible = false;
            bool bBlocked = !string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]);
            bool success = LoginUser(cLoginName.Text, cUsrPassword.Text, bBlocked || string.IsNullOrEmpty(Request.QueryString["typ"]),
                () => { PreMsgPopup(TranslateItem(GetLabels().Rows, "AccountLockedMsg")); },
                () =>
                {
                    cLoginAttempts.Text = (attempts + 1).ToString();
                    if (attempts >= 2)
                    {
                        LoginMathPanel.Visible = true;
                        CreateMathCapcha();
                    }
                    cUsrPassword.Focus();
                    PreMsgPopup(TranslateItem(GetLabels().Rows, "LoginFailedMsg"));
                    ShowLoginPanel();
                });
            if (success && !LUser.TwoFactorAuth && Config.EnableTwoFactorAuth == "M")
            {
                //this.Redirect(Request.Path);
                this.Redirect("~/MyAccount.aspx?typ=N");
            }
            else if (success && !bBlocked && !string.IsNullOrEmpty(Request.QueryString["typ"]))
            {
                cRedirectParent.Value = Config.SslUrl;
            }
            else if (success && bBlocked && IsProxy())
            {
                this.Redirect(Request.QueryString["ReturnUrl"]);
            }
        }
        public void ForgetBtn_Click(object sender, System.EventArgs e)
        {
            ShowResetPwdPanel();
        }

        public void ResetPwdBtn_Click(object sender, System.EventArgs e)
        {
            DataTable dt = (new LoginSystem()).GetSaltedUserInfo(0, cResetLoginName.Text, cResetUsrEmail.Text);
            DataTable dtLabel = GetLabels();
            /* it is possible that there is multiple user login returned
            * 2013.12.24 gary
             */
            System.Collections.Generic.List<string> popMsg = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                if (dt.Rows.Count > 0 && !string.IsNullOrEmpty(dt.Rows[0]["UsrEmail"].ToString()))
                {
                    string loginName = dr["LoginName"].ToString();
                    string userEmail = dr["UsrEmail"].ToString();
                    var reset_url = GetResetLoginUrl(dr["UsrId"].ToString(), "", "", "", "", null, null);
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.Append(TranslateItem(dtLabel.Rows, "ResetPwdEmailMsg1"));
                    sb.Append("<br /><strong><a href=\"").Append(reset_url.Value).Append("\">").Append(reset_url.Value).Append("</a></strong><br /><br />");
                    sb.Append("<strong>").Append(loginName).Append("</strong>&nbsp;");
                    sb.Append(TranslateItem(dtLabel.Rows, "ResetPwdEmailMsg2"));
                    sb.Append("<br/>" + loginName.Left(3) + "****" + loginName.Right(3) + "<br/>");
                    string host = XHost();
                    try
                    {
                        string from = base.SysCustServEmail(3);
                        base.SendEmail(string.Format(TranslateItem(dtLabel.Rows, "ResetPwdEmailSubject"), host, Config.WebTitle), sb.ToString(), userEmail, from, from, Config.WebTitle + " Customer Care", true);
                        System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex("^(.)([^\\@]*)(@)(.*)$");
                        var shieldedEmail = re.Replace(userEmail, (m) => m.Groups[1].Captures[0] + "xxxxxxx" + "@" + m.Groups[4]);
                        popMsg.Add(string.Format(TranslateItem(dtLabel.Rows, "ResetEmailSentMsg"), shieldedEmail));

                    }
                    catch (Exception err) { PreMsgPopup(err.Message.Replace(Environment.NewLine, "<br/>")); return; }
                }
                else
                {
                    // we are here because login name is used for reset request
                    PreMsgPopup(TranslateItem(dtLabel.Rows, "NoEmailOnFileMsg")); return;
                }
            }
            if (popMsg.Count > 0)
            {
                PreMsgPopup(string.Join("\n", popMsg.ToArray()));
            }
        }

        public void UpdateProfileBtn_Click(object sender, System.EventArgs e)
        {
            if (LUser != null && LUser.LoginName.ToLower() != "anonymous")
            {
                Credential cr = new Credential(LUser.LoginName, cUsrPasswordVerify.Text);
                if ((new LoginSystem()).GetLoginSecure(cr) == null && !LUser.OTPValidated)
                {
                    PreMsgPopup(TranslateItem(GetLabels().Rows, "PasswordVerificationFailed")); return;
                }
                DataTable dt = (new AdminSystem()).GetMstById("GetAdmUsr1ById", LUser.UsrId.ToString(), null, null);
                if (dt.Rows.Count == 1)
                {
                    try
                    {
                        string oldEmail = LUser.UsrEmail;
                        (new LoginSystem()).UpdUserLoginInfo(LUser.UsrId, cNewLoginName.Text, cNewUsrName.Text, cNewUsrEmail.Text);
                        cMsg.Text = TranslateItem(GetLabels().Rows, "LoginInfoChangedMsg"); cMsgDiv.Visible = true;
                        LUser.LoginName = cNewLoginName.Text;
                        LUser.UsrEmail = cNewUsrEmail.Text;
                        LUser.UsrName = cNewUsrName.Text;
                        if (cHome.Visible)
                        {
                            string script =
                            @"<script type='text/javascript' language='javascript'>
			            Sys.Application.add_load(function () {setTimeout(function() { try { window.location = '" + Config.SslUrl + @"';} catch (er) {};}, 5000);});
			            </script>";
                            ScriptManager.RegisterStartupScript(cMsgContent, typeof(Label), "AutoRedirect", script, false);
                        }
                        cUpdProfileBtn.Visible = false;
                        TwoFactorAuthenticationPanel.Visible = false;
                        NewPwdPanel.Visible = false;
                        try
                        {
                            string from = base.SysCustServEmail(3);
                            string subject = TranslateItem(GetLabels().Rows, "ProfileChangedEmailSubject");
                            string body = TranslateItem(GetLabels().Rows, "ProfileChangedEmailBody");
                            string site = GetSiteUrl(true);
                            if (!string.IsNullOrEmpty(oldEmail))
                            {
                                NotifyUser(subject, body, oldEmail, from);
                            }
                            else if (LUser.TwoFactorAuth)
                            {
                                string sysadminEmail = base.SysAdminEmail(3);
                                string admBody = string.Format(TranslateItem(GetLabels().Rows, "ProfileChangedEmailBodyForAdmin"), LUser.LoginName);
                                NotifyUser(subject, body, sysadminEmail, from);
                            }
                        }
                        catch { }
                    }
                    catch (Exception err)
                    {
                        PreMsgPopup(err.Message); return;
                    }
                }
            }
        }

        public void UpdPwdBtn_Click(object sender, System.EventArgs e)
        {
            string resetLoginName = ResetRequestLoginName();
            bool bIsNotLogin = true;

            if (cConfirmPwd.Text.Trim() != cNewUsrPassword.Text.Trim())
            {
                PreMsgPopup("Please match your passwords and try again!"); return;

            }
            foreach (var re in Config.PasswordComplexity.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!new System.Text.RegularExpressions.Regex("^.*" + re.Trim() + ".*$").IsMatch(cNewUsrPassword.Text.Trim()))
                {
                    if (string.IsNullOrEmpty(Config.PasswordHelpMsg)) PreMsgPopup("Password chosen is too simple, try again");
                    else PreMsgPopup(Config.PasswordHelpMsg); return;
                }
            }

            if (((LUser != null && LUser.LoginName.ToLower() != "anonymous") || !string.IsNullOrEmpty(resetLoginName)) && cNewUsrPassword.Text == cConfirmPwd.Text && !string.IsNullOrEmpty(cNewUsrPassword.Text))
            {
                bool bIsPasswordExpired = PasswordExpired(LUser);

                if (LUser != null && LUser.LoginName.ToLower() != "anonymous" && !bIsPasswordExpired)
                {
                    Credential cr1 = new Credential(LUser.LoginName, cUsrPasswordVerify.Text);
                    if ((new LoginSystem()).GetLoginSecure(cr1) == null && cHardenedLogin.Text != "Y")
                    {
                        PreMsgPopup("Please enter a correct password."); return;
                    }
                    resetLoginName = LUser.LoginName;
                    bIsNotLogin = false;
                }
                else
                {
                    bIsNotLogin = !bIsPasswordExpired;
                }

                Credential cr = new Credential(resetLoginName ?? LUser.LoginName, cNewUsrPassword.Text.Trim());
                try
                {
                    if ((new LoginSystem()).UpdUsrPassword(cr, LUser, !bIsPasswordExpired && cRemoveLinkedLogin.Checked))
                    {
                        if (bIsNotLogin)
                        {
                            bool success = LoginUser(resetLoginName, cNewUsrPassword.Text, false, () => { }, () => { });
                            if (success)
                            {
                                NewPwdPanel.Visible = false;
                                cMsg.Text = TranslateItem(GetLabels().Rows, "PasswordChangedMsg"); cMsgDiv.Visible = true;
                                string returnUrl = Request.QueryString["ReturnUrl"];
                                UserProfilePanel.Visible = true; cHome.Visible = (Request.QueryString["typ"] ?? "").ToUpper() != "E";
                                string loginUrl = System.Web.Security.FormsAuthentication.LoginUrl;
                                if (string.IsNullOrEmpty(loginUrl)) loginUrl = "MyAccount.aspx";
                                cHome.NavigateUrl = Request.QueryString["j"] != null ? "~/" + loginUrl : (Config.SslUrl.StartsWith("http") ? Config.SslUrl : "~/" + Config.SslUrl);
                                if (cHome.Visible)
                                {
                                    string script =
                                    @"<script type='text/javascript' language='javascript'>
			            Sys.Application.add_load(function () {setTimeout(function() { try { window.location = '" + (!string.IsNullOrEmpty(returnUrl) ? returnUrl : Config.SslUrl) + @"';} catch (er) {};}, 3000);});
			            </script>";
                                    ScriptManager.RegisterStartupScript(cMsgContent, typeof(Label), "AutoRedirect", script, false);
                                }
                            }
                        }
                        else
                        {
                            LUser.PwdChgDt = DateTime.Now;
                            cPwdExpMsg.Text = "";
                            cMsg.Text = TranslateItem(GetLabels().Rows, "PasswordChangedMsg"); cMsgDiv.Visible = true;
                            string returnUrl = Request.QueryString["ReturnUrl"];
                            UserProfilePanel.Visible = true; cHome.Visible = (Request.QueryString["typ"] ?? "").ToUpper() != "E";
                            string loginUrl = System.Web.Security.FormsAuthentication.LoginUrl;
                            if (string.IsNullOrEmpty(loginUrl)) loginUrl = "MyAccount.aspx";
                            cHome.NavigateUrl = Request.QueryString["j"] != null ? "~/" + loginUrl : (Config.SslUrl.StartsWith("http") ? Config.SslUrl : "~/" + Config.SslUrl);
                            if (cHome.Visible || (!string.IsNullOrEmpty(returnUrl) && bIsPasswordExpired))
                            {
                                string script =
                                @"<script type='text/javascript' language='javascript'>
			            Sys.Application.add_load(function () {setTimeout(function() { try { window.location = '" + (!string.IsNullOrEmpty(returnUrl) ? returnUrl : Config.SslUrl) + @"';} catch (er) {};}, 3000);});
			            </script>";
                                ScriptManager.RegisterStartupScript(cMsgContent, typeof(Label), "AutoRedirect", script, false);
                            }
                        }
                        NewPwdPanel.Visible = false;
                        TwoFactorAuthenticationPanel.Visible = false;
                        cUpdProfileBtn.Visible = false;
                        try
                        {
                            string from = base.SysCustServEmail(3);
                            string subject = TranslateItem(GetLabels().Rows, "PasswordChangedEmailSubject");
                            string body = TranslateItem(GetLabels().Rows, "PasswordChangedEmailBody");
                            if (!string.IsNullOrEmpty(LUser.UsrEmail))
                            {
                                NotifyUser(subject, body, LUser.UsrEmail, from);
                            }
                        }
                        catch { }
                    }
                }
                catch (Exception err) { PreMsgPopup(err.Message); return; }
            }
        }

        protected void cLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (LUser != null && LUser.LoginName.ToLower() != "anonymous")
            {
                if (!string.IsNullOrEmpty(cLang.SelectedValue))
                {
                    base.LUser.CultureId = short.Parse(cLang.SelectedValue);
                    base.LUser.Culture = (new AdminSystem()).SetCult(base.LUser.UsrId, base.LUser.CultureId);
                    base.LImpr = null; SetImpersonation(LUser.UsrId);
                    base.VMenu = new DataView((new MenuSystem()).GetMenu(base.LUser.CultureId, base.LCurr.SystemId, base.LImpr, base.SysConnectStr(base.LCurr.SystemId), base.AppPwd(base.LCurr.SystemId), null, null, null));
                    SetCultureId1(cLang, LUser.CultureId.ToString());
                    TranslateItems();
                }
            }
        }

        private void CreateMathCapcha()
        {
            Random rdm = new Random((int)(DateTime.Now.Ticks & 0xffffffff));
            int firstterm = rdm.Next(1, 11);
            int secondterm = rdm.Next(1, 11);
            int plusorminus = rdm.Next(0, 2);
            if (plusorminus == 0) { lit_math_plusminus.Text = firstterm + " + " + secondterm; cMathExpectedAnswer.Text = (firstterm + secondterm).ToString(); }
            else if (firstterm > secondterm) { lit_math_plusminus.Text = firstterm + " - " + secondterm; cMathExpectedAnswer.Text = (firstterm - secondterm).ToString(); }
            else { lit_math_plusminus.Text = secondterm + " - " + firstterm; cMathExpectedAnswer.Text = (secondterm - firstterm).ToString(); }
            cMathAnswer.Text = "";
        }

        private bool PasswordExpired(LoginUsr usr)
        {
            return (usr != null
                && usr.LoginName.ToLower() != "anonymous"
                && string.IsNullOrEmpty(LUser.Provider)
                && (usr.PwdChgDt == null || (usr.PwdDuration == 0 ? false : (DateTime.Today > usr.PwdChgDt.Value.AddDays(usr.PwdDuration)))));
        }

        private void PreMsgPopup(string msg)
        {
            int MsgPos = msg.IndexOf("RO.SystemFramewk.ApplicationAssert");
            string iconUrl = "images/warning.gif";
            string focusOnCloseId = string.Empty;
            string msgContent = ReformatErrMsg(msg);
            if (MsgPos >= 0 && LUser.TechnicalUsr != "Y") { msgContent = ReformatErrMsg(msg.Substring(0, MsgPos - 3)); }
            string script =
            @"<script type='text/javascript' language='javascript'>
			PopDialog('" + iconUrl + "','" + msgContent.Replace("'", @"\'") + "','" + focusOnCloseId + @"');
			</script>";
            ScriptManager.RegisterStartupScript(cMsgContent, typeof(Label), "Popup", script, false);
        }
        private DataTable GetLabels()
        {
            DataTable dtLabel = (new AdminSystem()).GetLabels(LUser != null && LUser.LoginName.ToLower() != "anonymous" ? LUser.CultureId : (short)1, "MyAccountModule", null, null, null);
            DataColumn[] pkey = new DataColumn[1];
            pkey[0] = dtLabel.Columns[0];
            dtLabel.PrimaryKey = pkey;
            return dtLabel;
        }
        private void TranslateItems()
        {
            cPwdHlpMsgLabel.Text = Config.PasswordHelpMsg ?? string.Empty;
            DataTable dtLabel = GetLabels();
            AdminSystem adm = new AdminSystem();
            if (dtLabel != null)
            {
                TranslateItem(cLoginNameLabel, dtLabel.Rows, "cLoginName");
                TranslateItem(cUsrPasswordLabel, dtLabel.Rows, "cUsrPassword");
                TranslateItem(cForgetBtn, dtLabel.Rows, "cForgetBtn");
                TranslateItem(cLoginBtn, dtLabel.Rows, "cLoginBtn");
                TranslateItem(cResetLoginNameLabel, dtLabel.Rows, "cResetLoginName");
                TranslateItem(cResetOrLabel, dtLabel.Rows, "cResetOr");
                TranslateItem(cResetUsrEmailLabel, dtLabel.Rows, "cResetUsrEmail");
                TranslateItem(cResetPwdBtn, dtLabel.Rows, "cResetPwdBtn");
                TranslateItem(cNewLoginNameLabel, dtLabel.Rows, "cNewLoginName");
                TranslateItem(cNewUserNameLabel, dtLabel.Rows, "cNewUserName");
                TranslateItem(cNewUsrEmailLabel, dtLabel.Rows, "cNewUsrEmail");
                TranslateItem(cNewUsrPasswordLabel, dtLabel.Rows, "cNewUsrPassword");
                TranslateItem(cConfirmPwdLabel, dtLabel.Rows, "cConfirmPwd");
                TranslateItem(cTermsOfServiceLink, dtLabel.Rows, "cTermsOfService");
                TranslateItem(cUpdPwdBtn, dtLabel.Rows, "cUpdPwdBtn");
                TranslateItem(cUpdProfileBtn, dtLabel.Rows, "cUpdProfileBtn");
                TranslateItem(cUsrPasswordVerifyLabel, dtLabel.Rows, "cUsrPasswordVerify");
                TranslateItem(cLoginHumanLabel, dtLabel.Rows, "cHumanLogin");
                TranslateItem(PwdTitle, dtLabel.Rows, "PwdTitle");
                TranslateItem(NewPwdTitle, dtLabel.Rows, "NewPwdTitle");
                TranslateItem(LoginAreaTitle, dtLabel.Rows, "LoginAreaTitle");
                TranslateItem(RestPwdTitle, dtLabel.Rows, "RestPwdTitle");
                TranslateItem(cWarnMsg1, dtLabel.Rows, "cWarnMsg1");
                TranslateItem(cWarnMsg2, dtLabel.Rows, "cWarnMsg2");
                TranslateItem(cCancelAccountLn, dtLabel.Rows, "cCancelAccountLn");
                TranslateItem(cCancelAccountMsg, dtLabel.Rows, "cCancelAccountMsg");
                TranslateItem(GoogleLoginTitle, dtLabel.Rows, "GoogleLoginTitle");
                TranslateItem(cDisableTwoFactor, dtLabel.Rows, "cDisableTwoFactor");
                TranslateItem(cResetTwoFactorKey, dtLabel.Rows, LUser != null && LUser.TwoFactorAuth ? "cResetTwoFactorKey" : (cTwoFactorSecretCode.Visible ? "cResentTwoFactorKey" : "cEnableTwoFactorKey"));
                TranslateItem(cShowTwoFactorKey, dtLabel.Rows, "cShowTwoFactorKey");
                TranslateItem(cTwoFactorSecretKeyHelp, dtLabel.Rows, "cTwoFactorSecretKeyHelp");
                TranslateItem(cTwoFactorSecretKeyLabel, dtLabel.Rows, "cTwoFactorSecretKeyLabel");
                TranslateItem(cTwoFactorSecretCodeHelp, dtLabel.Rows, "cTwoFactorSecretCodeHelp");
                TranslateItem(cTwoFactorSecretCodeLabel, dtLabel.Rows, "cTwoFactorSecretCodeLabel");
                TranslateItem(cOTPAccessCodeHelp, dtLabel.Rows, "cOTPAccessCodeHelp");
                TranslateItem(cOTPAccessCodeLabel, dtLabel.Rows, "cOTPAccessCodeLabel");
                TranslateItem(cSendOTPAccessCodeBtn, dtLabel.Rows, "cSendOTPAccessCodeBtn");
                TranslateItem(cRememberOTPAccessCode, dtLabel.Rows, "cRememberOTPAccessCode");
                TranslateItem(cForgetOTPCache, dtLabel.Rows, "cForgetOTPAccessCode");
                TranslateItem(cRemoveLinkedLogin, dtLabel.Rows, "cRemoveLinkedLogin");
                TranslateLoginLinkBtns(dtLabel);
                if (LUser != null && LUser.LoginName.ToLower() != "anonymous")
                {
                    cPwdExpMsg.Text = (new LoginSystem()).GetPwdExpMsg(base.LUser.UsrId.ToString(), base.LUser.CultureId.ToString(), Config.PwdExpDays);
                    cPwdExpiryDtLbl.Text = adm.GetMsg("{66}", base.LUser.CultureId, "N", string.Empty, string.Empty);
                    cPwdExpiryDt.Text = LUser.PwdDuration == 0 ? DateTime.Parse("9999.12.31").ToShortDateString() : (LUser.PwdChgDt.HasValue ? LUser.PwdChgDt.Value.AddDays(LUser.PwdDuration).ToShortDateString() : DateTime.Now.AddDays(-1).ToShortDateString());
                    cLastPwdChgDtLbl.Text = adm.GetMsg("{67}", base.LUser.CultureId, "N", string.Empty, string.Empty);
                    if (LUser.PwdChgDt.HasValue) { cLastPwdChgDt.Text = LUser.PwdChgDt.Value.ToShortDateString(); }
                    if (DateTime.Parse(cPwdExpiryDt.Text).AddDays(-LUser.PwdWarn) <= DateTime.Now)
                    {
                        cPwdExpiryDt.Style.Add("color", "#ffc200"); cPwdExpiryDt.Style.Add("font-size", "14px");
                    }
                    if (PasswordExpired(LUser))		// First time login or paassword expired.
                    {
                        cPwdExpMsg.Text = adm.GetMsg("{68}", base.LUser.CultureId, "N", string.Empty, string.Empty);
                        cPwdExpiryDt.Style["color"] = "red"; cPwdExpiryDt.Style.Add("font-size", "14px");
                    }
                }
                if (!string.IsNullOrEmpty(Request.QueryString["logo"]) && Request.QueryString["logo"].ToLower() == "n")
                {
                    cLoginImage.Visible = false;
                }
                else
                {
                    cLoginImage.Visible = true; cLoginImage.ImageUrl = Config.LoginImage;
                }
            }
        }
        private bool SendOTPAccessCode()
        {
            string secret = Guid.NewGuid().ToString();
            string code = RO.Common3.GoogleAuthenticator.CalculateOneTimePassword(System.Text.UTF8Encoding.UTF8.GetBytes(secret), 0);
            cTwoFactorSecretCode.EncryptionKey = secret;
            cTwoFactorSecretCode.TimeSkew = 10;
            string from = base.SysCustServEmail(3);
            string subject = TranslateItem(GetLabels().Rows, "OTPAccessCodeEmailSubject");
            string body = TranslateItem(GetLabels().Rows, "OTPAccessCodeEmailBody");
            if (!string.IsNullOrEmpty(LUser.UsrEmail))
            {
                TwoFactorSecretCodePanel.Visible = true;
                NotifyUser(subject, string.Format(body, code), LUser.UsrEmail, from);
                return true;
            }
            else return false;
        }

        protected void cDisableTwoFactor_Click(object sender, EventArgs e)
        {
            string from = base.SysCustServEmail(3);
            string subject = TranslateItem(GetLabels().Rows, "OTPDisableEmailSubject");
            string body = TranslateItem(GetLabels().Rows, "OTPDisableEmailBody");
            if (!string.IsNullOrEmpty(LUser.UsrEmail))
            {
                NotifyUser(subject, body, LUser.UsrEmail, from);
                cTwoFactorSecretQRCode.ImageUrl = "";
                cTwoFactorSecretKey.Text = "";
                TwoFactorSecretPanel.Visible = false;
                (new LoginSystem()).WrSetUsrOTPSecret(LUser.UsrId, false);
                LUser.TwoFactorAuth = false;
                cDisableTwoFactor.Visible = false;
                cResetTwoFactorKey.Visible = true;
                cShowTwoFactorKey.Visible = false;
                
                TranslateItem(cResetTwoFactorKey, GetLabels().Rows, "cEnableTwoFactorKey");
            }
            else
            {
                PreMsgPopup(TranslateItem(GetLabels().Rows, "OTPRequiredEmail"));
            }

        }
        protected void cResetTwoFactorKey_Click(object sender, EventArgs e)
        {
            if (!TwoFactorSecretPanel.Visible)
            {
                cChangeTwoFactoryKey.Checked = true;
                bool codeSent = SendOTPAccessCode();
                if (codeSent)
                {
                    cShowTwoFactorKey.Visible = false;
                    cTwoFactorSecretQRCode.ImageUrl = "";
                    cTwoFactorSecretKey.Text = "";
                    TwoFactorSecretPanel.Visible = false;
                    if (!LUser.TwoFactorAuth) TranslateItem(cResetTwoFactorKey, GetLabels().Rows, "cResentTwoFactorKey");
                    PreMsgPopup(TranslateItem(GetLabels().Rows, "OTPAccessCodeSent"));
                }
                else
                {
                    PreMsgPopup(TranslateItem(GetLabels().Rows, "OTPRequiredEmail"));
                }
            }
        }

        protected void cShowTwoFactorKey_Click(object sender, EventArgs e)
        {
            if (!TwoFactorSecretPanel.Visible)
            {
                cChangeTwoFactoryKey.Checked = false;
                bool codeSent = SendOTPAccessCode();
                if (codeSent)
                {
                    cTwoFactorSecretQRCode.ImageUrl = "";
                    cTwoFactorSecretKey.Text = "";
                    TwoFactorSecretPanel.Visible = false;
                    if (!LUser.TwoFactorAuth) TranslateItem(cShowTwoFactorKey, GetLabels().Rows, "cResentTwoFactorKey");
                    PreMsgPopup(TranslateItem(GetLabels().Rows, "OTPAccessCodeSent"));
                }
                else
                {
                    PreMsgPopup(TranslateItem(GetLabels().Rows, "OTPRequiredEmail"));
                }
            }
        }

        protected void cForgetOTPCache_Click(object sender, EventArgs e)
        {
            ForgetOTP();
            cForgetOTPCache.Visible = false;
        }

        protected void cTwoFactorSecretCode_TextChanged(object sender, EventArgs e)
        {
            bool bIsCodeMatch = cTwoFactorSecretCode.IsValid;

            if (bIsCodeMatch)
            {
                string site = GetSiteUrl(true) + " " + DateTime.Now.ToString("yyyy/MM/dd");
                string from = base.SysCustServEmail(3);
                string subject = TranslateItem(GetLabels().Rows, "OTPEnableEmailSubject");
                string body = TranslateItem(GetLabels().Rows, cChangeTwoFactoryKey.Checked ? "OTPEnableEmailBody" : "OTPRetrieveEmailBody");
                NotifyUser(subject, body, LUser.UsrEmail, from);
                LoginSystem login = new LoginSystem();
                string secret = cChangeTwoFactoryKey.Checked ? login.WrSetUsrOTPSecret(LUser.UsrId, true) : login.WrGetUsrOTPSecret(LUser.UsrId);
                cTwoFactorSecretQRCode.ImageUrl = RO.Common3.GoogleAuthenticator.GetQRCodeEmbeddedImg(site, System.Text.UTF8Encoding.UTF8.GetBytes(secret), 0);
                cTwoFactorSecretKey.Text = RO.Common3.GoogleAuthenticator.GetSecretCode(System.Text.UTF8Encoding.UTF8.GetBytes(secret));
                cTwoFactorSecretCode.EncryptionKey = "";
                cTwoFactorSecretCode.Text = "";
                TwoFactorSecretCodePanel.Visible = false;
                TwoFactorSecretPanel.Visible = true;
                LUser.TwoFactorAuth = true;
                LUser.OTPValidated = true;
                cDisableTwoFactor.Visible = true;
                cResetTwoFactorKey.Visible = false;
                cShowTwoFactorKey.Visible = false;
                TranslateItem(cResetTwoFactorKey, GetLabels().Rows, "cResetTwoFactorKey");
                TranslateItem(cShowTwoFactorKey, GetLabels().Rows, "cShowTwoFactorKey");
                TranslateItem(cDisableTwoFactor, GetLabels().Rows, "cDisableTwoFactor");

            }
        }
        protected void cOTPAccessCode_TextChanged(object sender, EventArgs e)
        {
            LUser.OTPValidated = LUser.OTPValidated || cOTPAccessCode.IsValid;
            if (LUser.OTPValidated)
            {
                if (cRememberOTPAccessCode.Checked) RememberOTP();
                ShowProfilePanel(false);
            }
            else
            {
                PreMsgPopup(TranslateItem(GetLabels().Rows, "OTPAccessCodeIncorrectMsg"));
            }
            cOTPAccessCode.Text = "";
        }
        protected void cSendOTPAccessCodeBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(LUser.UsrEmail))
            {
                string secret = (new LoginSystem()).WrGetUsrOTPSecret(LUser.UsrId);
                string accessCode = RO.Common3.GoogleAuthenticator.CalculateOneTimePassword(System.Text.UTF8Encoding.UTF8.GetBytes(secret));
                cOTPAccessCode.TimeSkew = 10;
                string subject = TranslateItem(GetLabels().Rows, "OTPAccessCodeEmailSubject");
                string body = TranslateItem(GetLabels().Rows, "OTPAccessCodeEmailBody");
                string from = base.SysCustServEmail(3);
                NotifyUser(subject, string.Format(body, accessCode), LUser.UsrEmail, from);
                PreMsgPopup(TranslateItem(GetLabels().Rows, "OTPAccessCodeSent"));
            }
            else
            {
                PreMsgPopup(TranslateItem(GetLabels().Rows, "OTPRequiredEmail"));
            }
        }

        protected void cCodeVerifyBtn_Click(object sender, EventArgs e)
        {
           
        }

        protected string OTPComponents()
        {
            string secret = new LoginSystem().WrGetUsrOTPSecret(LUser.UsrId);
            System.Security.Cryptography.HMACSHA256 hmac = new System.Security.Cryptography.HMACSHA256(System.Text.UTF8Encoding.UTF8.GetBytes(secret));

            // the more components, the safer it is from brute force attack
            // ideally some form of 'server side'(global reset) + client side(more variation)
            
            return Convert.ToBase64String(hmac.ComputeHash(System.Text.UTF8Encoding.UTF8.GetBytes(
                    ""
                    + Config.DesPassword // change would result in global reset
                    + LUser.LoginName
//                    + Request.UserAgent
                    )));
        }
        protected void RememberOTP(int days = 90)
        {
            Dictionary<string, string> otp = new Dictionary<string, string>();

            otp["for"] = OTPComponents();
            SetSecureCookie("otp" + LUser.UsrId.ToString(), otp, 3600 * 24 * days);

        }
        protected void ForgetOTP()
        {
            Dictionary<string, string> otp = new Dictionary<string, string>();

            otp["for"] = Guid.NewGuid().ToString();
            SetSecureCookie("otp" + LUser.UsrId.ToString(), otp, 0);

        }
        protected bool VerifyRememberedOTP()
        {
            try
            {
                Dictionary<string, string> otp = GetSecureCookie("otp" + LUser.UsrId.ToString());
                return LUser.TwoFactorAuth
                    && OTPComponents() == otp["for"];
            }
            catch { return false; }
        }

        protected void NotifyUser(string subject, string body, string email, string from)
        {
            string site = GetSiteUrl(true);
            string requestIPChain = string.Format("\r\n\r\nRequest Source IP info : [{0}]\r\n", string.Join(",", GetClientIpChain().ToArray()));
            SendEmail(subject + " " + site, body + requestIPChain, email, from, "", Config.WebTitle + " Customer Care", false);
        }

        protected void TranslateLoginLinkBtns(DataTable dtLabel)
        {
            string facebookLogin = "";
            string googleLogin = "";
            string microsoftLogin = "";
            string windowsLogin = "";
            foreach (var dr in LinkedUserLogin)
            {
                if (dr["ProviderCd"].ToString().Trim() == "F") facebookLogin = dr["LoginName"].ToString();
                else if (dr["ProviderCd"].ToString().Trim() == "G") googleLogin = dr["LoginName"].ToString();
                else if (dr["ProviderCd"].ToString().Trim() == "O") microsoftLogin = dr["LoginName"].ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0];
                else if (dr["ProviderCd"].ToString().Trim() == "M") microsoftLogin = dr["LoginName"].ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0];
                else if (dr["ProviderCd"].ToString().Trim() == "W") windowsLogin = dr["LoginName"].ToString();
            }
            if (string.IsNullOrEmpty(facebookLogin)) TranslateItem(cLinkFacebookBtn, dtLabel.Rows, "cLinkFacebookBtn");
            else TranslateItem(cLinkFacebookBtn, dtLabel.Rows, "cUnlinkFacebookBtn",new object[]{facebookLogin});
            if (string.IsNullOrEmpty(googleLogin)) TranslateItem(cLinkGoogleBtn, dtLabel.Rows, "cLinkGoogleBtn");
            else TranslateItem(cLinkGoogleBtn, dtLabel.Rows, "cUnlinkGoogleBtn", new object[] { googleLogin });
            if (string.IsNullOrEmpty(microsoftLogin)) TranslateItem(cLinkMicrosoftBtn, dtLabel.Rows, "cLinkMicrosoftBtn");
            else TranslateItem(cLinkMicrosoftBtn, dtLabel.Rows, "cUnlinkMicrosoftBtn", new object[] { microsoftLogin });
            if (string.IsNullOrEmpty(windowsLogin)) TranslateItem(cLinkWindowsBtn, dtLabel.Rows, "cLinkWindowsBtn");
            else TranslateItem(cLinkWindowsBtn, dtLabel.Rows, "cUnlinkWindowsBtn", new object[] { windowsLogin });

        }

        protected void LinkUserLogin(int UsrId, string ProviderCd, string LoginName, string LoginMeta = null)
        {
            Dictionary<string, string> providers = new Dictionary<string, string>{
                {"F","Facebook"},
                {"G","Google"},
                {"M","Microsoft"},
                {"O","Office 365/Azure"},
                {"W","Windows"},
                {"Fido2","WebAuthn/Fido2"},
                {"Eth1","Eth1 Wallet"}
                };
            new LoginSystem().LinkUserLogin(UsrId, ProviderCd, LoginName, LoginMeta);
            LinkedUserLogin = new LoginSystem().GetLinkedUserLogin(LUser.UsrId).AsEnumerable().ToList();
            if ((from dr in LinkedUserLogin where "F".IndexOf(dr["ProviderCd"].ToString().Trim()) >= 0 select dr).Count() > 0) cLinkFacebookBtn.OnClientClick = "";
            if ((from dr in LinkedUserLogin where "G".IndexOf(dr["ProviderCd"].ToString().Trim()) >= 0 select dr).Count() > 0) cLinkGoogleBtn.OnClientClick = "";

            TranslateLoginLinkBtns(GetLabels());
            cRemoveLinkedLogin.Visible = LinkedUserLogin.Count > 0;

            try
            {
                string from = base.SysCustServEmail(3);
                string subject = TranslateItem(GetLabels().Rows, "ProfileChangedEmailSubject");
                string body = string.Format(TranslateItem(GetLabels().Rows, "LoginLinkAddedEmailBody"), LUser.LoginName, providers[ProviderCd], LoginName);
                string site = GetSiteUrl(true);
                if (!string.IsNullOrEmpty(LUser.UsrEmail))
                {
                    NotifyUser(subject, body, LUser.UsrEmail, from);
                }
            }
            catch { }
        }
        protected void UnlinkUserLogin(int UsrId, string ProviderCd, string LoginName)
        {
            Dictionary<string, string> providers = new Dictionary<string, string>{
                {"F","Facebook"},
                {"G","Google"},
                {"M","Microsoft"},
                {"O","Office 365/Azure"},
                {"W","Windows"},
                {"Fido2","WebAuthn/Fido2"}
                };
            new LoginSystem().UnlinkUserLogin(UsrId, ProviderCd, LoginName);
            LinkedUserLogin = new LoginSystem().GetLinkedUserLogin(LUser.UsrId).AsEnumerable().ToList();
            if ((from dr in LinkedUserLogin where "F".IndexOf(dr["ProviderCd"].ToString().Trim()) >= 0 select dr).Count() == 0) cLinkFacebookBtn.OnClientClick = "FacebookSignIn('" + Config.FacebookAppId + "','" + cFacebookAccessToken.ClientID + "','" + cFacebookLoginBtn.ClientID + "');return false;";
            if ((from dr in LinkedUserLogin where "G".IndexOf(dr["ProviderCd"].ToString().Trim()) >= 0 select dr).Count() == 0) cLinkGoogleBtn.OnClientClick = "GoogleSignIn('" + Config.GoogleClientId + "','" + cGoogleAccessToken.ClientID + "','" + cGoogleLoginBtn.ClientID + "');return false;";
            TranslateLoginLinkBtns(GetLabels());
            cRemoveLinkedLogin.Visible = LinkedUserLogin.Count > 0;

            try
            {
                string from = base.SysCustServEmail(3);
                string subject = TranslateItem(GetLabels().Rows, "ProfileChangedEmailSubject");
                string body = string.Format(TranslateItem(GetLabels().Rows, "LoginLinkRemovedEmailBody"), providers[ProviderCd], LoginName, LUser.LoginName);
                string site = GetSiteUrl(true);
                if (!string.IsNullOrEmpty(LUser.UsrEmail))
                {
                    NotifyUser(subject, body, LUser.UsrEmail, from);
                }
            }
            catch { }
        }
        protected void cLinkEth1WalletBtn_Click(object sender, EventArgs e)
        {
            string sig = cWebAuthnResult.Text;
            if (!string.IsNullOrEmpty(sig))
            {
                var signer = new Nethereum.Signer.EthereumMessageSigner();
                byte[] message = System.Text.UTF8Encoding.UTF8.GetBytes(cEth1RegistrationRequest.Text);
                string signerAddress = signer.EcRecover(message, sig);
                LinkUserLogin(LUser.UsrId, "Eth1", signerAddress);
                PreMsgPopup(
                    string.Format("Eth1 Mobile Wallet/MetaMask registration successful with id {0}"
                        , signerAddress
                        )
                    );

            }
        }

        protected void cLinkWebAuthnBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (LUser != null && LUser.LoginName != "Anonymous")
                {
                    var aaguidLookup = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase){
                    {"6028B017-B1D4-4C02-B4B3-AFCDAFC96BB2","Windows Hello software authenticator"},
                    {"6E96969E-A5CF-4AAD-9B56-305FE6C82795","Windows Hello VBS software authenticator"},
                    {"08987058-CADC-4B81-B6E1-30DE50DCBE96","Windows Hello TPM authenticator"},
                    {"9DDD1817-AF5A-4672-A2B9-3E3DD95000A9","Windows Hello VBS hardware authenticator"},
                    {"cb69481e-8ff7-4039-93ec-0a2729a154a8","YubiKey 5 5.1 all variance"},
                    {"ee882879-721c-4913-9775-3dfcce97072a","YubiKey 5 5.2 all variance"},
                    {"fa2b99dc-9e39-4257-8f92-4a30d23c4118","YubiKey 5 NFC"},
                    {"2fc0579f-8113-47ea-b116-bb5a8db9202a","YubiKey 5 NFC"},
                    {"c5ef55ff-ad9a-4b9f-b580-adebafe026d0","YubiKey 5Ci"},
                    {"f8a011f3-8c0a-4d15-8006-17111f9edc7d","Security Key By Yubico"},
                    {"b92c3f9a-c014-4056-887f-140a2501163b","Security Key By Yubico"},
                    {"6d44ba9b-f6ec-2e49-b930-0c8fe920cb73","Security Key NFC By Yubico"},
                    {"149a2021-8ef6-4133-96b8-81f8d5b7f1f5","Security Key NFC By Yubico"},
                };

                    var x = cWebAuthnResult.Text;
                    Fido2Configuration fido2Config = new Fido2Configuration
                    {
                        Origin = "https://" + XHost(),
                        ServerDomain = XHost(),
                        ServerName = "site name",
                        ServerIcon = "https://www.rintagi.com/images/a.gif",
                        Timeout = 60 * 1000,
                        TimestampDriftTolerance = 1000
                    };

                    var fido2 = new Fido2(fido2Config);
                    IsCredentialIdUniqueToUserAsyncDelegate callback = async (IsCredentialIdUniqueToUserParams args) =>
                    {
                        var id = args.CredentialId; // generated ID by authenticator(should be always unique for each authenticator, equivalent to client cert)
                        var u = args.User;          // user info, kind of useless as this can be changed by js at client side
                        return await Task.FromResult(true);
                    };

                    var request = Newtonsoft.Json.JsonConvert.DeserializeObject<CredentialCreateOptions>(cRegistrationRequest.Text);
                    AuthenticatorAttestationRawResponseEx regResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthenticatorAttestationRawResponseEx>(x);
                    var success = fido2.MakeNewCredentialAsync(regResponse, request, callback).Result;
                    var clientDataJson = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthenticatorResponse>(System.Text.UTF8Encoding.UTF8.GetString(regResponse.Response.ClientDataJson));
                    var challenge = clientDataJson.Challenge;
                    var regState = Newtonsoft.Json.JsonConvert.DeserializeObject<WebAuthnChallenge>(System.Text.UTF8Encoding.UTF8.GetString(challenge));
                    var forUsrId = regState.usrId;
                    var appPath = regState.appPath;
                    var expiry = regState.expiredAfter;
                    var credentialId = base64UrlEncode(success.Result.CredentialId);
                    var publicKey = System.Convert.ToBase64String(success.Result.PublicKey);
                    var signingCounter = success.Result.Counter; // designed for replay attact prevention but useless for a multiple node situation
                    var user = success.Result.User;
                    var usrId = System.Convert.ToBase64String(user.Id);
                    var aaguid = success.Result.Aaguid;
                    var authData = new AuthenticatorData(AuthenticatorAttestationResponse.Parse(regResponse).AttestationObject.AuthData);
                    var userPresence = authData.UserPresent;
                    var userVerified = authData.UserVerified;
                    var loginMeta = Newtonsoft.Json.JsonConvert.SerializeObject(new WebAuthnMeta
                    {
                        credentialId = credentialId,
                        publicKey = publicKey,
                        aaguid = aaguid.ToString(),
                        usrId = usrId,
                        appPath = appPath,
                        platform = regResponse.platform,
                        isMobile = regResponse.isMobile,
                        authenticatorType = aaguidLookup.ContainsKey(aaguid.ToString()) ? aaguidLookup[aaguid.ToString()] : "unknown",
                        requestIPChain = string.Format("[{0}]",string.Join(",",GetClientIpChain().ToArray())),
                        userPresence = userPresence,
                        userVerified = userVerified,
                    });
                    LinkUserLogin(LUser.UsrId, "Fido2", credentialId, loginMeta);
                    try
                    {
                        // re-gen to prevent re-registration
                        string extAppDomainUrl =
                            !string.IsNullOrWhiteSpace(Config.ExtBaseUrl) && !string.IsNullOrEmpty(Request.Headers["X-Forwarded-For"])
                                ? Config.ExtBaseUrl
                                : Request.Url.AbsoluteUri.Replace(Request.Url.Query, "").Replace(Request.Url.Segments[Request.Url.Segments.Length - 1], "");

                        RegisterWebAuthnSetup(extAppDomainUrl, null);
                        VerifyWebAuthnSetup(extAppDomainUrl, null);
                    }
                    catch { }
                    if (!LUser.TwoFactorAuth && false) 
                    {
                        new LoginSystem().WrSetUsrOTPSecret(LUser.UsrId, true);
                    }

                    PreMsgPopup(
                        string.Format("WebAuthn(Fido2) registration successful with id {0}{1}"
                            , credentialId
                            , LUser.TwoFactorAuth ? "" : "\r\nPlease consider enable 2FA and/or choosing really random/complex password of at least 16 characters to harden other adhoc login"
                            )
                        );
                }
            }
            catch (Exception ex)
            {
                PreMsgPopup("failed to setup WebAuthn(Fido2)");
                ErrorTrace(ex, "error");
            }
        }
        protected void cLinkWindowsBtn_Click(object sender, EventArgs e)
        {
            if (LUser != null && LUser.LoginName != "Anonymous")
            {
                var LinkedAccount = (from dr in LinkedUserLogin where "W".IndexOf(dr["ProviderCd"].ToString().Trim()) >= 0 select dr).ToList();
                if (LinkedAccount.Count > 0)
                {
                    UnlinkUserLogin((int)LinkedAccount[0]["UsrId"], LinkedAccount[0]["ProviderCd"].ToString(), LinkedAccount[0]["LoginName"].ToString());
                }
                else
                {
                    if (this.Page.User.Identity.IsAuthenticated
                      && !string.IsNullOrEmpty(Session["WindowsLoginName"] as string))
                    {
                        LinkUserLogin(LUser.UsrId, "W", Session["WindowsLoginName"] as string);
                    }
                    else if (Config.TrustedLoginFederationUrl == "integrated")
                    {
                        System.Web.Security.FormsAuthentication.SignOut();
                        Response.StatusCode = 401;
                        Session["RequestWindowsLink"] = "1";
                    }
                }
            }
        }
        protected void cLinkGoogleBtn_Click(object sender, EventArgs e)
        {
            if (LUser != null && LUser.LoginName != "Anonymous")
            {
                var LinkedAccount = (from dr in LinkedUserLogin where "G".IndexOf(dr["ProviderCd"].ToString().Trim()) >= 0 select dr).ToList();
                if (LinkedAccount.Count > 0)
                {
                    UnlinkUserLogin((int)LinkedAccount[0]["UsrId"], LinkedAccount[0]["ProviderCd"].ToString(), LinkedAccount[0]["LoginName"].ToString());
                }
            }
        }
        protected void cLinkFacebookBtn_Click(object sender, EventArgs e)
        {
            if (LUser != null && LUser.LoginName != "Anonymous")
            {
                var LinkedAccount = (from dr in LinkedUserLogin where "F".IndexOf(dr["ProviderCd"].ToString().Trim()) >= 0 select dr).ToList();
                if (LinkedAccount.Count > 0)
                {
                    UnlinkUserLogin((int)LinkedAccount[0]["UsrId"], LinkedAccount[0]["ProviderCd"].ToString(), LinkedAccount[0]["LoginName"].ToString());
                }
            }
        }
        protected void cLinkMicrosoftBtn_Click(object sender, EventArgs e)
        {
            if (LUser != null && LUser.LoginName != "Anonymous")
            {
                var LinkedAccount = (from dr in LinkedUserLogin where "M,O".IndexOf(dr["ProviderCd"].ToString().Trim()) >= 0 select dr).ToList();
                if (LinkedAccount.Count > 0)
                {
                    UnlinkUserLogin((int) LinkedAccount[0]["UsrId"],LinkedAccount[0]["ProviderCd"].ToString(), LinkedAccount[0]["LoginName"].ToString());
                }
                else
                {
                    AzureLoginBtn_Click(sender, e);
                }
            }
        }

        #region webauthn/fido2
        private void RegisterWebAuthnSetup(string appDomainUrl,  string registrationJSON)
        {
            if (!Config.EnableFido2) return;
            //var x = Newtonsoft.Json.JsonConvert.DeserializeObject(registrationJSON);
            bool isSafari = Request.UserAgent.Contains("Safari");
            var serverDomain = new Uri(appDomainUrl).GetComponents(UriComponents.Host, UriFormat.Unescaped);
            var serverPath = new Uri(appDomainUrl).GetComponents(UriComponents.Path, UriFormat.Unescaped);
            string usrId = LUser.UsrId.ToString();
            string usrIdB64 = System.Convert.ToBase64String(usrId.ToUtf8ByteArray());
            
            var dtLogin = new LoginSystem().GetLogins(null, "Fido2", usrId);
            
            Fido2Configuration fido2Config = new Fido2Configuration
            {
//                Origin = serverDomain,
                ServerDomain = serverDomain,
                ServerName = serverDomain,
//                ServerIcon = "https://www.rintagi.com/images/a.gif",
                Timeout = 60 * 1000,
                TimestampDriftTolerance = 1000
            };
            byte[] challenge =
                Newtonsoft.Json.JsonConvert.SerializeObject(
                new WebAuthnChallenge
                {
                    nonce = Guid.NewGuid().ToString(),
                    usrId = serverPath + usrIdB64,
                    appPath = serverPath,
                    expiredAfter = Common3.Utils.ToUnixTime(DateTime.UtcNow.AddMinutes(30))
                }
                ).ToUtf8ByteArray();
            Fido2User user = new Fido2User
            {
                DisplayName = LUser.UsrName,
                /* must be restricted to no more than than 64 for device like yubikey as it would fail without reason */
                //Name = (Guid.NewGuid().ToString() + " " + DateTime.UtcNow.ToString("o")).Left(64),
                //Id= Guid.NewGuid().ToString().ToUtf8ByteArray()
                Name = (serverPath + LUser.LoginName).Left(64),
                Id = (serverPath + usrIdB64).ToUtf8ByteArray()
            };
            AuthenticatorSelection authenticatorSelection = new AuthenticatorSelection
            {
                RequireResidentKey = false,
                UserVerification = UserVerificationRequirement.Discouraged,
                //                 AuthenticatorAttachment = AuthenticatorAttachment.Platform,
            };
            // iOS Safari cannot use Direct/Indirect, library don't support
            AttestationConveyancePreference attConveyancePreference = isSafari ? AttestationConveyancePreference.None : AttestationConveyancePreference.Indirect;
            List<PublicKeyCredentialDescriptor> excludedCredentials = dtLogin.AsEnumerable().Select(
                    dr =>
                    {
                        return new PublicKeyCredentialDescriptor()
                        {
                            Id = base64UrlDecode(dr["LoginName"].ToString()),
                            // iOS Safari cannot specify Transport
                            Transports = isSafari 
                                ? null 
                                : new AuthenticatorTransport[]{
                                AuthenticatorTransport.Ble, AuthenticatorTransport.Internal, AuthenticatorTransport.Lightning, AuthenticatorTransport.Nfc, AuthenticatorTransport.Usb
                            },
                            Type = PublicKeyCredentialType.PublicKey,
                        };
                    }
                    ).ToList();
            //new List<PublicKeyCredentialDescriptor> { };
            AuthenticationExtensionsClientInputs clientExtensions = new AuthenticationExtensionsClientInputs
            {
                Extensions = true,
                SimpleTransactionAuthorization = "you are registering to " + GetDomainUrl(false),
                Location = true,
                UserVerificationMethod = true,
                BiometricAuthenticatorPerformanceBounds = new AuthenticatorBiometricPerfBounds
                {
                    FAR = float.MaxValue,
                    FRR = float.MaxValue
                }
            };

            var fido2 = new Fido2(fido2Config);

            // must do this for the verification to work
            var options = fido2.RequestNewCredential(user, null, authenticatorSelection, attConveyancePreference, clientExtensions);
            // the challenge is random byte but we need more info, replace it
            options.Challenge = challenge;
            string createRequestJson = options.ToJson();
            cRegistrationRequest.Text = createRequestJson;
        }

        private void VerifyWebAuthnSetup(string appDomainUrl, string registrationJSON, bool TwoFA = false)
        {
            if (!Config.EnableFido2) return;
            //var x = Newtonsoft.Json.JsonConvert.DeserializeObject(registrationJSON);
            var serverDomain = new Uri(appDomainUrl).GetComponents(UriComponents.Host, UriFormat.Unescaped);
            var serverPath = new Uri(appDomainUrl).GetComponents(UriComponents.Path, UriFormat.Unescaped);
            Fido2Configuration fido2Config = new Fido2Configuration
            {
                Origin = serverDomain,
                ServerDomain = serverDomain,
                ServerName = serverDomain,
                ServerIcon = "https://www.rintagi.com/images/a.gif",
                Timeout = 60 * 1000,
                TimestampDriftTolerance = 1000
            };
            byte[] challenge =
                Newtonsoft.Json.JsonConvert.SerializeObject(
                new WebAuthnChallenge
                {
                    nonce = Guid.NewGuid().ToString(),
                    usrId = null,
                    appPath = serverPath,
                    expiredAfter = Common3.Utils.ToUnixTime(DateTime.UtcNow.AddMinutes(30))
                }
                ).ToUtf8ByteArray();
            AuthenticatorSelection authSelection = new AuthenticatorSelection
            {
                RequireResidentKey = false,
                UserVerification = UserVerificationRequirement.Required,
                AuthenticatorAttachment = TwoFA ? AuthenticatorAttachment.Platform : (AuthenticatorAttachment?) null,
                //                AuthenticatorAttachment = null,
            };
            var dtLogin = new LoginSystem().GetLogins(cLoginName.Text, "Fido2");
            //var dtLogin = new LoginSystem().GetLogins("John Doe", "Fido2");
            List<PublicKeyCredentialDescriptor> allowedCredentials = dtLogin.AsEnumerable().Select(
                    dr =>
                    {
                        return new PublicKeyCredentialDescriptor()
                        {
                            Id = base64UrlDecode(dr["LoginName"].ToString()),
                            Transports = new AuthenticatorTransport[]{
                                AuthenticatorTransport.Ble, AuthenticatorTransport.Internal, AuthenticatorTransport.Lightning, AuthenticatorTransport.Nfc, AuthenticatorTransport.Usb
                            },
                            Type = PublicKeyCredentialType.PublicKey,
                        };
                    }
                    ).ToList();

            //List<PublicKeyCredentialDescriptor> allowedCredentials = new List<PublicKeyCredentialDescriptor> {};
            AuthenticationExtensionsClientInputs clientExtensions = new AuthenticationExtensionsClientInputs
            {
                Extensions = true,
                SimpleTransactionAuthorization = "you are signing in to abc.com",
                Location = true,
                UserVerificationMethod = true,
            };

            var fido2 = new Fido2(fido2Config);

            var verificationRequest = Fido2NetLib.AssertionOptions.Create(fido2Config, challenge, allowedCredentials, UserVerificationRequirement.Preferred, clientExtensions);
            string verificationRequestJson = verificationRequest.ToJson();
            cAssertionRequest.Text = verificationRequestJson;
        }

        #endregion
        #region eth1 wallet(metamask)
        private void RegisterEth1WalletSetup(string appDomainUrl, string registrationJSON)
        {
            if (!Config.EnableCryptoWallet) return;
            var serverDomain = new Uri(appDomainUrl).GetComponents(UriComponents.Host, UriFormat.Unescaped);
            var serverPath = new Uri(appDomainUrl).GetComponents(UriComponents.Path, UriFormat.Unescaped);
            string usrId = LUser.UsrId.ToString();
            string usrIdB64 = System.Convert.ToBase64String(usrId.ToUtf8ByteArray());

            string challenge =
                Newtonsoft.Json.JsonConvert.SerializeObject(
                new WebAuthnChallenge
                {
                    nonce = Guid.NewGuid().ToString(),
                    usrId = serverPath + usrIdB64,
                    appPath = serverPath,
                    expiredAfter = Common3.Utils.ToUnixTime(DateTime.UtcNow.AddMinutes(30))
                }
                );
            cEth1RegistrationRequest.Text = challenge;
        }

        private void VerifyEth1WalletSetup(string appDomainUrl, string registrationJSON, bool TwoFA = false)
        {
            if (!Config.EnableCryptoWallet) return;
            var serverDomain = new Uri(appDomainUrl).GetComponents(UriComponents.Host, UriFormat.Unescaped);
            var serverPath = new Uri(appDomainUrl).GetComponents(UriComponents.Path, UriFormat.Unescaped);
            string challenge =
                Newtonsoft.Json.JsonConvert.SerializeObject(
                new WebAuthnChallenge
                {
                    nonce = Guid.NewGuid().ToString(),
                    usrId = null,
                    appPath = serverPath,
                    expiredAfter = Common3.Utils.ToUnixTime(DateTime.UtcNow.AddMinutes(30))
                }
                );
            cEth1AssertionRequest.Text = challenge;
        }

        #endregion
    }
}