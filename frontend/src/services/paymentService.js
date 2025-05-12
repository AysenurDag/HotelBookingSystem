import axios from "axios";
const API_BASE = (process.env.REACT_APP_API_URL || "http://localhost:8082") + "/api/payments";

/**
 * Rezervasyona ait ödeme bilgisini çeker.
 */
export async function getPaymentByBooking(bookingId) {
  const res = await axios.get(`${API_BASE}/booking/${bookingId}`);
  return res.data;
}

/**
 * Aynı endpoint kullanılarak status da alınır.
 */
export async function getPaymentStatus(bookingId) {
  return getPaymentByBooking(bookingId);
}

/**
 * Yeni ödeme başlatır. Backend’deki route: POST /api/payments/process
 */
export async function createPayment(payload) {
  const res = await axios.post(`${API_BASE}/process`, payload);
  return res.data;
}






// src/services/paymentService.js

//const BASE_URL = "http://localhost:8082/api/Payments";


// export async function getPaymentByBooking(bookingId) {
//   const res = await fetch(`${BASE}/booking/${bookingId}`);
//   if (!res.ok) throw new Error("Ödeme bilgisi alınamadı");
//   return res.json();
// }

// export async function createPayment(dto) {
//   const res = await fetch(BASE, {
//     method: "POST",
//     headers: { "Content-Type": "application/json" },
//     body: JSON.stringify(dto)
//   });
//   if (!res.ok) {
//     const err = await res.json();
//     throw new Error(err.message || "Ödeme başlatılamadı");
//   }
//   return res.json(); // { bookingId, id, trackUrl }
// }
