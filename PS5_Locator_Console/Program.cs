using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using PS5_Locator_Console.Helpers;
using PS5_Locator_Console.Interfaces;
using PS5_Locator_Console.Models;
using PS5_Locator_Console.Services;

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
        var _bestBuyScrapper = new BestBuyScrapper(scraperHelper, itemComparer);

        try
        {
            var bestBuyProducts = await _bestBuyScrapper.ScrapeBestBuyAsync(args);

            var deals = itemComparer.CompareItemsByPrice(bestBuyProducts, new List<ItemModel>());

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
