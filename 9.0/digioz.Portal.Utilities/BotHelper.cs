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
            "ahrefsbot", "dotbot", "rogerbot", "exabot", "facebot", "ia_archiver",
            "mj12bot", "screaming frog", "petalbot", "seznambot", "blex", "uptimerobot",
            "pingdom", "gptbot", "anthropic-ai", "claudebot", "bytespider", "sogou",
            "pinterestbot", "redditbot", "tumblr", "archive.org_bot", "wayback",
            "uptimebot", "statusbot", "better uptime", "site24x7", "siteimprove",
            "serpstatbot", "magpie-crawler", "adsbot-google", "amazonbot", "proximic",
            "applebot-extended", "applenewsbot", "apple-pubsub"
        };

        private static readonly Dictionary<string, string> BotPatterns = new()
        {
            { "googlebot", "Googlebot" },
            { "adsbot-google", "Google AdsBot" },
            { "mediapartners-google", "Google AdSense" },
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
            { "applebot-extended", "Applebot Extended" },
            { "applenewsbot", "Apple News Bot" },
            { "apple-pubsub", "Apple PubSub" },
            { "semrushbot", "SemrushBot" },
            { "ahrefsbot", "AhrefsBot" },
            { "dotbot", "DotBot" },
            { "msnbot", "MSNBot" },
            { "rogerbot", "Rogerbot" },
            { "exabot", "Exabot" },
            { "facebot", "Facebot" },
            { "ia_archiver", "Alexa Crawler" },
            { "mj12bot", "Majestic Bot" },
            { "screaming frog", "Screaming Frog SEO Spider" },
            { "petalbot", "PetalBot" },
            { "seznambot", "SeznamBot" },
            { "blexbot", "BLEXBot" },
            { "uptimerobot", "UptimeRobot" },
            { "pingdom", "Pingdom Bot" },
            { "gptbot", "OpenAI GPTBot" },
            { "anthropic-ai", "Anthropic AI" },
            { "claudebot", "Claude Bot" },
            { "bytespider", "ByteSpider (TikTok)" },
            { "sogou", "Sogou Spider" },
            { "pinterestbot", "Pinterest Bot" },
            { "redditbot", "Reddit Bot" },
            { "tumblr", "Tumblr Bot" },
            { "archive.org_bot", "Internet Archive Bot" },
            { "wayback", "Wayback Machine" },
            { "uptimebot", "Uptime Bot" },
            { "statusbot", "Status Bot" },
            { "better uptime", "Better Uptime Bot" },
            { "site24x7", "Site24x7 Bot" },
            { "siteimprove", "Siteimprove Bot" },
            { "serpstatbot", "SerpstatBot" },
            { "magpie-crawler", "Magpie Crawler" },
            { "amazonbot", "Amazon Bot" },
            { "proximic", "Proximic Spider" }
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
