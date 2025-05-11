
import axios from "axios";

const API_BASE = process.env.REACT_APP_API_URL || "http://localhost:8082";

export function createPayment(data) {

  return axios
    .post(`${API_BASE}/api/payments`, data)
    .then(res => res.data);
}

export function getPaymentByBooking(bookingId) {
  return axios
    .get(`${API_BASE}/api/Payments/booking/${bookingId}`)
    .then(res => res.data);
}

export function getPaymentStatus(bookingId) {
  // aynı endpoint’i kullanabilirsiniz
  return getPaymentByBooking(bookingId);
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

