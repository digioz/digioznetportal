using System;
using System.Collections.Generic;
using System.Linq;

namespace digioz.Portal.Utilities
{
    public static class BotHelper
    {
        private static readonly string[] BotKeywords = new[]
        {
            "bot", "crawler", "spider", "crawling", "slurp", "mediapartners",
            "bingbot", "googlebot", "msnbot", "yahoo", "baiduspider", "facebookexternalhit",
            "twitterbot", "linkedinbot", "whatsapp", "telegrambot", "slackbot",
            "discordbot", "applebot", "duckduckbot", "yandexbot", "semrushbot",
            "ahrefsbot", "dotbot", "rogerbot", "exabot", "facebot", "ia_archiver"
        };

        private static readonly Dictionary<string, string> BotPatterns = new()
        {
            { "googlebot", "Googlebot" },
            { "bingbot", "Bingbot" },
            { "slurp", "Yahoo Slurp" },
            { "duckduckbot", "DuckDuckBot" },
            { "baiduspider", "Baiduspider" },
            { "yandexbot", "YandexBot" },
            { "facebookexternalhit", "Facebook Bot" },
            { "twitterbot", "Twitterbot" },
            { "linkedinbot", "LinkedInBot" },
            { "whatsapp", "WhatsApp Bot" },
            { "telegrambot", "Telegram Bot" },
            { "slackbot", "Slackbot" },
            { "discordbot", "Discordbot" },
            { "applebot", "Applebot" },
            { "semrushbot", "SemrushBot" },
            { "ahrefsbot", "AhrefsBot" },
            { "dotbot", "DotBot" },
            { "msnbot", "MSNBot" },
            { "rogerbot", "Rogerbot" },
            { "exabot", "Exabot" },
            { "facebot", "Facebot" },
            { "ia_archiver", "Alexa Crawler" }
        };

        public static bool IsBot(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return false;

            var lowerUserAgent = userAgent.ToLowerInvariant();
            return BotKeywords.Any(keyword => lowerUserAgent.Contains(keyword));
        }

        public static string ExtractBotName(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown Bot";

            var lowerUserAgent = userAgent.ToLowerInvariant();

            foreach (var pattern in BotPatterns.Where(p => lowerUserAgent.Contains(p.Key)))
            {
                return pattern.Value;
            }

            return "Unknown Bot";
        }
    }
}
