import React, { useState } from "react";
import { Link } from "react-router-dom";
import {
  FiHeart,
  FiUser,
  FiGlobe,
  FiMenu,
  FiClock,
  FiHelpCircle,
} from "react-icons/fi";
import "./Header.css";

const Header = () => {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [isLangModalOpen, setIsLangModalOpen] = useState(false);

  return (
    <header className="modern-header">
      <Link to="/" className="logo">
        <span className="logo-red">tri</span>
        <span className="logo-orange">va</span>
        <span className="logo-blue">go</span>
      </Link>

      <nav className="header-right">
        <Link to="/favorites">
          <FiHeart /> Favorites
        </Link>
        <button onClick={() => setIsLangModalOpen(true)}>
          <FiGlobe /> EN · TL
        </button>
        <Link to="/login">
          <FiUser /> Log in
        </Link>
        <div className="menu-wrapper">
          <button onClick={() => setIsMenuOpen(!isMenuOpen)}>
            <FiMenu /> Menu
          </button>
          {isMenuOpen && (
            <div className="dropdown-menu">
              <Link to="/recent">
                <FiClock /> Recently viewed
              </Link>
              <Link to="/help">
                <FiHelpCircle /> Help and support
              </Link>
            </div>
          )}
        </div>
      </nav>

      {isLangModalOpen && (
        <div
          className="modal-overlay"
          onClick={() => setIsLangModalOpen(false)}
        >
          <div className="lang-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Select language and currency</h3>
              <button
                className="close-btn"
                onClick={() => setIsLangModalOpen(false)}
              >
                ×
              </button>
            </div>
            <div className="modal-body">
              <label>Language</label>
              <select>
                <option>English</option>
              </select>
              <label>Currency</label>
              <select>
                <option>TRY - Turkish Lira</option>
              </select>
              <button className="apply-btn">Apply</button>
            </div>
          </div>
        </div>
      )}
    </header>
  );
};

export default Header;
