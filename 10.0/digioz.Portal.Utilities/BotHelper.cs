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
            "applebot-extended", "applenewsbot", "apple-pubsub", "scooter", "ask jeeves",
            "feedfetcher", "gigabot", "heise", "heritrix", "iccrawler", "ichiro",
            "metagerbot", "ng-search", "nutch", "omniexplorer", "validator", "psbot",
            "seekbot", "sensis", "seosearch", "snappy", "synoobot", "turnitinbot",
            "voyager", "w3c", "wisenut", "yacy", "yahooseeker", "oai-searchbot",
            "perplexitybot", "duckduck", "fast", "neomo", "urltrends", "tkl.iis.u-tokyo",
            "telekom.de"
        };

        private static readonly Dictionary<string, string> BotPatterns = new()
        {
            // Google Bots
            { "googlebot", "Google Bot" },
            { "adsbot-google", "AdsBot [Google]" },
            { "mediapartners-google", "Google Adsense Bot" },
            { "feedfetcher-google", "Google Feedfetcher" },
            { "google desktop", "Google Desktop" },
            
            // Microsoft Bots
            { "bingbot", "Bing Bot" },
            { "msnbot-newsblogs", "MSN NewsBlogs" },
            { "msnbot-media", "MSNbot Media" },
            { "msnbot", "MSN Bot" },
            
            // Yahoo Bots
            { "yahoo-mmcrawler", "Yahoo MMCrawler Bot" },
            { "yahoo! de slurp", "Yahoo Slurp Bot" },
            { "yahoo! slurp", "Yahoo Bot" },
            { "yahooseeker", "YahooSeeker Bot" },
            { "slurp", "Yahoo Slurp Bot" },
            
            // Search Engine Bots
            { "duckduckbot", "DuckDuckGo Bot" },
            { "baiduspider", "Baidu Spider" },
            { "yandexbot", "YandexBot" },
            
            // SEO & Analytics Bots
            { "ahrefsbot", "Ahrefs Bot" },
            { "semrushbot", "Semrush Bot" },
            { "mj12bot", "Majestic-12 Bot" },
            { "dotbot", "DotBot" },
            { "rogerbot", "Rogerbot" },
            { "serpstatbot", "SerpstatBot" },
            
            // Social Media Bots
            { "facebookexternalhit", "Facebook Bot" },
            { "twitterbot", "Twitterbot" },
            { "linkedinbot", "LinkedInBot" },
            { "whatsapp", "WhatsApp Bot" },
            { "telegrambot", "Telegram Bot" },
            { "slackbot", "Slackbot" },
            { "discordbot", "Discordbot" },
            { "pinterestbot", "Pinterest Bot" },
            { "redditbot", "Reddit Bot" },
            { "tumblr", "Tumblr Bot" },
            
            // E-commerce & Cloud Bots
            { "amazonbot", "Amazon Bot" },
            { "applebot-extended", "Applebot Extended" },
            { "applenewsbot", "Apple News Bot" },
            { "apple-pubsub", "Apple PubSub" },
            { "applebot", "Applebot" },
            
            // AI Bots
            { "gptbot", "OpenAI GPTBot" },
            { "oai-searchbot", "OpenAI SearchBot" },
            { "anthropic-ai", "Anthropic AI" },
            { "claudebot", "Claude Bot" },
            { "perplexitybot", "PerplexityBot" },
            
            // Archive & Monitoring Bots
            { "ia_archiver", "Alexa Bot" },
            { "archive.org_bot", "Internet Archive Bot" },
            { "wayback", "Wayback Machine" },
            { "uptimerobot", "UptimeRobot" },
            { "uptimebot", "Uptime Bot" },
            { "statusbot", "Status Bot" },
            { "better uptime", "Better Uptime Bot" },
            { "pingdom", "Pingdom Bot" },
            { "site24x7", "Site24x7 Bot" },
            { "siteimprove", "Siteimprove Bot" },
            
            // Legacy Search Bots
            { "scooter", "Alta Vista Bot" },
            { "ask jeeves", "Ask Jeeves Bot" },
            { "wisenutbot", "WiseNut Bot" },
            
            // Crawlers & Spiders
            { "exabot", "Exabot Bot" },
            { "petalbot", "PetalBot" },
            { "bytespider", "ByteSpider (TikTok)" },
            { "sogou", "Sogou Spider" },
            { "facebot", "Facebot" },
            { "seznambot", "SeznamBot" },
            { "blexbot", "BLEXBot" },
            { "magpie-crawler", "Magpie Crawler" },
            { "proximic", "Proximic Spider" },
            { "screaming frog", "Screaming Frog SEO Spider" },
            
            // Special Purpose Bots
            { "fast enterprise crawler", "FAST Enterprise Crawler" },
            { "fast-webcrawler", "FAST WebCrawler Crawler" },
            { "neomo.de", "Francis Bot" },
            { "gigabot", "Gigabot Bot" },
            { "heise-it-markt-crawler", "Heise IT-Markt Crawler" },
            { "heritrix", "Heritrix Crawler" },
            { "ibm.com/cs/crawler", "IBM Research Bot" },
            { "iccrawler", "ICCrawler - ICjobs" },
            { "ichiro", "ichiro Crawler" },
            { "metagerbot", "Metager Bot" },
            { "ng-search", "NG-Search Bot" },
            { "nutchcvs", "Nutch/CVS Bot" },
            { "lucene.apache.org/nutch", "Nutch Bot" },
            { "nutch", "Nutch Bot" },
            { "omniexplorer_bot", "OmniExplorer Bot" },
            { "online link validator", "Online link Validator" },
            { "psbot", "psbot [Picsearch]" },
            { "seekbot", "Seekport Bot" },
            { "sensus web crawler", "Sensis Crawler" },
            { "seo search crawler", "SEO Crawler" },
            { "seoma", "Seoma Crawler" },
            { "seosearch", "SEOSearch Crawler" },
            { "urltrends.com", "Snappy Bot" },
            { "snappy", "Snappy Bot" },
            { "tkl.iis.u-tokyo.ac.jp", "Steeler Crawler" },
            { "synoobot", "Synoo Bot" },
            { "telekom.de", "Telekom Bot" },
            { "turnitinbot", "TurnitinBot Bot" },
            { "voyager", "Voyager Bot" },
            { "w3 sitesearch crawler", "W3 Sitesearch" },
            { "w3c-checklink", "W3C Linkcheck" },
            { "w3c_", "W3C Validator" },
            { "yacybot", "YaCy Bot" }
        };

        private static readonly HashSet<string> LegitimateSearchEngineBots = new(BotPatterns.Keys);

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

        public static bool IsLegitimateBot(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return false;

            var lowerUserAgent = userAgent.ToLowerInvariant();

            // Check if the user agent matches any legitimate bot pattern from our BotPatterns dictionary
            return BotPatterns.Keys.Any(key =>    
                LegitimateSearchEngineBots.Contains(key) && lowerUserAgent.Contains(key));
        }
    }
}
