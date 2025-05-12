import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import PaymentStatus from "../../components/payment/PaymentStatus";
import { getPaymentStatus } from "../../services/paymentService";

export default function PaymentResultPage() {
  const { bookingId } = useParams();
  const [status, setStatus] = useState("Pending");
  const [reason, setReason] = useState("");

  useEffect(() => {
    if (!bookingId) return;

    const interval = setInterval(async () => {
      try {
        const data = await getPaymentStatus(bookingId);
        // Backend JSON camelCase: data.status, data.refundReason
        setStatus(data.status);
        setReason(data.refundReason || data.reason || "");

        if (data.status !== "Pending") {
          clearInterval(interval);
        }
      } catch (e) {
        setReason("Durum alınırken hata oluştu");
        clearInterval(interval);
      }
    }, 1500);

    return () => clearInterval(interval);
  }, [bookingId]);

  if (!bookingId) {
    return <p>Hangi ödeme belirsiz.</p>;
  }

  return (
    <div>
      <h1>Ödeme Durumu (Rezervasyon #{bookingId})</h1>
      <PaymentStatus status={status} reason={reason} />
    </div>
  );
}