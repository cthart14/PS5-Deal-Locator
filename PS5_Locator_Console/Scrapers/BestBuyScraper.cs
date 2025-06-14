using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FuzzySharp;
using Microsoft.Playwright;
using PS5_Locator_Console.Interfaces;
using PS5_Locator_Console.Models;

namespace PS5_Locator_Console.Scrapers
{
    public class BestBuyScraper
    {
        private readonly IScraperHelper _scraperHelper;
        private readonly IItemComparer _itemComparer;

        public BestBuyScraper(IScraperHelper scraperHelper, IItemComparer itemComparer)
        {
            _scraperHelper =
                scraperHelper ?? throw new ArgumentNullException(nameof(scraperHelper));
            _itemComparer = itemComparer ?? throw new ArgumentNullException(nameof(itemComparer));
        }

        public async Task<List<ItemModel>> ScrapeBestBuyAsync(string[] args)
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

                    Console.WriteLine($"Searching Best Buy for: {arg}");
                    var products = await ScrapeSearchTerm(browser, arg);
                    cleanedProducts.AddRange(products);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while scraping Best Buy: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return cleanedProducts.OrderBy(p => p.Price).ToList();
        }

        private async Task<List<ItemModel>> ScrapeSearchTerm(IBrowser browser, string searchTerm)
        {
            var products = new List<ItemModel>();

            try
            {
                var normalizedSearchTerm = _itemComparer.NormalizeSearchTerm(searchTerm);

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
                var searchUrl =
                    $"https://www.bestbuy.com/site/searchpage.jsp?st={encodedSearchTerm}";

                await page.GotoAsync(
                    searchUrl,
                    new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded }
                );

                // Wait for products to load
                try
                {
                    await page.WaitForSelectorAsync(
                        "li.product-list-item",
                        new PageWaitForSelectorOptions { Timeout = 10000 }
                    );
                }
                catch (TimeoutException)
                {
                    Console.WriteLine($"No products found for search term: {searchTerm}");
                    return products;
                }

                // Scroll to load more products
                await _scraperHelper.ScrollAndLoadMoreAsync(page, 3, 800, 1000);

                var items = await page.QuerySelectorAllAsync("li.product-list-item");

                foreach (var item in items)
                {
                    try
                    {
                        var product = await ExtractProductInfo(item);
                        if (IsValidProduct(product, searchTerm, normalizedSearchTerm))
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

            Console.WriteLine(
                $"Found {products.Count} products for search term '{searchTerm}' at Best Buy."
            );
            return products;
        }

        private async Task<ItemModel> ExtractProductInfo(IElementHandle item)
        {
            var titleElement = await item.QuerySelectorAsync("h2.product-title");
            var priceElement = await item.QuerySelectorAsync(
                "div.customer-price.medium, span.sr-only:has-text('current price')"
            );
            var linkElement = await item.QuerySelectorAsync("a");

            var title = titleElement != null ? (await titleElement.InnerTextAsync()).Trim() : null;
            var priceText =
                priceElement != null ? (await priceElement.InnerTextAsync()).Trim() : null;
            var link = linkElement != null ? await linkElement.GetAttributeAsync("href") : null;

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
                link = "https://www.bestbuy.com" + link;
            }

            return new ItemModel
            {
                Title = title,
                Price = price,
                Link = link,
                Store = "Best Buy",
            };
        }

        private bool IsValidProduct(
            ItemModel product,
            string originalSearchTerm,
            string normalizedSearchTerm
        )
        {
            if (string.IsNullOrEmpty(product.Title) || !product.Price.HasValue)
            {
                return false;
            }

            // Use fuzzy matching to ensure relevance
            var titleMatchScore = Math.Max(
                Fuzz.PartialRatio(normalizedSearchTerm, product.Title),
                Fuzz.PartialRatio(originalSearchTerm, product.Title)
            );

            return titleMatchScore > 60;
        }
    }
}
