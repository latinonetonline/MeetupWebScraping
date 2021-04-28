using System;
using System.Collections.Generic;

namespace MeetupWebScraping.Models
{
    record Result(DateTime WebScrapingDate, IEnumerable<MeetupEvent> MeetupEvents);

}
