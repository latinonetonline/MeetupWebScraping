using AivenEcommerce.V1.Modules.GitHub.Services;

using AngleSharp;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;

using GitHubActionSharp;

using MeetupWebScraping.Models;

using Octokit;

using PuppeteerSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeetupWebScraping
{
    enum Parameters
    {
        [Parameter("gh-token")]
        GitHubToken
    }

    class Program
    {
        public static Browser Browser { get; set; }

        static async Task Main(string[] args)
        {
            GitHubActionContext actionContext = new(args);
            actionContext.LoadParameters();

            string value = actionContext.GetParameter(Parameters.GitHubToken);

            string html = await GetPageHtmlAsync(new("https://www.meetup.com/es-ES/latino-net-online/events/past/rss"));

            List<MeetupEvent> events = await GetEventsAsync(html);

            await UploadEventsAsync(new(DateTime.Now, events), value);

            await Browser.DisposeAsync();

            Console.WriteLine("Finish");
        }

        static async Task<string> GetPageHtmlAsync(Uri uri)
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            Browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });

            using (var page = await Browser.NewPageAsync())
            {
                await page.GoToAsync(uri.ToString());

                var html = await page.GetContentAsync();

                return html;
            }
        }

        static async Task<List<MeetupEvent>> GetEventsAsync(string html)
        {
            //Use the default configuration for AngleSharp
            var config = Configuration.Default.WithCss();

            //Create a new context for evaluating webpages with the given config
            var context = BrowsingContext.New(config);


            //Create a virtual request to specify the document to load (here from our fixed string)
            var document = await context.OpenAsync(req => req.Content(html));

            IHtmlCollection<IElement> elements = document.QuerySelectorAll(".card.card--hasHoverShadow.eventCard.border--none.buttonPersonality");

            List<MeetupEvent> events = new();

            foreach (IElement element in elements.Take(3))
            {
                IElement linkElement = element.QuerySelector(".eventCard--link");

                IElement titleElement = element.QuerySelector("div > div:nth-child(1) > div > div > div:nth-child(1) > div.text--ellipsisTwoLines.text--sectionTitle.margin--halfBottom.text--secondary > a");

                IElement summaryElement = element.QuerySelector("div > div:nth-child(2) > div > div > div > p:nth-child(2)");

                IElement imageElement = element.QuerySelector("span.eventCardHead--photo");

                IElement dateElement = element.QuerySelector("div > div:nth-child(1) > div > div > div:nth-child(1) > div.eventTimeDisplay.text--labelSecondary.text--small.wrap--singleLine--truncate.margin--halfBottom > time");

                Uri linkMeetup = new("https://www.meetup.com" + linkElement.GetAttribute("href"));

                string title = titleElement.InnerHtml;
                string summaryHtml = summaryElement.InnerHtml;

                var imageUrl = imageElement.GetStyle().GetBackgroundImage().Split("\"")[1];

                string dateMiliseconds = dateElement.GetAttribute("datetime");
                DateTime date = (new DateTime(1970, 1, 1)).AddMilliseconds(Convert.ToDouble(dateMiliseconds));

                Uri youtubeLink = await GetYoutubeLinkAsync(linkMeetup);

                events.Add(new(title, summaryHtml, new Uri(imageUrl), linkMeetup, youtubeLink, date));
            }

            return events;

        }


        static async Task<Uri> GetYoutubeLinkAsync(Uri meetupLink)
        {
            var html = await GetPageHtmlAsync(meetupLink);

            int indexYoutube = html.IndexOf("https://www.youtube.com/watch?v=");

            var youtubeLink = html[indexYoutube..html.IndexOf("&quot", indexYoutube)];

            return new(youtubeLink);
        }

        static async Task UploadEventsAsync(Result result, string githubToken)
        {

            GitHubClient githubClient = new(new Octokit.ProductHeaderValue(nameof(MeetupWebScraping)));

            Octokit.Credentials basicAuth = new(githubToken);

            githubClient.Credentials = basicAuth;

            IGitHubService gitHubService = new GitHubService(githubClient);

            string path = "events";
            string fileName = "PastEvents";

            bool fileExist = await gitHubService.ExistFileAsync(251758832, path, fileName);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            if (fileExist)
            {
                await gitHubService.UpdateFileAsync(251758832, path, fileName, JsonSerializer.Serialize(result, options));
            }
            else
            {
                await gitHubService.CreateFileAsync(251758832, path, fileName, JsonSerializer.Serialize(result, options));
            }
        }
    }
}
