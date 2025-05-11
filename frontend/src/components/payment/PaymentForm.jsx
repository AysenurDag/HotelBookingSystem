import { useState } from "react";

export default function PaymentForm({ bookingId, customerId, amount, onSubmit }) {
  const [cardNumber, setCardNumber] = useState("");
  const [expiry, setExpiry] = useState("");
  const [cvv, setCvv] = useState("");

  const handle = e => {
    e.preventDefault();
    onSubmit({ bookingId, customerId, amount, cardNumber, expiry, cvv });
  };

  return (
    <form onSubmit={handle}>
      <div>
        <label>Kart Numarası</label>
        <input
          type="text"
          maxLength={19}
          value={cardNumber}
          onChange={e => setCardNumber(e.target.value)}
          required
        />
      </div>
      <div>
        <label>Son Kullanma Tarihi (MM/YY)</label>
        <input
          type="text"
          placeholder="MM/YY"
          value={expiry}
          onChange={e => setExpiry(e.target.value)}
          required
        />
      </div>
      <div>
        <label>CVV</label>
        <input
          type="text"
          maxLength={4}
          value={cvv}
          onChange={e => setCvv(e.target.value)}
          required
        />
      </div>
      <button type="submit">Öde {amount} ₺</button>
    </form>
  );
}
