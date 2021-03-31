using System;
using MyPhonePlugins;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

namespace K2MPhoneCallApi
{
    [CRMPluginLoader]
    public class K2MPhoneApi
    {
        private static K2MPhoneApi _phoneApiInstance = null;
        private readonly IMyPhoneCallHandler _callHandler;
    
        private string callId = null;

        [CRMPluginInitializer]
        public static void Loader(IMyPhoneCallHandler callHandler)
        {
            _phoneApiInstance = new K2MPhoneApi(callHandler);
        }

        private K2MPhoneApi(IMyPhoneCallHandler callhandler)
        {
            _callHandler = callhandler;
            callhandler.OnCallStatusChanged += Callhandler_OnCallStatusChanged;
        }

        private void Callhandler_OnCallStatusChanged(object sender, CallStatus callInfo)
        {
            if (callInfo.OriginatorType == OriginatorType.Queue && callInfo.Incoming &&
                callInfo.State == CallState.Connected && !callInfo.IsHold && !callInfo.IsMuted &&
                callInfo.CallID != callId)
             {
                 
                 callId = callInfo.CallID;
                 string text = Post(callInfo.OtherPartyNumber);
                 bool durum = text != "bos";
                 if (durum)
                 {
                     Process.Start("chrome.exe", "https://crm365.folkart.com.tr/WebResources/new_customersearch?data=" + callInfo.OtherPartyNumber + ",callid=" + text);
                 }
               
                 // Process.Start("http://10.41.1.33:8182/Home/CallerRedirectPage?CallerId=" + callInfo.OtherPartyNumber);
             }
             // else if (callInfo.OriginatorType == OriginatorType.Queue && callInfo.Incoming &&
             //          callInfo.State == CallState.Ringing && !controlled)
             // {
             //     controlled = true;
             // }
        }
        public static string Post(string value)
        {
            Uri requestUri = new Uri("https://contact.folkart.com.tr:8899/3cx/makecall/getcallid/");
            WebRequest webRequest = WebRequest.Create(requestUri);
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            webRequest.ContentType = "text/plain";
            webRequest.Method = "POST";
            string result;
            try
            {
                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }
                HttpWebResponse httpWebResponse = (HttpWebResponse)webRequest.GetResponse();
                string text = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
                result = text;
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
                result = null;
            }
            return result;
        }
    
    }
   
}
