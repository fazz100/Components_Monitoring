import React from 'react';
import { Navigate } from 'react-router-dom';
//import { isTokenValid } from '../helpers/auth-token-helper'; // adjust path as needed

function ProtectedRoute({ children }) {
    /*
  if (!isTokenValid()) {
    // Token is missing or expired — clear storage and go to login
    localStorage.clear();
    return <Navigate to="/" replace />;
  }*/

  // Token is still valid — allow access
  return children;
}

export default ProtectedRoute;