using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using PS5_Locator_Console.Helpers;
using PS5_Locator_Console.Models;
using PS5_Locator_Console.Services;

namespace PS5_Locator_Console;

class Program
{
    public static async Task Main(string[] args)
    {
        var _scraperHelper = new ScraperHelper();
        var _itemComparer = new ItemComparer();
        var _bestBuyScrapper = new BestBuyScrapper(_scraperHelper);

        try
        {
            var bestBuyProducts = await _bestBuyScrapper.ScrapeBestBuyAsync(args);

            var deals = _itemComparer.CompareItemsByPrice(bestBuyProducts, new List<ItemModel>());

            foreach (var deal in deals)
            {
                Console.WriteLine($"{deal.Title} - {deal.Price:C} @ {deal.Store} \n {deal.Link}");
                Console.WriteLine(new string('-', 50));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
