using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JobFinderWebSite.Models;
using System.Net.Http;

namespace JobFinderWebSite.Custom
{
    // MM "Token Module: - Generate the token"
    //-----------------------------------
    public class Token
    {
        public static HttpCookie GetToken(LoginViewModel model)
        {
            using (HttpClient _httpClient = new HttpClient())
            {
                //_httpClient.BaseAddress = new Uri("http://localhost:53784/");
                _httpClient.BaseAddress = new Uri("http://ahmed3196-001-site1.ctempurl.com/");

                var content = new StringContent(string.Format("grant_type=password&username={0}&password={1}", model.Email, model.Password), System.Text.Encoding.UTF8);

                var responseTask = _httpClient.PostAsync("token", content);
                responseTask.Wait();

                var response = responseTask.Result;

                if(response.IsSuccessStatusCode)
                {
                    var readTask = response.Content.ReadAsAsync<TokenModel>();
                    var token = readTask.Result;

                    // https://docs.microsoft.com/en-us/dotnet/api/system.web.httpcookie?view=netframework-4.8

                    HttpCookie BearerToken = new HttpCookie("BearerToken", token.access_token);
                    BearerToken.HttpOnly = true;

                    return BearerToken;
                }
                return null;
            }
        }
    }
    //-----------------------------------
}