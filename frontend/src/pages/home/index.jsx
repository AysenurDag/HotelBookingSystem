import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import SearchBar from "../../components/SearchBar";
import BannerSlider from "../../components/BannerSlider";
import "./HomePage.css";

const exploreDestinations = [
  {
    name: "Istanbul",
    image: "/images/istanbul.jpg",
  },
  {
    name: "Antalya",
    image: "/images/antalya.jpg",
  },
  {
    name: "Bodrum",
    image: "/images/Bodrum.jpg",
  },
];

const HomePage = () => {
  const navigate = useNavigate();
  const [searchPerformed, setSearchPerformed] = useState(false);

  const handleSearch = () => {
    setSearchPerformed(true);
  };

  const handleExploreClick = (destination) => {
    navigate(`/results?destination=${encodeURIComponent(destination)}`);
  };

  return (
    <div className="home-container">
      <section className="hero-banner">
        <BannerSlider />
      </section>

      <SearchBar initialValues={{}} onSearch={handleSearch} />

      {!searchPerformed ? (
        <section className="inspire-section">
          <h2>Explore Popular Destinations</h2>
          <div className="promo-grid">
            {exploreDestinations.map((dest) => (
              <div
                key={dest.name}
                className="promo-card"
                onClick={() => handleExploreClick(dest.name)}
              >
                <img
                  src={dest.image}
                  alt={dest.name}
                  className="promo-image"
                  onError={(e) =>
                    (e.target.src =
                      "https://via.placeholder.com/300x200?text=No+Image")
                  }
                />
                <div className="promo-overlay">{dest.name}</div>
              </div>
            ))}
          </div>
        </section>
      ) : (
        <p style={{ marginTop: "2rem" }}>
          Hotel results will appear here (mock)
        </p>
      )}
    </div>
  );
};

export default HomePage;
