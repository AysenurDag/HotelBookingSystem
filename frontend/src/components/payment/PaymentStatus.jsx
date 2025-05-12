
import React from "react";

export default function PaymentStatus({ status, reason }) {
  return (
    <div style={{ textAlign: "center", marginTop: 50 }}>
      {status === "Success" ? (
        <>
          <h2>🎉 Payment Successful!</h2>
          <p>Your booking is confirmed.</p>
        </>
      ) : (
        <>
          <h2>❌ Payment Failed</h2>
          <p>Reason: {reason || "Unknown error"}</p>
        </>
      )}
    </div>
  );
}
