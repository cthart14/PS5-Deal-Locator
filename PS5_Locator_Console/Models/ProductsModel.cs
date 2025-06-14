using System;

namespace PS5_Locator_Console.Models;

public class ProductsModel
{
    public List<ItemModel>? best_Buy { get; set; }
    public List<ItemModel>? walmart { get; set; }
    public List<ItemModel>? amazon { get; set; }
    public List<ItemModel>? target { get; set; }
}
