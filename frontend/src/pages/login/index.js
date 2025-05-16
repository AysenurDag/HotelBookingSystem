// src/pages/login/index.js
import React from "react";
import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../../msalConfig";
import "../login/login.css";

function LoginPage() {
  const { instance } = useMsal();

  const handleLogin = () => {
    instance.loginRedirect(loginRequest);
  };

  return (
    <div className="login-page">
      <div className="login-box">
        <h2>Welcome Back</h2>
        <p>Sign in with your university account to continue</p>
        <button onClick={handleLogin} className="login-button">
          Login with Microsoft Entra ID
        </button>
      </div>
    </div>
  );
}

export default LoginPage;
