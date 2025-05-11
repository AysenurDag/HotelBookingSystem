import { useEffect, useState } from "react";
import { useParams }           from "react-router-dom";
import PaymentStatus           from "../../components/payment/PaymentStatus";
import { getPaymentStatus }    from "../../services/paymentService";

export default function PaymentResultPage() {
  const { bookingId } = useParams();
  const [status, setStatus] = useState("Pending");
  const [reason, setReason] = useState(null);
  const [error, setError]   = useState(null);

  useEffect(() => {
    if (!bookingId) return;

    const interval = setInterval(async () => {
      try {
        const data = await getPaymentStatus(bookingId);
        // back-end’den dönen JSON
        setStatus(data.Status);
        setReason(data.Reason);

        if (data.Status !== "Pending") {
          clearInterval(interval);
        }
      } catch (e) {
        setError("Durum alınırken hata oluştu");
        clearInterval(interval);
      }
    }, 1500);

    return () => clearInterval(interval);
  }, [bookingId]);

  if (error) return <p style={{ color: "red" }}>{error}</p>;
  if (!bookingId) return <p> Hangi ödeme belirsiz.</p>;

  return (
    <div>
      <h1>Ödeme Durumu (Rezervasyon #{bookingId})</h1>
      <PaymentStatus status={status} reason={reason} />
    </div>
  );
}
