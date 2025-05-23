// src/services/api.js
import axios from "axios";
import { loginRequest } from "../msalConfig";
import { msalInstance } from "../msalInstance"; // ✅ sadece buradan import et

// Token al ve API'ye yetkili istek gönder
export async function getCurrentUser() {
  const accounts = msalInstance.getAllAccounts();
  if (!accounts.length) throw new Error("No user logged in");

  const result = await msalInstance.acquireTokenSilent({
    ...loginRequest,
    account: accounts[0],
  });

  const token = result.accessToken;
  // proxy ayarlıysa direkt relative path kullanın:
  const res = await axios.get("http://localhost:5289/api/auth/CurrentUser", {
    headers: { Authorization: `Bearer ${token}` }
  });

  sessionStorage.setItem("userId", res.data.userId);
  console.log("Kullanıcı bilgisi :", res.data.userId);
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
