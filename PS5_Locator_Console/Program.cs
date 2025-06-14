using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using PS5_Locator_Console.Helpers;
using PS5_Locator_Console.Interfaces;
using PS5_Locator_Console.Models;
using PS5_Locator_Console.Scrapers;

namespace PS5_Locator_Console;

class Program
{
    private readonly IScraperHelper _scraperHelper;
    private readonly IItemComparer _itemComparer;

    public Program(IScraperHelper scraperHelper, IItemComparer itemComparer)
    {
        _scraperHelper = scraperHelper;
        _itemComparer = itemComparer;
    }

    public static async Task Main(string[] args)
    {
        var scraperHelper = new ScraperHelper();
        var itemComparer = new ItemComparer();
        var _bestBuyScraper = new BestBuyScraper(scraperHelper, itemComparer);
        var _walmartScraper = new WalmartScraper(scraperHelper, itemComparer);

        var returnModel = new ProductsModel();

        try
        {
            var bestBuyTask = _bestBuyScraper.ScrapeBestBuyAsync(args);
            var walmartTask = _walmartScraper.ScrapeWalmartAsync(args);

            var results = await Task.WhenAll(bestBuyTask, walmartTask);
            var bestBuyProducts = results[0];
            var walmartProducts = results[1];

            if (bestBuyProducts.Any())
            {
                bestBuyProducts = itemComparer.FindDeals(bestBuyProducts);
                returnModel.bestBuy = bestBuyProducts;
            }

            if (walmartProducts.Any())
            {
                walmartProducts = itemComparer.FindDeals(walmartProducts);
                returnModel.walmart = walmartProducts;
            }

            var props = typeof(ProductsModel).GetProperties();

            foreach (var prop in props)
            {
                if (prop.GetValue(returnModel) is List<ItemModel> items && items.Any())
                {
                    DisplayResults(returnModel);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    public static void DisplayResults(ProductsModel model)
    {
        var props = typeof(ProductsModel).GetProperties();

        foreach (var prop in props)
        {
            var items = prop.GetValue(model) as List<ItemModel>;
            if (items != null && items.Any())
            {
                Console.WriteLine($"{prop.Name.ToUpper()} Results:");
                foreach (var item in items)
                {
                    Console.WriteLine(
                        $"|--- {item.Title.ToUpper()} | ${item.Price}\n \t|- {item.Link}"
                    );
                }
            }
        }
    }
}
