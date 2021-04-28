using GitHubActionSharp;
using System;

namespace WhoToGreetAction
{
    enum Parameters
    {
        [Parameter("who-to-greet")]
        WhoToGreet
    }

    class Program
    {
        static void Main(string[] args)
        {
            GitHubActionContext actionContext = new GitHubActionContext(args);
            actionContext.LoadParameters();

            string value = actionContext.GetParameter(Parameters.WhoToGreet);

            Console.WriteLine($"Hello {value}!");
        }
    }
}
