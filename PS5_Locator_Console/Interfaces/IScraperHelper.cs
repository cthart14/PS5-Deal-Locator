using System;
using Microsoft.Playwright;

namespace PS5_Locator_Console.Interfaces;

public interface IScraperHelper
{
    Task ScrollAndLoadMoreAsync(IPage page, int scrollCount, int scrollSpace, int delay);
}
