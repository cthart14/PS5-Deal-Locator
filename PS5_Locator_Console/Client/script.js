fetch("results.json")
  .then((res) => res.json())
  .then((data) => {
    const container = document.getElementById("deals-container");

    data.forEach((deal) => {
      const card = document.createElement("div");
      card.className = "deal-card";

      // Format the price to ensure it displays consistently
      const formattedPrice = typeof deal.Price === 'number' 
        ? `$${deal.Price.toFixed(2)}` 
        : `$${deal.Price}`;

      card.innerHTML = `
        <h3><a href="${deal.Link}" target="_blank">${deal.Title}</a></h3>
        
        <div class="card-content">
          <div class="card-body">
            ${deal.Image ? `<img src="${deal.Image}" alt="Product image" />` : ""}
          </div>
          
          <div class="card-footer">
            <div class="price-container">
              <strong>Price:</strong>
              <a href="${deal.Link}" target="_blank"><span class="price-value">${formattedPrice}</span></a>
            </div>
            
            <div class="store-info">
              <strong>Store:</strong>
              <span class="store-name">${deal.Store}</span>
            </div>
          </div>
        </div>
      `;

      container.appendChild(card);
    });
  })
  .catch((err) => {
    console.error("Failed to load JSON:", err);
    
    // Display a user-friendly error message
    const container = document.getElementById("deals-container");
    container.innerHTML = `
      <div style="text-align: center; color: #ff6b6b; font-size: 1.2em; margin-top: 50px;">
        <p>Oops! Unable to load deals at the moment.</p>
        <p>Please try refreshing the page.</p>
      </div>
    `;
  });