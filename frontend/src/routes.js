import React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Home from "./pages/home";
import Login from "./pages/login";
import Results from "./pages/results";
import Layout from "./components/Layout";
import HotelDetail from './pages/HotelDetail';
import PaymentPage from "./pages/payment/PaymentPage";
import PaymentResultPage from "./pages/payment/PaymentResultPage";
import ProtectedRoute from "./components/ProtectedRoute";

export default function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>

        {/* 1) Login sayfası */}
        <Route
          path="/login"
          element={
            <Layout>
              <Login />
            </Layout>
          }
        />

        {/* 2) Ana sayfa – public */}
        <Route
          path="/"
          element={
            <Layout>
              <Home />
            </Layout>
          }
        />

        {/* 3) Sonuçlar sayfası – korumalı */}
        <Route
          path="/results"
          element={
            <ProtectedRoute>
              <Layout>
                <Results />
              </Layout>
            </ProtectedRoute>
          }
        />

        {/* 4) Hotel detay */}
        <Route
          path="/hotel/:id"
          element={
            <Layout>
              <HotelDetail />
            </Layout>
          }
        />

        {/* 5) Ödeme formu */}
        <Route
          path="/payment"
          element={
             <ProtectedRoute>
                <Layout>
              <PaymentPage />
            </Layout>
               </ProtectedRoute>
          }
        />

        {/* 6) Ödeme sonucu */}
        <Route
          path="/payment/result/:bookingId"
          element={
            <ProtectedRoute>
              <Layout>
                <PaymentResultPage />
              </Layout>
            </ProtectedRoute>
          }
        />

        {/* 7) Bilinmeyen route’lar → ana sayfaya */}
        <Route path="*" element={<Navigate to="/" replace />} />
        
      </Routes>
    </BrowserRouter>
  );
}
