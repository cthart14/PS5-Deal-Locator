using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using PS5_Locator_Console.Interfaces;

namespace PS5_Locator_Console.Helpers
{
    public class ScraperHelper : IScraperHelper
    {
        public async Task ScrollAndLoadMoreAsync(IPage page, int scrollCount, int scrollSpace, int delay)
        {
            for (int i = 0; i < scrollCount; i++)
            {
                await page.EvaluateAsync($"window.scrollBy(0, {scrollSpace})"); // Scroll down to load more products
                await Task.Delay(delay);
            }
        }
    }
}