import React from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import Home from "./pages/home";
import Login from "./pages/login";
import Results from "./pages/results";
import Layout from "./components/Layout";
import HotelDetail from './pages/HotelDetail';

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
      </Routes>
    </BrowserRouter>
  );
};

export default AppRoutes;
