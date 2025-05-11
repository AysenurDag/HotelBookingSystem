// src/msalConfig.js
// frontend tenant id :
// 7cbc58ec-5a5e-4808-b100-5cba8de635a3
export const msalConfig = {
  auth: {
    clientId: "7cbc58ec-5a5e-4808-b100-5cba8de635a3", // frontend App Registration ID
    authority: "https://hotelbookingext.ciamlogin.com/1649cae6-96a4-42a7-9f82-3e339752b193", // ✅ CIAM authority
    knownAuthorities: ["hotelbookingext.ciamlogin.com"],
    redirectUri: "http://localhost:3000"
  },
  cache: {
    cacheLocation: "localStorage",
    storeAuthStateInCookie: false
  }
};

// src/msalConfig.js (aynı dosyada)

export const loginRequest = {
  scopes: [
    "api://f6bc9acf-c194-4221-8145-2afc6775bd46/access_as_user"
  ],
  // B2C/CIAM user-flow policy’niz varsa ekleyin:
  extraQueryParameters: { p: "HotelBookingSignUp" }
};
