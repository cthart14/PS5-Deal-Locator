using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Channels;
using PS5_Locator_Console.Interfaces;
using PS5_Locator_Console.Models;

namespace PS5_Locator_Console.Helpers;

public class ItemComparer : IItemComparer
{
    public List<ItemModel> GetBestPrices(List<ItemModel> items)
    {
        // Handle null inputs
        if (!items.Any())
            return new List<ItemModel>();

        try
        {
            var idealItems = items
                .GroupBy(p => new { p.Store, NormalizedTitle = GetNormalizedTitle(p.Title) })
                .SelectMany(g => g.OrderBy(p => p.Price).Take(3))
                .ToList();
            return idealItems;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while comparing items: {ex.Message}");
            return new List<ItemModel>();
        }
    }

    public List<ItemModel> GetBestDeals(ProductsModel products)
    {
        var deals = new List<ItemModel>();

        if (products != null)
        {
            var props = typeof(ProductsModel).GetProperties();

            foreach (var prop in props)
            {
                var items = prop.GetValue(products) as List<ItemModel>;
                if (items != null && items.Any())
                {
                    deals.AddRange(items.OrderBy(item => item.Price).Take(3));
                }
            }

            deals = deals.OrderBy(item => item.Price).Take(5).ToList();
        }

        return deals;
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
            keyParts.Add("playstation 5");
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

    public string NormalizeSearchTerm(string searchTerm)
    {
        searchTerm = searchTerm.ToLower();

        var keywords = new[] { "ps5", "pro", "slim", "digital", "xboxsx", "xbox 1" };
        var replacements = new[]
        {
            "Playstation 5",
            "Pro",
            "Slim",
            "Digital",
            "XBOX Series X",
            "XBOX 1",
        };

        for (int i = 0; i < keywords.Length; i++)
        {
            searchTerm = searchTerm.Replace(keywords[i], replacements[i]);
        }

        var clensedSearchTerm = searchTerm.Replace(@"\d", "").Trim();

        foreach (var replacement in replacements)
        {
            clensedSearchTerm = clensedSearchTerm.Replace(replacement, "");
        }

        if (clensedSearchTerm.Trim() == string.Empty)
        {
            searchTerm += " Console";
        }

        return searchTerm.Trim();
    }
}
