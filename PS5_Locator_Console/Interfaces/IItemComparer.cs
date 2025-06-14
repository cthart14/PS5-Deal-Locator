using System;
using PS5_Locator_Console.Models;

namespace PS5_Locator_Console.Interfaces;

public interface IItemComparer
{
    List<ItemModel> GetBestPrices(List<ItemModel> items);
    string GetNormalizedTitle(string? title);
    string NormalizeSearchTerm(string searchTerm);
}
