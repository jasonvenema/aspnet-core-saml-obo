// using Microsoft.AspNetCore.Authentication.OpenIdConnect;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Http;
// using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Security.Claims;
// using System.Text;
// using System.Threading.Tasks;
// using System.Web;
// using System.Xml;
// using WebApplication1.Util;

// namespace WebApplication1.Middleware
// {
//     public class SamlResponseCacheMiddleware
//     {
//         private const string SAML_RESPONSE_KEY = "SAMLResponse";
//         private const string CLAIM_USER_NAME = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

//         private readonly RequestDelegate _next;

//         public SamlResponseCacheMiddleware(RequestDelegate next)
//         {
//             this._next = next;
//         }

//         public async Task Invoke(HttpContext context)
//         {
//             // SAML response will arrive via POST
//             if (String.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
//             {
//                 var samlResponse = await ReadSaml2ResponseFromBody(context);
//                 AddSamlResponseToCache(samlResponse);
//             }

//             await this._next.Invoke(context).ConfigureAwait(false);
//         }

//         private static void AddSamlResponseToCache(string samlResponse)
//         {
//             string userName = default(string);
//             using (MemoryStream strm = new MemoryStream(Encoding.UTF8.GetBytes(samlResponse)))
//             {
//                 XmlReader reader = XmlReader.Create(strm);
//                 while (reader.Read())
//                 {
//                     if (reader.NodeType == XmlNodeType.Element && reader.Name == "Attribute")
//                     {
//                         if (reader.GetAttribute("Name") == CLAIM_USER_NAME)
//                         {
//                             reader.Read();
//                             reader.Read();
//                             userName = reader.Value;
//                             break;
//                         }
//                     }
//                 }
//             }

//             if (!String.IsNullOrEmpty(userName))
//             {
//                 //TokenCache.AddToken(userName, samlResponse);
//             }
//         }

//         private static async Task<string> ReadSaml2ResponseFromBody(HttpContext context)
//         {
//             // Allow downstream middleware to read the stream by buffering our own copy
//             context.Request.EnableBuffering();

//             string saml2ResponseString = default(string);
//             string saml2ResponseStringDow = default(string);

//             // Read the stream body
//             using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
//             {
//                 var body = await reader.ReadToEndAsync();

//                 // Check for a SAML response in the body
//                 if (body.Contains(SAML_RESPONSE_KEY))
//                 {
//                     var bodyPairs = body.Split(new char[] { '&' });
//                     foreach (var pair in bodyPairs)
//                     {
//                         var tuple = pair.Split(new char[] { '=' });
//                         if (tuple.Length > 1)
//                         {
//                             if (String.Equals(tuple[0], SAML_RESPONSE_KEY, StringComparison.OrdinalIgnoreCase))
//                             {
//                                 saml2ResponseString = DecodeSamlResponse(tuple[1], true);

//                                 saml2ResponseStringDow = DecodeSamlResponse(DOW_SAML_RESPONSE, false);
//                             }
//                         }
//                     }
//                 }

//                 context.Request.Body.Seek(0, SeekOrigin.Begin);
//             }

//             return String.Concat(saml2ResponseString, Environment.NewLine, saml2ResponseStringDow);
//         }

//         private static string DecodeSamlResponse(string encodedResponse, bool shouldUrlDecode)
//         {
//             string saml2ResponseString;
//             var saml2Decoded = encodedResponse;
//             if (shouldUrlDecode)
//             {
//                 saml2Decoded = HttpUtility.UrlDecode(encodedResponse, Encoding.UTF8);
//             }

//             var saml2Response = Convert.FromBase64String(saml2Decoded);
//             saml2ResponseString = Encoding.UTF8.GetString(saml2Response);
//             //saml2ResponseString = saml2ResponseString.Replace("\\\"", "\"");
//             using (FileStream fs = new FileStream(@"C:\Temp\samlj.xml", FileMode.Create, FileAccess.ReadWrite))
//             {
//                 fs.Write(saml2Response);
//             }
//             return saml2ResponseString;
//         }

