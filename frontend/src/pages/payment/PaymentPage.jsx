
import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import PaymentForm from "../../components/payment/PaymentForm";
import { getPaymentByBooking, createPayment } from "../../services/paymentService";


export default function PaymentPage() {
    const { bookingId } = useParams();
    const navigate = useNavigate();

    const [info, setInfo] = useState(null);
    const [loading, setLoad] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        getPaymentByBooking(bookingId)
            .then(data => {
                setInfo(data);
                setLoad(false);
            })
            .catch(err => {
                setError(err.message);
                setLoad(false);
            });
    }, [bookingId]);

    if (loading) return <p>Yükleniyor…</p>;
    if (error) return <p style={{ color: "red" }}>{error}</p>;

    const { amount, customerId } = info;

    const handleSubmit = async cardData => {
        try {
            const { trackUrl } = await createPayment({
                bookingId: Number(bookingId),
                customerId,
                amount,
                ...cardData     // { cardNumber, expiry, cvv }
            });
            navigate(`/payment/result/${bookingId}`, { state: { trackUrl } });
        } catch (e) {
            alert(e.response?.data?.title || e.message);
        }
    };

    return (
        <div>
            <h1>Ödeme Yap (Rezervasyon #{bookingId})</h1>
            <p>Ödenecek Tutar: {amount}₺</p>

            <PaymentForm
                bookingId={Number(bookingId)}
                customerId={customerId}
                amount={amount}
                onSubmit={handleSubmit}
            />
        </div>
    );
}