import React from "react";
import { Link } from "react-router-dom";
import "./Header.css";

const Header = () => {
  return (
    <header className="modern-header">
      <div className="header-left">
        <Link to="/" className="logo">
          trivago
        </Link>
      </div>
      <nav className="header-right">
        <Link to="/user/bookings">My Bookings</Link>
        <Link to="/login">Login</Link>
      </nav>
    </header>
  );
};

export default Header;
