// src/routes.js
import React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Home from "./pages/home";
import Login from "./pages/login";
import Results from "./pages/results";
import Layout from "./components/Layout";
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

        {/* 2) Ana sayfa – artık public */}
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

        {/* 4) Bilinmeyen route’lar → ana sayfaya */}
        <Route path="*" element={<Navigate to="/" replace />} />

      </Routes>
    </BrowserRouter>
  );
}
