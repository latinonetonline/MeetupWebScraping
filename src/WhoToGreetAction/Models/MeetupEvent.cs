using System;

namespace MeetupWebScraping.Models
{
    record MeetupEvent(string Title, string Description, Uri Image, Uri MeetupLink, Uri YoutubeLink, DateTime StartDate);
}
