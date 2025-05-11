// src/pages/home/index.js
import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useIsAuthenticated } from "@azure/msal-react";
import SearchBar from "../../components/SearchBar";
import BannerSlider from "../../components/BannerSlider";
import { getCurrentUser } from "../../services/api";
import "./HomePage.css";

const exploreDestinations = [
  { name: "Istanbul", image: "/images/istanbul.jpg" },
  { name: "Antalya", image: "/images/antalya.jpg" },
  { name: "Bodrum", image: "/images/Bodrum.jpg" },
];

const HomePage = () => {
  const isAuth = useIsAuthenticated();
  const navigate = useNavigate();
  const [searchPerformed, setSearchPerformed] = useState(false);
  const [user, setUser] = useState(null);

  useEffect(() => {
    // Sadece login olduktan sonra API çağır
    if (!isAuth) return;

    getCurrentUser()
      .then((res) => {
        setUser(res);
        console.log("Aktif kullanıcı:", res);
      })
      .catch((err) =>
        console.log("Kullanıcı bilgisi alınamadı:", err.message)
      );
  }, [isAuth]);

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

      {/* 👤 Login olmuş kullanıcıyı karşılama */}
      {user && (
        <div
          className="welcome-user"
          style={{ margin: "1rem 0", fontSize: "1.1rem" }}
        >
          👋 Welcome, <strong>{user.name || user.email}</strong>
        </div>
      )}

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
