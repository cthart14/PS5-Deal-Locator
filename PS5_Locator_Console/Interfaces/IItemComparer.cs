using System;
using PS5_Locator_Console.Models;

namespace PS5_Locator_Console.Interfaces;

public interface IItemComparer
{
    List<ItemModel> CompareItemsByPrice(List<ItemModel> items, List<ItemModel> existingItems);
    string GetNormalizedTitle(string? title);
}
