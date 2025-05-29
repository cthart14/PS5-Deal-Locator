using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Playwright;
using PS5_Locator_Console.Interfaces;
using PS5_Locator_Console.Models;

namespace PS5_Locator_Console.Services;

public class BestBuyScrapper
{
    private readonly IScraperHelper _scraperHelper;

    public BestBuyScrapper(IScraperHelper scraperHelper)
    {
        _scraperHelper = scraperHelper;
    }

    public async Task<List<ItemModel>> ScrapeBestBuyAsync(string[] args)
    {
        List<ItemModel> cleanedProducts = new List<ItemModel>();

        foreach (var arg in args)
        {
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
                    Geolocation = new Geolocation { Latitude = 47.6062f, Longitude = -122.3321f },
                    Locale = "en-US",
                }
            );

            // Load BestBuy and wait for items
            var page = await context.NewPageAsync();
            await page.GotoAsync($"https://www.bestbuy.com/site/searchpage.jsp?st={arg}");
            await Task.Delay(5000); // Wait for the page to load
            await _scraperHelper.ScrollAndLoadMoreAsync(page, 5, 800, 2000);

            await page.WaitForSelectorAsync("li.product-list-item");

            var items = await page.QuerySelectorAllAsync("li.product-list-item");
            List<ItemModel> products = new List<ItemModel>();

            foreach (var item in items)
            {
                var titleElement = await item.QuerySelectorAsync("h2.product-title");
                var priceElement = await item.QuerySelectorAsync("div.customer-price.medium");
                var linkElement = await item.QuerySelectorAsync("a");

                var product = new ItemModel
                {
                    Title =
                        titleElement != null
                            ? (await titleElement.InnerTextAsync()).Trim()
                            : "No Title",
                    Price =
                        priceElement != null
                        && decimal.TryParse(
                            (await priceElement.InnerTextAsync()).Replace("$", "").Replace(",", ""),
                            out var parsedPrice
                        )
                            ? parsedPrice
                            : null,
                    Link =
                        linkElement != null
                            ? await linkElement.GetAttributeAsync("href")
                            : "No Link",

                    Store = "Best Buy",
                };
                products.Add(product);
            }

            products = products
                .Where(p =>
                    (p.Title?.Contains(arg, StringComparison.OrdinalIgnoreCase) == true)
                    && p.Price != null
                )
                .ToList();

            cleanedProducts.AddRange(products);
        }

        return cleanedProducts.OrderBy(p => p.Price).ToList();
    }
}