//         private const string DOW_SAML_RESPONSE = "PHNhbWxwOlJlc3BvbnNlIElEPSJfMTI5M2M3ZmItN2UwMy00ZjkwLWJiZWMtZmVmNWJmMjE0N2ZmIiBWZXJzaW9uPSIyLjAiIElzc3VlSW5zdGFudD0iMjAxOS0xMC0xNVQxOTozNjoyMC44NzlaIiBEZXN0aW5hdGlvbj0iaHR0cHM6Ly9hdXRobi51czEuaGFuYS5vbmRlbWFuZC5jb20vc2FtbDIvc3AvYWNzL2JhNTkxNjc0NS9iYTU5MTY3NDUiIEluUmVzcG9uc2VUbz0iUzVhNTA5ZGZjLTI1YTktNDA0Zi05NzE1LTk5OTcxNjE3Mzc5Mi1nUFpIczlXNmJIdFJBcERMUDVwMHBmdGFEY2VKMjJZTU83RmlEV2xtYlBzIiB4bWxuczpzYW1scD0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOnByb3RvY29sIj48SXNzdWVyIHhtbG5zPSJ1cm46b2FzaXM6bmFtZXM6dGM6U0FNTDoyLjA6YXNzZXJ0aW9uIj5odHRwczovL3N0cy53aW5kb3dzLm5ldC9jM2UzMmY1My1jYjdmLTQ4MDktOTY4ZC0xY2M0Y2NjNzg1ZmUvPC9Jc3N1ZXI+PHNhbWxwOlN0YXR1cz48c2FtbHA6U3RhdHVzQ29kZSBWYWx1ZT0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOnN0YXR1czpTdWNjZXNzIi8+PC9zYW1scDpTdGF0dXM+PEFzc2VydGlvbiBJRD0iX2E2MTQ0MWY3LTMzZjEtNGY5Ny04MmZiLTMxMWQwMDVkOWMwMCIgSXNzdWVJbnN0YW50PSIyMDE5LTEwLTE1VDE5OjM2OjIwLjg3NFoiIFZlcnNpb249IjIuMCIgeG1sbnM9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjIuMDphc3NlcnRpb24iPjxJc3N1ZXI+aHR0cHM6Ly9zdHMud2luZG93cy5uZXQvYzNlMzJmNTMtY2I3Zi00ODA5LTk2OGQtMWNjNGNjYzc4NWZlLzwvSXNzdWVyPjxTaWduYXR1cmUgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvMDkveG1sZHNpZyMiPjxTaWduZWRJbmZvPjxDYW5vbmljYWxpemF0aW9uTWV0aG9kIEFsZ29yaXRobT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS8xMC94bWwtZXhjLWMxNG4jIi8+PFNpZ25hdHVyZU1ldGhvZCBBbGdvcml0aG09Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvMDQveG1sZHNpZy1tb3JlI3JzYS1zaGEyNTYiLz48UmVmZXJlbmNlIFVSST0iI19hNjE0NDFmNy0zM2YxLTRmOTctODJmYi0zMTFkMDA1ZDljMDAiPjxUcmFuc2Zvcm1zPjxUcmFuc2Zvcm0gQWxnb3JpdGhtPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwLzA5L3htbGRzaWcjZW52ZWxvcGVkLXNpZ25hdHVyZSIvPjxUcmFuc2Zvcm0gQWxnb3JpdGhtPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzEwL3htbC1leGMtYzE0biMiLz48L1RyYW5zZm9ybXM+PERpZ2VzdE1ldGhvZCBBbGdvcml0aG09Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvMDQveG1sZW5jI3NoYTI1NiIvPjxEaWdlc3RWYWx1ZT5zZXR1ZkJBUlFmbDFkdE1VV0JYVE45cWEzQmg3WWF5MXFiUVl1MkNnZGpnPTwvRGlnZXN0VmFsdWU+PC9SZWZlcmVuY2U+PC9TaWduZWRJbmZvPjxTaWduYXR1cmVWYWx1ZT5KTnhKandvOEI0Rzg5a0FTSlRPamJtZExWc1h5ajh6STh3czRtNWNBOGN2Z0NhREJLcHgvUEpLdlN0ejd5V0FVbEVvazBZbmE5a040UGN2S1dTekpWL01uTE5KdnR1bS9iNU5WQytnVVE2emJxeUhoMlZCR3VGL3YzVk5KUzZCZTNoNzNMVC9EQ0ZieUJLQ3VqT1J3ZHV0Mm9QSzRiWk1mWERwa3IzeUpMeWt2M3BjOWVxcTFrcUJxU3lyRG1TMkxCenpVOXptdUxRLzFmMmpzd040VEpXamw2bjdYbDJCaW5ta2ZlTXkwek5uMFJCM1JCYnBPSzVuL1ZhQ1ZnMjBZdDBaejRDNzJ6WXlaVGFSbFRmUTduckI2MHcxSVJRT1pwUHdKOVFsSVk1ZmZ5L0d5OUEvVjVncHE5dU9NVjIvWkFRT2VleEtyUWkrSGYxUkJ0Slh4Y0E9PTwvU2lnbmF0dXJlVmFsdWU+PEtleUluZm8+PFg1MDlEYXRhPjxYNTA5Q2VydGlmaWNhdGU+TUlJQzhEQ0NBZGlnQXdJQkFnSVFPUVhLTTdrOFJKZE5hVDFFNUhqRHR6QU5CZ2txaGtpRzl3MEJBUXNGQURBME1USXdNQVlEVlFRREV5bE5hV055YjNOdlpuUWdRWHAxY21VZ1JtVmtaWEpoZEdWa0lGTlRUeUJEWlhKMGFXWnBZMkYwWlRBZUZ3MHhPVEE0TWpZd056UTVORGRhRncweU1qQTRNall3TnpRNU16ZGFNRFF4TWpBd0JnTlZCQU1US1UxcFkzSnZjMjltZENCQmVuVnlaU0JHWldSbGNtRjBaV1FnVTFOUElFTmxjblJwWm1sallYUmxNSUlCSWpBTkJna3Foa2lHOXcwQkFRRUZBQU9DQVE4QU1JSUJDZ0tDQVFFQTRDZzVocFYzT1hoM04zakpJUGdLWnAzMTJmN3FNcTFXQ2tnM3BNakNWbWRvMGhETU4rNFpZckd1YnpNbXkwV1VKdUVrN1JsZ0NKb2I3eXJRcnR6VzhRR0R5TEZwK3J1azZrcmxYTDEvQVhtZytVbWRVV2tSMENiUlNiRVl4QVZPZVZKVTNGbThmNlQxbzU5QjhETWNQdy9UbGhzdVp3UFpQZzJDd1plSHo5aU9SVE82bjg0d3lrM2M1amkyWFRqTE51WlNzNVQ1QlBQeEI1eHo3Yml0NWxESUIrRUV2dTdXeWtJUVpqVWhXNjNhaVhEcU5sRmtoY3BHMnREM0VycHVOVkxNVHdqVWZZT1ZndlZTaHo2M205dm5BUFYzUityRzljcy80RDJ1OTZlb3J3a2lZOVhYb0lYcjFLdDJhclBOaGtOMitib2VjNnd0WDI0VFJJOGxRd0lEQVFBQk1BMEdDU3FHU0liM0RRRUJDd1VBQTRJQkFRQXpaVGxULzRWU1VuTEpXQWJTcDFBKy81S2wveTFGeXBPNUF3SE1OQXB5UlRsT1RiRHJNb01STjZOdGNoNU1zQUlObzRFK094VHJzOGNqOWZOdlg4ZUpoaWRnK05LRnA1d1NtWlpqbXRoVyt2RE5kaEpBTWI0Zm12aS9zb1NEaFJDZCszRi9OZE1nWmVLL3BQVlpNdTRESkx5NG5SWk5vQlVMZjRBVklabG03OUJqYnNuN09ycjNoRy9aNmZFdzNsYWx5NGN6S0E3dW1Eb2IyTzJ5Y2xyd01qc25WNThJNlpWL0NFdFNLbUk5WExHUUM5MzZHRFhnaFJXdmNWNTVPY0Z0bDJtMmo3UTFJZFN3OFBGUUpUaFEzWnZkQzB6THFuYzNUaHAzVmoxK0x6M1pGMEY3V2xGQVhIYXNTV3VqelphRkQ4SFMwZ2dhNjI0Z2ZqVklaTUQ4PC9YNTA5Q2VydGlmaWNhdGU+PC9YNTA5RGF0YT48L0tleUluZm8+PC9TaWduYXR1cmU+PFN1YmplY3Q+PE5hbWVJRCBGb3JtYXQ9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjEuMTpuYW1laWQtZm9ybWF0OnVuc3BlY2lmaWVkIj5VQTA0MzU0PC9OYW1lSUQ+PFN1YmplY3RDb25maXJtYXRpb24gTWV0aG9kPSJ1cm46b2FzaXM6bmFtZXM6dGM6U0FNTDoyLjA6Y206YmVhcmVyIj48U3ViamVjdENvbmZpcm1hdGlvbkRhdGEgSW5SZXNwb25zZVRvPSJTNWE1MDlkZmMtMjVhOS00MDRmLTk3MTUtOTk5NzE2MTczNzkyLWdQWkhzOVc2Ykh0UkFwRExQNXAwcGZ0YURjZUoyMllNTzdGaURXbG1iUHMiIE5vdE9uT3JBZnRlcj0iMjAxOS0xMC0xNVQxOTo0MToyMC44NzRaIiBSZWNpcGllbnQ9Imh0dHBzOi8vYXV0aG4udXMxLmhhbmEub25kZW1hbmQuY29tL3NhbWwyL3NwL2Fjcy9iYTU5MTY3NDUvYmE1OTE2NzQ1Ii8+PC9TdWJqZWN0Q29uZmlybWF0aW9uPjwvU3ViamVjdD48Q29uZGl0aW9ucyBOb3RCZWZvcmU9IjIwMTktMTAtMTVUMTk6MzE6MjAuODc0WiIgTm90T25PckFmdGVyPSIyMDE5LTEwLTE1VDIwOjM2OjIwLjg3NFoiPjxBdWRpZW5jZVJlc3RyaWN0aW9uPjxBdWRpZW5jZT5odHRwczovL3VzMS5oYW5hLm9uZGVtYW5kLmNvbS9iYTU5MTY3NDU8L0F1ZGllbmNlPjwvQXVkaWVuY2VSZXN0cmljdGlvbj48L0NvbmRpdGlvbnM+PEF0dHJpYnV0ZVN0YXRlbWVudD48QXR0cmlidXRlIE5hbWU9Imh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vaWRlbnRpdHkvY2xhaW1zL3RlbmFudGlkIj48QXR0cmlidXRlVmFsdWU+YzNlMzJmNTMtY2I3Zi00ODA5LTk2OGQtMWNjNGNjYzc4NWZlPC9BdHRyaWJ1dGVWYWx1ZT48L0F0dHJpYnV0ZT48QXR0cmlidXRlIE5hbWU9Imh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vaWRlbnRpdHkvY2xhaW1zL29iamVjdGlkZW50aWZpZXIiPjxBdHRyaWJ1dGVWYWx1ZT5jYmM1MjJkMy02MzY0LTQ5YWYtYTlhYy0xMTFkZjJiNTU2MzM8L0F0dHJpYnV0ZVZhbHVlPjwvQXR0cmlidXRlPjxBdHRyaWJ1dGUgTmFtZT0iaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI+PEF0dHJpYnV0ZVZhbHVlPlVBMDQzNTRARG93LmNvbTwvQXR0cmlidXRlVmFsdWU+PC9BdHRyaWJ1dGU+PEF0dHJpYnV0ZSBOYW1lPSJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9zdXJuYW1lIj48QXR0cmlidXRlVmFsdWU+TWF0aHk8L0F0dHJpYnV0ZVZhbHVlPjwvQXR0cmlidXRlPjxBdHRyaWJ1dGUgTmFtZT0iaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZ2l2ZW5uYW1lIj48QXR0cmlidXRlVmFsdWU+SmVyZW15PC9BdHRyaWJ1dGVWYWx1ZT48L0F0dHJpYnV0ZT48QXR0cmlidXRlIE5hbWU9Imh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vaWRlbnRpdHkvY2xhaW1zL2Rpc3BsYXluYW1lIj48QXR0cmlidXRlVmFsdWU+TWF0aHksIEplcmVteSAoSik8L0F0dHJpYnV0ZVZhbHVlPjwvQXR0cmlidXRlPjxBdHRyaWJ1dGUgTmFtZT0iaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS9pZGVudGl0eS9jbGFpbXMvaWRlbnRpdHlwcm92aWRlciI+PEF0dHJpYnV0ZVZhbHVlPmh0dHBzOi8vc3RzLndpbmRvd3MubmV0L2MzZTMyZjUzLWNiN2YtNDgwOS05NjhkLTFjYzRjY2M3ODVmZS88L0F0dHJpYnV0ZVZhbHVlPjwvQXR0cmlidXRlPjxBdHRyaWJ1dGUgTmFtZT0iaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS9jbGFpbXMvYXV0aG5tZXRob2RzcmVmZXJlbmNlcyI+PEF0dHJpYnV0ZVZhbHVlPmh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9hdXRoZW50aWNhdGlvbm1ldGhvZC93aW5kb3dzPC9BdHRyaWJ1dGVWYWx1ZT48L0F0dHJpYnV0ZT48QXR0cmlidXRlIE5hbWU9Imh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI+PEF0dHJpYnV0ZVZhbHVlPmplcmVteS5tYXRoeUBkb3cuY29tPC9BdHRyaWJ1dGVWYWx1ZT48L0F0dHJpYnV0ZT48L0F0dHJpYnV0ZVN0YXRlbWVudD48QXV0aG5TdGF0ZW1lbnQgQXV0aG5JbnN0YW50PSIyMDE5LTEwLTE1VDE5OjM2OjE5Ljc5OVoiIFNlc3Npb25JbmRleD0iX2E2MTQ0MWY3LTMzZjEtNGY5Ny04MmZiLTMxMWQwMDVkOWMwMCI+PEF1dGhuQ29udGV4dD48QXV0aG5Db250ZXh0Q2xhc3NSZWY+dXJuOmZlZGVyYXRpb246YXV0aGVudGljYXRpb246d2luZG93czwvQXV0aG5Db250ZXh0Q2xhc3NSZWY+PC9BdXRobkNvbnRleHQ+PC9BdXRoblN0YXRlbWVudD48L0Fzc2VydGlvbj48L3NhbWxwOlJlc3BvbnNlPg==";
//     }

//     public static class SamlResponseCacheExtensions
//     {
//         public static IApplicationBuilder UseSamlResponseCache(this IApplicationBuilder builder)
//         {
//             return builder.UseMiddleware<SamlResponseCacheMiddleware>();
//         }
//     }
// }
