using System;
using System.Threading.Channels;
using PS5_Locator_Console.Interfaces;
using PS5_Locator_Console.Models;

namespace PS5_Locator_Console.Helpers;

public class ItemComparer : IItemComparer
{
    public List<ItemModel> CompareItemsByPrice(List<ItemModel> items, List<ItemModel> otherItems)
    {
        var idealItems = new List<ItemModel>();

        if (items == null || otherItems == null || items.Count == 0 || otherItems.Count == 0)
        {
            throw new ArgumentException("Items lists cannot be null or empty.");
        }
        else
        {
            try
            {
                var allProducts = items.Concat(otherItems).OrderBy(p => p.Price).ToList();

                idealItems = allProducts
                    .GroupBy(p => GetNormalizedTitle(p.Title))
                    .SelectMany(g => g.OrderBy(p => p.Price).Take(2))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while comparing items: {ex.Message}");
            }
        }

        return idealItems;
    }

    public string GetNormalizedTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "unknown";

        string normalized = title.ToLower();

        // Define keyword groups
        bool isPS5 = normalized.Contains("ps5") || normalized.Contains("playstation 5");
        bool isXbox = normalized.Contains("xbox");
        bool isConsole = normalized.Contains("console");
        bool isDigital = normalized.Contains("digital");
        bool isDisc = normalized.Contains("disc");
        bool isController = normalized.Contains("controller");
        bool isBundle = normalized.Contains("bundle");
        bool isGame = normalized.Contains("game");
        bool isAccessory = normalized.Contains("accessory") || normalized.Contains("accessories");
        bool isHeadset = normalized.Contains("headset") || normalized.Contains("headphones");

        // Build a simplified grouping key
        var keyParts = new List<string>();
        if (isPS5)
            keyParts.Add("ps5");
        if (isXbox)
            keyParts.Add("xbox");
        if (isConsole)
            keyParts.Add("console");
        if (isDigital)
            keyParts.Add("digital");
        if (isDisc)
            keyParts.Add("disc");
        if (isController)
            keyParts.Add("controller");
        if (isBundle)
            keyParts.Add("bundle");
        if (isGame)
            keyParts.Add("game");
        if (isAccessory)
            keyParts.Add("accessory");
        if (isHeadset)
            keyParts.Add("headset");

        if (keyParts.Count == 0)
        {
            return title.Trim();
        }

        return string.Join(" ", keyParts).Trim();
    }
}
