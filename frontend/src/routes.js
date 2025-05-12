import React from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import Home from "./pages/home";
import Login from "./pages/login";
import Results from "./pages/results";
import Layout from "./components/Layout";
import HotelDetail from './pages/HotelDetail';
import PaymentPage from "./pages/payment/PaymentPage";
import PaymentResultPage from "./pages/payment/PaymentResultPage";

const AppRoutes = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route
          path="/"
          element={
            <Layout>
              <Home />
            </Layout>
          }
        />
        <Route
          path="/login"
          element={
            <Layout>
              <Login />
            </Layout>
          }
        />
        <Route path="/results" element={<Results />} />
        <Route path="/hotel/:id" element={<HotelDetail />} />
        
        {/* Ödeme formu */}
        <Route
          path="/payment/:bookingId"
          element={
            <Layout>
              <PaymentPage />
            </Layout>
          }
        />

        {/* Ödeme sonucu */}
        <Route
          path="/payment/result/:bookingId"
          element={
            <Layout>
              <PaymentResultPage />
            </Layout>
          }
        />






      </Routes>
    </BrowserRouter>
  );
};

export default AppRoutes;
