using System;
using PS5_Locator_Console.Models;

namespace PS5_Locator_Console.Interfaces;

public interface IItemHelper
{
    List<ItemModel> GetBestPrices(List<ItemModel> items);
    string GetNormalizedTitle(string? title);
    string NormalizeSearchTerm(string searchTerm);
    bool IsValidProduct(ItemModel product, string originalSearchTerm, string normalizedSearchTerm);
}
