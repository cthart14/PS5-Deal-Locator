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
    private readonly IItemHelper _ItemHelper;

    public Program(IScraperHelper scraperHelper, IItemHelper ItemHelper)
    {
        _scraperHelper = scraperHelper;
        _ItemHelper = ItemHelper;
    }

    public static async Task Main(string[] args)
    {
        var scraperHelper = new ScraperHelper();
        var ItemHelper = new ItemHelper();
        var _bestBuyScraper = new BestBuyScraper(scraperHelper, ItemHelper);
        var _walmartScraper = new WalmartScraper(scraperHelper, ItemHelper);
        var _targetScraper = new TargetScraper(scraperHelper, ItemHelper);
        var _amazonScraper = new AmazonScraper(scraperHelper, ItemHelper);

        var returnModel = new ProductsModel();

        try
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine(
                    "No search terms provided. Please provide search terms as command line arguments."
                );
                return;
            }

            Console.WriteLine("\nSearching for products...");

            var bestBuyTask = _bestBuyScraper.ScrapeBestBuyAsync(args);
            var walmartTask = _walmartScraper.ScrapeWalmartAsync(args);
            var targetTask = _targetScraper.ScrapeTargetAsync(args);
            var amazonTask = _amazonScraper.ScrapeAmazonAsync(args);

            var results = await Task.WhenAll(bestBuyTask, walmartTask, targetTask, amazonTask);
            var bestBuyProducts = results[0];
            var walmartProducts = results[1];
            var targetProducts = results[2];
            var amazonProducts = results[3];

            if (bestBuyProducts.Any())
            {
                bestBuyProducts = ItemHelper.GetBestPrices(bestBuyProducts);
                returnModel.best_Buy = bestBuyProducts;
            }

            if (walmartProducts.Any())
            {
                walmartProducts = ItemHelper.GetBestPrices(walmartProducts);
                returnModel.walmart = walmartProducts;
            }

            if (targetProducts.Any())
            {
                targetProducts = ItemHelper.GetBestPrices(targetProducts);
                returnModel.target = targetProducts;
            }

            if (amazonProducts.Any())
            {
                amazonProducts = ItemHelper.GetBestPrices(amazonProducts);
                returnModel.amazon = amazonProducts;
            }

            var props = typeof(ProductsModel).GetProperties();
            var deals = new List<ItemModel>();

            // Check if any property has items and display results
            foreach (var prop in props)
            {
                if (prop.GetValue(returnModel) is List<ItemModel> items && items.Any())
                {
                    DisplayResults(returnModel);
                    deals = ItemHelper.GetBestDeals(returnModel);
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
