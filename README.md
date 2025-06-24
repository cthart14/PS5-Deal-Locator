# 🎮 PS5 Deal Locator

<div align="center">

![PS5 Banner](https://img.shields.io/badge/PS5-Deal%20Hunter-blue?style=for-the-badge&logo=playstation&logoColor=white)
![Status](https://img.shields.io/badge/Status-Active%20Development-green?style=for-the-badge)
![Stores](https://img.shields.io/badge/Stores-5%20(More%20Coming!)-orange?style=for-the-badge)

**🔥 Find the best PS5 deals across the web - because every gamer deserves to play! 🔥**

[🚀 Quick Start](#-quick-start) • [🏪 Supported Stores](#-supported-stores) • [🛣️ Roadmap](#️-roadmap) • [🤝 Contributing](#-contributing)

</div>

---

## 🎯 The Mission

This project started when my PS5 began overheating, making it completely unplayable. Instead of paying outragous prices or settling for overpriced bundles, I decided to build a solution that helps fellow gamers find legitimate deals across major retailers.

**If you're struggling to find a reasonably priced PS5, this tool is for you.**

---

## ✨ Features

- 🔍 **Smart Price Scraping** - Automatically finds current PS5 prices across stores
- ⚡ **Lightning Fast** - Console app delivers results in seconds
- 💰 **Best Price Comparison** - Shows the lowest price for each product
- 🎯 **Clean Console Output** - Easy to read price listings
- 🔧 **Modular Design** - Built with interfaces to easily add new stores
- 📊 **Multiple Products** - Tracks standard consoles, digital editions, and bundles

---

## 🏪 Supported Stores

| Store | Status | Price Tracking | Implementation |
|-------|--------|----------------|----------------|
| 🛒 **Best Buy** | ✅ Active | ✅ Yes | ✅ Complete |
| 🎯 **Target** | ✅ Active | ✅ Yes | ✅ Complete |
| 🛍️ **Walmart** | ✅ Active | ✅ Yes | ✅ Complete |
| 📦 **Amazon** | ✅ Active | ✅ Yes | ✅ Complete |
| 🎮 **GameStop** | ✅ Active | ✅ Yes | ✅ Complete |

*More stores coming soon! Have a suggestion? [Open an issue!](../../issues)*

---

## 🚀 Quick Start

### Prerequisites
```bash
# Make sure you have .NET 6.0+ installed
dotnet --version
```

### Installation
```bash
# Clone the repository
git clone https://github.com/yourusername/PS5-Deal-Locator.git
cd PS5-Deal-Locator

# Restore NuGet packages
dotnet restore
```

### Usage
```bash
# Build and run the console app
dotnet run "Your Product"

# If searching for multiple products 
dotnet run "Product", "Other Product" ..etc.

# Watch the magic happen! 🎉
```

---

## 📊 Sample Output

```
PS5 Deal Locator - Finding the best prices...

Scanning Stores for "search term"

Found # items at "searched store"

PRODUCTS FOUND:

BEST BUY:
├─ PS5 Console (Standard): $499.99
├─ PS5 Console (Digital): $399.99
└─ PS5 Spider-Man Bundle: $559.99

WALMART:
├─ PS5 Console (Standard): $499.99
├─ PS5 Console (Digital): $399.99
└─ PS5 Spider-Man Bundle: $559.99

BEST DEALS FOUND:
Standard Console: $499.99 (Best Buy)
Digital Console: $399.99 (Best Buy)
Spider-Man Bundle: $559.99 (Best Buy)

```

---

## 🛣️ Roadmap

### Phase 1: Foundation ✅
- [x] Best Buy scraper implementation
- [x] Basic price comparison
- [x] Clean console output

### Phase 2: Expansion 🔄
- [x] Add Target scraper implementation
- [x] Add Walmart scraper implementation  
- [x] Add Amazon scraper implementation
- [x] Add GameStop scraper implementation  
- [x] Multi-threading for faster scanning

### Phase 3: Enhancement 🔮
- [ ] Export results to JSON/CSV
- [x] Configuration file support
- [ ] Price history logging
- [x] Command-line arguments
- [ ] API Intergration

---

## 🔧 Technical Details

**Built with:**
- ☄️ C# .NET 6.0+
- 🌐 HttpClient for web requests
- 📊 HtmlAgilityPack for HTML parsing
- ⚡ Async/await for performance
- 🎯 Clean console application architecture

**Architecture:**
```
PS5-Deal-Locator/
├── Scrapers/
│   ├── ...Scraper.cs    # All Store Scrapers implementation
├── Interfaces
│   ├──  I...Helper.cs   # All Helper logic interfaces for depedency logic
├── Models/
│   ├── Product.cs           # Product data model
│   └── Store.cs            # Store information
├── Scrapers/
│   └── PriceComparer.cs    # Price comparison logic
├── Helpers/
│   ├── ProductModel.cs  # Product data model
│   └── ItemModel.cs     # Item data model
├── Helpers/
│   |── ItemHelper.cs    # Price comparison logic
|   └── ScraperHelper.cs  # Website Scraping help logic
├── Program.cs              # Main console application
└── PS5-Deal-Locator.csproj # Project file
```

---

## 🤝 Contributing

Found a bug? Want to add a store? Have ideas for improvement? **I'd love your help!**

### How to Contribute:
1. 🍴 Fork the repository
2. 🌟 Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. 💻 Make your changes
4. ✅ Test thoroughly
5. 📤 Commit your changes (`git commit -m 'Add some AmazingFeature'`)
6. 🚀 Push to the branch (`git push origin feature/AmazingFeature`)
7. 🎉 Open a Pull Request

### Contribution Ideas:
- 🏪 Add new store scrapers
- 🔔 Implement notification systems
- 🎨 Improve output formatting
- 📱 Build mobile integration
- 🐛 Fix bugs and improve performance

---

## ⚠️ Important Notes

- **Respectful Scraping**: This tool includes delays and follows robots.txt guidelines
- **Personal Use**: Intended for personal deal hunting, not commercial reselling
- **No Guarantees**: Prices and stock can change rapidly - always verify on the retailer's site
- **Rate Limiting**: Built-in delays prevent overwhelming retailer servers

---

This tool levels the playing field by automating the tedious process of checking multiple stores, giving you more time to actually **play games** instead of hunting for consoles.

---

## 🙏 Acknowledgments

- **The Gaming Community** - for sharing restock tips and deal alerts
- **Fellow Developers** - for open source scraping libraries and tools
- **Retailers** - for maintaining public APIs and reasonable access policies

---

<div align="center">


**If this tool helped you snag a PS5 deal, consider giving it a ⭐!**

[![Star This Repo](https://img.shields.io/github/stars/cthart14/PS5-Deal-Locator?style=social)](../../stargazers)
[![Follow Me](https://img.shields.io/github/followers/cthart14?style=social)](../../followers)

 👤 [Check Out My Profile](https://www.github.com/cthart14)

*Built by a gamer, for gamers*

</div>

---

## 📞 Support

Having issues? Found a bug? Want to chat about gaming?

- 🐛 [Report Issues](../../issues)
- 💬 [Start a Discussion](../../discussions)
- 📧 [Email Me](mailto:your.email@example.com)

