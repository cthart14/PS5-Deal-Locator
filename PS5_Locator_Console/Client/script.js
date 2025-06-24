

fetch("results.json")
  .then((res) => res.json())
  .then((data) => {
    const container = document.getElementById("deals-container");

    data.forEach((deal) => {
      const card = document.createElement("div");
      card.className = "deal-card";

      card.innerHTML = `
        <h3><a href="${deal.Link}" target="_blank">${deal.Title}</a></h3>
        <p><strong>Price:</strong> ${deal.Price}</p>
        <p><strong>Store:</strong> ${deal.Store}</p>
        ${deal.ImageUrl ? `<img src="${deal.ImageUrl}" alt="picture not available" />` : ""}
      `;

      container.appendChild(card);
    });
  })
  .catch((err) => {
    console.error("Failed to load JSON:", err);
  });
