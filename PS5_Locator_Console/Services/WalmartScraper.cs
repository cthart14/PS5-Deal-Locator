using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using FuzzySharp;
using Microsoft.Playwright;
using PS5_Locator_Console.Interfaces;
using PS5_Locator_Console.Models;

namespace PS5_Locator_Console.Services;

public class WalmartScraper
{
    private readonly IScraperHelper _scraperHelper;
    private readonly IItemComparer _itemComparer;

    public WalmartScraper(IScraperHelper scraperHelper, IItemComparer itemComparer)
    {
        _scraperHelper = scraperHelper;
        _itemComparer = itemComparer;
    }

    public async Task<List<ItemModel>> ScrapeBestBuyAsync(string[] args)
    {
        List<ItemModel> cleanedProducts = new List<ItemModel>();

        if (args.Length == 0)
        {
            throw new ArgumentNullException(nameof(args), "No search terms provided.");
        }

        try
        {
            foreach (var arg in args)
            {
                if (string.IsNullOrWhiteSpace(arg))
                {
                    Console.WriteLine("Skipping empty search term.");
                    continue;
                }

                var searchterm = _itemComparer.NormalizeSearchTerm(arg);

                using var playwright = await Playwright.CreateAsync();

                var browser = await playwright.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions
                    {
                        Headless = false, // set to true to hide the browser
                    }
                );

                var context = await browser.NewContextAsync(
                    new BrowserNewContextOptions
                    {
                        Permissions = new[] { "geolocation" },
                        Geolocation = new Geolocation
                        {
                            Latitude = 47.6062f,
                            Longitude = -122.3321f,
                        },
                        Locale = "en-US",
                    }
                );

                // Load Walmart and wait for items
                var page = await context.NewPageAsync();
                var encodedSearchterm = Uri.EscapeDataString(searchterm);
                await page.GotoAsync($"https://www.walmart.com/search?q={encodedSearchterm}");
                await Task.Delay(5000); // Wait for the page to load
                await _scraperHelper.ScrollAndLoadMoreAsync(page, 5, 800, 2000);

                await page.WaitForSelectorAsync("div.mb0.ph0-xl.pt0-xl.bb.b--near-white.w-25.pb3-m.ph1");

                var items = await page.QuerySelectorAllAsync("div.mb0.ph0-xl.pt0-xl.bb.b--near-white.w-25.pb3-m.ph1");
                List<ItemModel> products = new List<ItemModel>();

                foreach (var item in items)
                {
                    var titleElement = await item.QuerySelectorAsync("span.product-title");
                    var priceElement = await item.QuerySelectorAsync("div.product-price");
                    var linkElement = await item.QuerySelectorAsync("a");

                    var product = new ItemModel
                    {
                        Title =
                            titleElement != null
                                ? (await titleElement.InnerTextAsync()).Replace("-", "").Trim()
                                : null,
                        Price =
                            priceElement != null
                            && decimal.TryParse(
                                (await priceElement.InnerTextAsync())
                                    .Replace("$", "")
                                    .Replace(",", "")
                                    .Replace("currunt price Now", "").Trim(),
                                out var parsedPrice
                            )
                                ? parsedPrice
                                : null,
                        Link =
                            linkElement != null
                                ? await linkElement.GetAttributeAsync("href")
                                : null,

                        Store = "Best Buy",
                    };
                    products.Add(product);
                }

                products = products
                    .Where(p =>
                        p.Title != null
                        && (
                            Fuzz.PartialRatio(searchterm, p.Title) > 70
                            || Fuzz.PartialRatio(arg, p.Title) > 70
                        )
                        && p.Price != null
                    )
                    .ToList();

                cleanedProducts.AddRange(products);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while searching Best Buy: {ex.Message}");
        }

        return cleanedProducts.OrderBy(p => p.Price).ToList();
    }
}
