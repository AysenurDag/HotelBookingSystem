import axios from "axios";
import { msalConfig, loginRequest } from "../msalConfig";
import { PublicClientApplication } from "@azure/msal-browser";

// MSAL istemcisi oluştur
const msalInstance = new PublicClientApplication(msalConfig);

// Token al ve API'ye yetkili istek gönder
export async function getCurrentUser() {
  const accounts = msalInstance.getAllAccounts();
  if (accounts.length === 0) throw new Error("No user logged in");

  const response = await msalInstance.acquireTokenSilent({
    ...loginRequest,
    account: accounts[0]
  });

  const token = response.accessToken;

  const res = await axios.get("http://localhost:5289/api/auth/CurrentUser", {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  return res.data;
}

// Örnek: logout işlemi
export async function logoutUser() {
  const accounts = msalInstance.getAllAccounts();
  if (accounts.length === 0) throw new Error("No user logged in");

  const response = await msalInstance.acquireTokenSilent({
    ...loginRequest,
    account: accounts[0]
  });

  const token = response.accessToken;

  await axios.post("http://localhost:5289/api/auth/logout", {}, {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });
}
