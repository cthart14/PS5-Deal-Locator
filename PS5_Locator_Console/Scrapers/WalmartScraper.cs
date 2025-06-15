using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FuzzySharp;
using Microsoft.Playwright;
using PS5_Locator_Console.Interfaces;
using PS5_Locator_Console.Models;

namespace PS5_Locator_Console.Scrapers
{
    public class WalmartScraper
    {
        private readonly IScraperHelper _scraperHelper;
        private readonly IItemHelper _ItemHelper;

        public WalmartScraper(IScraperHelper scraperHelper, IItemHelper ItemHelper)
        {
            _scraperHelper =
                scraperHelper ?? throw new ArgumentNullException(nameof(scraperHelper));
            _ItemHelper = ItemHelper ?? throw new ArgumentNullException(nameof(ItemHelper));
        }

        public async Task<List<ItemModel>> ScrapeWalmartAsync(string[] args)
        {
            var cleanedProducts = new List<ItemModel>();

            if (args == null || args.Length == 0)
            {
                Console.WriteLine("No search terms provided.");
                return cleanedProducts;
            }

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = false, // Set to false for debugging
                }
            );

            try
            {
                foreach (var arg in args)
                {
                    if (string.IsNullOrWhiteSpace(arg))
                    {
                        Console.WriteLine("Skipping empty search term.");
                        continue;
                    }

                    Console.WriteLine($"Searching Walmart for: {arg}");
                    var products = await ScrapeSearchTerm(browser, arg);
                    cleanedProducts.AddRange(products);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while scraping Walmart: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return cleanedProducts.OrderBy(p => p.Price).ToList();
        }

        private async Task<List<ItemModel>> ScrapeSearchTerm(IBrowser browser, string searchTerm)
        {
            var products = new List<ItemModel>();

            try
            {
                var normalizedSearchTerm = _ItemHelper.NormalizeSearchTerm(searchTerm);

                await using var context = await browser.NewContextAsync(
                    new BrowserNewContextOptions
                    {
                        Locale = "en-US",
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                        Permissions = new[] { "geolocation" },
                        Geolocation = new Geolocation
                        {
                            Latitude = 47.6062f,
                            Longitude = -122.3321f,
                        },
                    }
                );

                var page = await context.NewPageAsync();

                // Navigate to search results
                var encodedSearchTerm = Uri.EscapeDataString(normalizedSearchTerm);
                var searchUrl = $"https://www.walmart.com/search?q={encodedSearchTerm}";

                await page.GotoAsync(
                    searchUrl,
                    new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded }
                );

                // Wait for products to load
                Task.Delay(6000).Wait();

                try
                {
                    await page.WaitForSelectorAsync(
                        "div.mb0.ph0-xl.pt0-xl.bb.b--near-white.w-25.pb3-m.ph1",
                        new PageWaitForSelectorOptions { Timeout = 10000 }
                    );
                }
                catch (TimeoutException)
                {
                    Console.WriteLine($"No products found for {searchTerm} at Walmart");
                    return products;
                }

                // Scroll to load more products
                await _scraperHelper.ScrollAndLoadMoreAsync(page, 3, 800, 1500);

                var items = await page.QuerySelectorAllAsync(
                    "div.mb0.ph0-xl.pt0-xl.bb.b--near-white.w-25.pb3-m.ph1"
                );

                foreach (var item in items)
                {
                    try
                    {
                        var product = await ExtractWalmartProductInfo(item);
                        if (_ItemHelper.IsValidProduct(product, searchTerm, normalizedSearchTerm))
                        {
                            products.Add(product);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error extracting product info: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping search term '{searchTerm}': {ex.Message}");
            }

            Console.WriteLine($"Found {products.Count} products for {searchTerm} at Walmart.");
            return products;
        }

        private async Task<ItemModel> ExtractWalmartProductInfo(IElementHandle item)
        {
            var titleElement = await item.QuerySelectorAsync(
                "span[data-automation-id='product-title']"
            );
            var priceElement = await item.QuerySelectorAsync(
                "div[data-automation-id='product-price']"
            );
            var linkElement = await item.QuerySelectorAsync("a");

            var title = titleElement != null ? (await titleElement.InnerTextAsync()).Trim() : null;
            var link = linkElement != null ? await linkElement.GetAttributeAsync("href") : null;
            string priceText = "";

            if (priceElement != null)
            {
                var priceSpan = await priceElement.QuerySelectorAsync("span.w_iUH7");
                if (priceSpan != null)
                {
                    priceText = (await priceSpan.InnerTextAsync()).Trim();
                }
            }

            decimal? price = null;
            if (!string.IsNullOrEmpty(priceText))
            {
                var cleanPriceText = priceText
                    .Replace("$", "")
                    .Replace(",", "")
                    .Replace("current price", "")
                    .Replace("Now", "")
                    .Trim();

                if (decimal.TryParse(cleanPriceText, out var parsedPrice))
                {
                    price = parsedPrice;
                }
            }

            if (!string.IsNullOrEmpty(link) && !link.StartsWith("http"))
            {
                link = "https://www.walmart.com" + link;
            }

            return new ItemModel
            {
                Title = title,
                Price = price,
                Link = link,
                Store = "Walmart",
            };
        }
    }
}
