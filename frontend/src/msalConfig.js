// src/msalConfig.js
export const msalConfig = {
  auth: {
    clientId: "47c088f7-696a-4fa7-a85d-9e3efeb17358", // Senin App Registration clientId’in
    authority: "https://login.microsoftonline.com/1649cae6-96a4-42a7-9f82-3e339752b193", // TenantId'in
    redirectUri: "http://localhost:3000"
  },
  cache: {
    cacheLocation: "localStorage",
    storeAuthStateInCookie: false
  }
};

export const loginRequest = {
  scopes: ["https://hotelbookingexternal.onmicrosoft.com/api/access_as_user"]
};
