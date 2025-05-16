// src/components/ProtectedRoute.js
import React from 'react';
import { Navigate } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';

export default function ProtectedRoute({ children }) {
  const isAuthenticated = useIsAuthenticated();
  return isAuthenticated
    ? children
    : <Navigate to="/login" replace />;
}
