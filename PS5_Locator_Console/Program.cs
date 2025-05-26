using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;

class Program
{
    public static async Task Main(string[] args)
    {
        using var playwright = await Playwright.CreateAsync();
    }
}
