import { useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import PaymentForm from "../../components/payment/PaymentForm";
import { createBooking } from '../../services/bookingService';

export default function PaymentPage() {
    const navigate = useNavigate();
    const location = useLocation();
    const bookingData = location.state?.bookingData;

    const [bookingId, setBookingId] = useState(null);
    const [info, setInfo] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const handleSubmit = async (cardData) => {
        try {
            console.log("🚀 Gönderilen bookingData:", bookingData, typeof bookingData);

            if (!bookingData) {
                setError("Rezervasyon verisi bulunamadı.");
                return;
            }

            setLoading(true);

            const result = await createBooking(bookingData);
            if (result.error) {
                alert(result.error);
                return;
            }

            const newBookingId = result.bookingId;
            setBookingId(newBookingId);

            navigate(`/payment/result/${newBookingId}`);
        } catch (e) {
            setError(e.response?.data?.title || e.message);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div>
            <h1>Ödeme Yap</h1>
            {loading && <p>İşleniyor...</p>}
            {error && <p style={{ color: "red" }}>{error}</p>}

            <PaymentForm
                bookingId={bookingId}
                customerId={info?.customerId}
                amount={info?.amount}
                onSubmit={handleSubmit}
            />
        </div>
    );
}
