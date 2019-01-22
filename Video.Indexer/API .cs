using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Video.Indexer.Models;

namespace Video.Indexer
{

    public class Api
    {
        const string apiUrl = "https://api.videoindexer.ai";
        const string accountId = "...";
        const string location = "westus2";
        const string apiKey = "...";

        VideoModel VideoIndexer(string videoUrl)
        {
            VideoModel videoModelResult = new VideoModel();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.ServicePointManager.SecurityProtocol | System.Net.SecurityProtocolType.Tls12;

            // HttpClient
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

            // GET ACCESS TOKEN     
            var requestResultToken = client.GetAsync($"{apiUrl}/auth/{location}/Accounts/{accountId}/AccessToken?allowEdit=true").Result;
            var accessToken = requestResultToken.Content.ReadAsStringAsync().Result.Replace("\"", "");

            client.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");
            var multipartFormDataContent = new MultipartFormDataContent();
            var uploadRequestResult = client.PostAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos?accessToken={accessToken}&name=some_name&description=some_description&privacy=private&partition=some_partition&videoUrl={videoUrl}", multipartFormDataContent).Result;
            var uploadResult = uploadRequestResult.Content.ReadAsStringAsync().Result;

            // GET VIDEO ID
            var idVideo = JsonConvert.DeserializeObject<dynamic>(uploadResult)["id"];

            // GET ACCESS TOKEN            
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            var videoTokenRequestResultAsync = client.GetAsync($"{apiUrl}/auth/{location}/Accounts/{accountId}/Videos/{idVideo}/AccessToken?allowEdit=true").Result;
            var videoAccessToken = videoTokenRequestResultAsync.Content.ReadAsStringAsync().Result.Replace("\"", "");

            client.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");

            // wait for the video index to finish
            while (true)
            {
                Thread.Sleep(10000);

                var videoGetIndexRequestResult = client.GetAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos/{idVideo}/Index?accessToken={videoAccessToken}&language=English").Result;
                var videoGetIndexResult = videoGetIndexRequestResult.Content.ReadAsStringAsync().Result;

                var processingState = JsonConvert.DeserializeObject<dynamic>(videoGetIndexResult)["state"];

                // Work finished
                if (processingState != "Uploaded" && processingState != "Processing")
                {
                    break;
                }
            }
            // search for the video
            var searchRequestResultAsync = client.GetAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos/Search?accessToken={accessToken}&id={idVideo}").Result;
            videoModelResult.Search = searchRequestResultAsync.Content.ReadAsStringAsync().Result;
            // get insights widget link
            var insightsWidgetRequestResultAync = client.GetAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos/{idVideo}/InsightsWidget?accessToken={videoAccessToken}&widgetType=Keywords&allowEdit=true").Result;
            videoModelResult.InsightsWidgetUri = insightsWidgetRequestResultAync.Headers.Location;
            // get player widget link
            var playerWidgetRequestResultAsync = client.GetAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos/{idVideo}/PlayerWidget?accessToken={videoAccessToken}").Result;
            videoModelResult.PlayerWidgetUri = playerWidgetRequestResultAsync.Headers.Location;
            return videoModelResult;
        }
    }
}
