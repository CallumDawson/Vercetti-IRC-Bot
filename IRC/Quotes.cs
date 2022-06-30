/*****************************************************************************
*
*  PROJECT:     IRC Utility/Development Bot
*  DEVELOPER:   Callum Dawson
*
*  All code herein is strictly closed source. It may not be used for other
*  projects, and may not be open sourced without permission.
*
*****************************************************************************/

using System;

namespace IRC
{
    class Quotes
    {
        static string[] quotes = {
            "Well thank you, Umberto. Nobody's said that to me since I left jail.",
            "No. I just wanted to piss you off before I killed you.",
            "This is the last dance for Lance Vance!",
            "What did I tell you before? NO GIANT SHARKS.",
            "What did I do wrong in a past life?",
            "Ok, Mr. Lance Vance Dance.",
            "Ok guys. Calm down, I'll handle this. Normally I wouldn't busy myself with driving around a bunch of drunken Scottish bisexuals, but in YOUR case, I'll make an exception!",
            "Look, it'll be easy, haven't you ever seen a movie? We walk into the bank, wave the guns around... and leave very rich men.",
            "As you can see, gentlemen... this is going to be the easiest buck we ever made.",
            "I ain't got no suntan.",
            "Go get some sleep.",
            "The more we learn now, the less we'll have to learn when we take this town over.",
            "Call my lawyer, Rosenberg.",
            "Okay, timers are set, five seconds and counting.",
            "Never forget the Second Amendment, asshole!",
            "Good thing I don't own this piece of crap.",
            "I did your wife, you poor bastard.",
            "Just think of this as socialism in action.",
            "What do you know about counterfeiting?",
            "I'll drop by your office tomorrow and we can start sorting this mess out.",
            "You took fifteen years from me, Sonny, and now I'm gonna make you pay!",
            "The last thing I needed was this. Maybe the last thing I needed was an enema, but this comes close."
        };

        public string GetRandomQuote()
        {
            Random random = new Random();
            return quotes[random.Next(quotes.Length)];
        }
    }
}
