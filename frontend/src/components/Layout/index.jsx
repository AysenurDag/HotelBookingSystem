// src/components/Layout.js
import React from "react";
import Header from "./Header";
import Footer from "./Footer";
import { msalInstance } from "../../msalInstance";
import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import "./Layout.css";

const Layout = ({ children }) => {
  const isAuth = useIsAuthenticated();
  const { accounts } = useMsal();
  const account = accounts?.[0];

  const handleLogout = () => {
    msalInstance.logoutRedirect({
      postLogoutRedirectUri: "http://localhost:3000",
    });
  };

  return (
    <>
      <Header />

      {/* 👤 Welcome bloğu - tam Header altında */}
      {isAuth && account && (
        <div className="welcome-bar">
          👋 Welcome, <strong>{account.username}</strong>
         
        </div>
      )}

      <main>{children}</main>
      <Footer />
    </>
  );
};

export default Layout;
