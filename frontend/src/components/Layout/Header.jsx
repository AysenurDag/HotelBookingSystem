// src/components/Header.js
import React from "react";
import { Link } from "react-router-dom";
import { useIsAuthenticated } from "@azure/msal-react";
import { msalInstance } from "../../msalInstance";
import "./Header.css";

const Header = () => {
  const isAuth = useIsAuthenticated();

  const handleLogout = () => {
    msalInstance.logoutRedirect({
      postLogoutRedirectUri: "http://localhost:3000",
    });
  };

  return (
    <header className="modern-header">
      <div className="header-left">
        <Link to="/" className="logo">
          trivago
        </Link>
      </div>
      <nav className="header-right">
        <Link to="/user/bookings">My Bookings</Link>

        {isAuth ? (
          <button onClick={handleLogout} className="logout-link">
            Logout
          </button>
        ) : (
          <Link to="/login">Login</Link>
        )}
      </nav>
    </header>
  );
};

export default Header;
