//src/pages/login/index.js
import React from "react";
import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../../msalConfig";

function LoginPage() {
  const { instance } = useMsal();

  const handleLogin = () => {
    instance.loginRedirect(loginRequest);
  };

  return (
    <div className="login-page">
      <h2>Sign In</h2>
      <button onClick={handleLogin}>Login with Microsoft Entra ID</button>
    </div>
  );
}

export default LoginPage;
