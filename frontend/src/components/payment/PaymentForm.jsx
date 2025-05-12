import { useState } from "react";
import "./PaymentForm.css";


function formatCardNumber(value) {
  const digits = value.replace(/\D/g, "").slice(0, 16);
  return digits.match(/.{1,4}/g)?.join(" ") ?? digits;
}


function formatExpiry(value) {
  const digits = value.replace(/\D/g, "").slice(0, 4);
  if (digits.length <= 2) return digits;
  return digits.slice(0, 2) + "/" + digits.slice(2);
}

export default function PaymentForm({ bookingId, customerId, amount, onSubmit }) {
  const [cardNumber, setCardNumber] = useState("");
  const [expiry, setExpiry]         = useState("");
  const [cvv, setCvv]               = useState("");

  const handleSubmit = e => {
    e.preventDefault();
    onSubmit({ bookingId, customerId, amount, cardNumber, expiry, cvv });
  };

  return (
    <div className="card">
      <div className="card-header">
        Ödeme Yap (Rezervasyon #{bookingId})
      </div>

      <div className="card-body">
        {/* --- Summary Column --- */}
        <div className="summary">
         <h2>Ödenecek Tutar</h2>
          <div className="amount">{amount} ₺</div>
          <p>Lütfen kart bilgilerinizi girerek ödemenizi tamamlayın.</p>
        </div>

        {/* --- Form Column --- */}
        <form className="form-container" onSubmit={handleSubmit}>
          {/* Card Number */}
          <div className="form-group">
            <label htmlFor="card-number">Kart Numarası</label>
            <input
              id="card-number"
              type="text"
              maxLength={19}                  // 16 digits + 3 spaces
              placeholder="XXXX XXXX XXXX XXXX"
              value={cardNumber}
              onChange={e => setCardNumber(formatCardNumber(e.target.value))}
              required
            />
          </div>

          {/* Expiry & CVV */}
          <div className="form-group form-group-inline">
            <div style={{ flex: 1 }}>
              <label htmlFor="exp-date">Son Kullanma Tarihi</label>
              <input
                id="exp-date"
                type="text"
                maxLength={5}                // "MM/YY"
                placeholder="MM/YY"
                value={expiry}
                onChange={e => setExpiry(formatExpiry(e.target.value))}
                required
              />
            </div>
            <div style={{ flex: 1 }}>
              <label htmlFor="cvv">CVV</label>
              <input
                id="cvv"
                type="text"
                maxLength={4}
                placeholder="123"
                value={cvv}
                onChange={e => setCvv(e.target.value.replace(/\D/g, ""))}
                required
              />
            </div>
          </div>

          <button type="submit" className="btn-pay">
            Öde {amount} ₺
          </button>
        </form>
      </div>
    </div>
  );
}
