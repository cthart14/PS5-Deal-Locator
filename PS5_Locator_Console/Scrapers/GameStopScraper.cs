using System;
using Microsoft.Playwright;
using PS5_Locator_Console.Interfaces;
using PS5_Locator_Console.Models;

namespace PS5_Locator_Console.Scrapers;

public class GameStopScraper
{
    private readonly IScraperHelper _scraperHelper;
    private readonly IItemHelper _ItemHelper;

    public GameStopScraper(IScraperHelper scraperHelper, IItemHelper ItemHelper)
    {
        _scraperHelper = scraperHelper ?? throw new ArgumentNullException(nameof(scraperHelper));
        _ItemHelper = ItemHelper ?? throw new ArgumentNullException(nameof(ItemHelper));
    }

    public async Task<List<ItemModel>> ScrapeGameStopAsync(string[] args)
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

                Console.WriteLine($"Searching GameStop for: {arg}");
                var products = await ScrapeSearchTerm(browser, arg);
                cleanedProducts.AddRange(products);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while scraping GameStop: {ex.Message}");
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
                    Geolocation = new Geolocation { Latitude = 47.6062f, Longitude = -122.3321f },
                }
            );

            var page = await context.NewPageAsync();

            // Navigate to search results
            var encodedSearchTerm = Uri.EscapeDataString(normalizedSearchTerm);
            var searchUrl = $"https://www.gamestop.com/search/?q={encodedSearchTerm}";

            await page.GotoAsync(
                searchUrl,
                new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded }
            );

            // Wait for products to load
            Task.Delay(10000).Wait();

            try
            {
                await page.WaitForSelectorAsync(
                    "div.product-grid-tile-wrapper",
                    new PageWaitForSelectorOptions { Timeout = 10000 }
                );
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"No products found for {searchTerm} at GameStop");
                return products;
            }

            // Scroll to load more products
            await _scraperHelper.ScrollAndLoadMoreAsync(page, 3, 800, 1500);

            var items = await page.QuerySelectorAllAsync("div.product-grid-tile-wrapper");

            foreach (var item in items)
            {
                try
                {
                    var product = await ExtractBestBuyProductInfo(item);
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

        Console.WriteLine($"Found {products.Count} products for {searchTerm} at GameStop.");
        return products;
    }

    private async Task<ItemModel> ExtractBestBuyProductInfo(IElementHandle item)
    {
        var titleElement = await item.QuerySelectorAsync("div.render-tile-name");
        var priceElement = await item.QuerySelectorAsync("div.render-sale-price");
        var linkElement = await item.QuerySelectorAsync("a");
        var imageElement = await item.QuerySelectorAsync("div.render-tile-image");

        var title = titleElement != null ? (await titleElement.InnerTextAsync()).Trim() : null;
        var priceText = priceElement != null ? (await priceElement.InnerTextAsync()).Trim() : null;
        var link = linkElement != null ? await linkElement.GetAttributeAsync("href") : null;
        var image = imageElement != null ? await imageElement.GetAttributeAsync("style") : null;

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
            link = "https://www.gamestop.com" + link;
        }

        if (!string.IsNullOrEmpty(image))
        {
            int openParen = image.IndexOf('(');
            if (openParen >= 0)
            {
                int closeParen = image.IndexOf(')', openParen + 1);
                if (closeParen > openParen)
                {
                    // +1 to skip the '(' itself
                    image = image.Substring(openParen + 1, closeParen - openParen - 1);
                }
            }
        }

        return new ItemModel
        {
            Title = title,
            Price = price,
            Link = link,
            Store = "GameStop",
            Image = image,
        };
    }
}
