import React, { useState } from 'react';
import { loginUser } from '../api/user-api';
import { saveTokens } from '../helpers/auth-token-helper';
import { useNavigate } from 'react-router-dom';

function Login() {
  const [Username, setUsername] = useState('');
  const [PasswordString, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    setSuccess(false);

    try {
      const { ok, data } = await loginUser({ Username, PasswordString });

      // OAuth 2.0 /token response: access_token, refresh_token, expires_in, userName, userId, fullName
      if (ok && data.access_token) {
        // Save tokens (AuthToken, token, RefreshToken, TokenExpiry) and related info in localStorage
        saveTokens(data);
        localStorage.setItem('Username', data.userName);
        localStorage.setItem('UserId', data.userId);

        localStorage.setItem('userId', data.userId);
        localStorage.setItem('username', data.userName);
        localStorage.setItem('fullName', data.fullName); // This is [First Name] [Last Name]

        setSuccess(true);

        

        // Delay redirect slightly for user feedback
        setTimeout(() => navigate('/Applications'), 2000);
      } else {
        setError(data.error_description || 'Invalid username or password.');
      }
    } catch (err) {
      console.error(err);
      setError('An error occurred during login.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      {/* The Header is now handled globally by App.jsx */}
      <div className="login-card">
        <h2>Login</h2>
        <form onSubmit={handleLogin}>
          <div className="form-group">
            <label>Username:</label>
            <input
              type="text"
              value={Username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Enter username"
              required
            />
          </div>
          <div className="form-group">
            <label>Password:</label>
            <input
              type="password"
              value={PasswordString}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter password"
              required
            />
          </div>
          <button type="submit" className="login-btn" disabled={loading}>
            {loading ? 'Logging in...' : 'Login'}
          </button>
        </form>

        {error && <p style={{ color: '#d9534f', marginTop: '10px', fontWeight: 'bold' }}>{error}</p>}
        {success && <p style={{ color: '#5cb85c', marginTop: '10px', fontWeight: 'bold' }}>Login successful! Redirecting...</p>}
      </div>
    </div>
  );
}

export default Login;
