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
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("No search terms provided. Please provide search terms as command line arguments.");
                return;
            }

            Console.WriteLine("\nSearching for products...");

            var bestBuyTask = _bestBuyScraper.ScrapeBestBuyAsync(args);
            var walmartTask = _walmartScraper.ScrapeWalmartAsync(args);

            var results = await Task.WhenAll(bestBuyTask, walmartTask);
            var bestBuyProducts = results[0];
            var walmartProducts = results[1];

            if (bestBuyProducts.Any())
            {
                bestBuyProducts = itemComparer.GetBestPrices(bestBuyProducts);
                returnModel.best_Buy = bestBuyProducts;
            }

            if (walmartProducts.Any())
            {
                walmartProducts = itemComparer.GetBestPrices(walmartProducts);
                returnModel.walmart = walmartProducts;
            }

            var props = typeof(ProductsModel).GetProperties();
            var deals = new List<ItemModel>();

            // Check if any property has items and display results
            foreach (var prop in props)
            {
                if (prop.GetValue(returnModel) is List<ItemModel> items && items.Any())
                {
                    DisplayResults(returnModel);
                    deals = itemComparer.GetBestDeals(returnModel);
                    break;
                }
            }

            if (deals.Any())
            {
                Console.WriteLine("\nBEST DEALS:");
                Console.WriteLine(new string('-', 50));
                foreach (var deal in deals)
                {
                    Console.WriteLine(
                        $"|--- {deal.Title?.ToUpper()} | ${deal.Price} | ({deal.Store?.ToUpper()})\n    \u2514-- {deal.Link} \n"
                    );
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
                Console.WriteLine($"\n{prop.Name.Replace("_", " ").ToUpper()} RESULTS:");
                Console.WriteLine(new string('-', 50));
                foreach (var item in items)
                {
                    Console.WriteLine(
                        $"|--- {item.Title?.ToUpper()} | ${item.Price}\n    \u2514-- {item.Link} \n"
                    );
                }
            }
        }
    }
}
